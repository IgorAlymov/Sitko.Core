using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sitko.Core.App;
using Sitko.Core.App.Helpers;
using Sitko.Core.Grpc.Server.Discovery;
using Tempus;

namespace Sitko.Core.Grpc.Server.Consul
{
    public class ConsulGrpcServicesRegistrar : IGrpcServicesRegistrar, IAsyncDisposable
    {
        private readonly IOptionsMonitor<GrpcServerConsulModuleConfig> _optionsMonitor;
        private readonly IApplication _application;
        private readonly IConsulClient? _consulClient;
        private readonly string _host = "127.0.0.1";
        private readonly bool _inContainer = DockerHelper.IsRunningInDocker();
        private readonly ILogger<ConsulGrpcServicesRegistrar> _logger;
        private GrpcServerConsulModuleConfig Options => _optionsMonitor.CurrentValue;
        private readonly int _port;

        private readonly ConcurrentDictionary<string, string> _registeredServices = new();

        private bool _disposed;
        private IScheduledTask? _updateTtlTask;

        public ConsulGrpcServicesRegistrar(IOptionsMonitor<GrpcServerConsulModuleConfig> optionsMonitor,
            IApplication application,
            IServer server, IScheduler scheduler, ILogger<ConsulGrpcServicesRegistrar> logger,
            IConsulClient? consulClient = null)
        {
            _optionsMonitor = optionsMonitor;
            _application = application;
            _consulClient = consulClient;
            _logger = logger;
            if (!string.IsNullOrEmpty(Options.Host))
            {
                _logger.LogInformation("Use grpc host from config");
                _host = Options.Host;
            }
            else if (_inContainer)
            {
                _logger.LogInformation("Use docker ip as grpc host");
                var dockerIp = DockerHelper.GetContainerAddress();
                if (string.IsNullOrEmpty(dockerIp))
                {
                    throw new Exception("Can't find host ip for grpc");
                }

                _host = dockerIp;
            }

            _logger.LogInformation("GRPC Host: {Host}", _host);
            if (Options.Port != null && Options.Port > 0)
            {
                _logger.LogInformation("Use grpc port from config");
                _port = Options.Port.Value;
            }
            else
            {
                var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                var address = serverAddressesFeature.Addresses.Select(a => new Uri(a))
                    .FirstOrDefault(u => u.Scheme == "https");
                if (address == null)
                {
                    throw new Exception("Can't find https address for grpc service");
                }

                _port = address.Port > 0 ? address.Port : 443;
            }

            _logger.LogInformation("GRPC Port: {Port}", _port);
            //_updateTtlTask = UpdateChecksAsync(_updateTtlCts.Token);
            _updateTtlTask = scheduler.Schedule(TimeSpan.FromSeconds(15), async token =>
            {
                await UpdateServicesTtlAsync(token);
            }, (context, _) =>
            {
                _logger.LogError(context.Exception, "Error updating TTL for gRPC services: {ErrorText}",
                    context.Exception.ToString());
                return Task.CompletedTask;
            });
        }


        public async ValueTask DisposeAsync()
        {
            if (!_disposed && _consulClient != null)
            {
                if (_updateTtlTask != null)
                {
                    await _updateTtlTask.Cancel();
                    _updateTtlTask = null;
                }

                foreach (var registeredService in _registeredServices)
                {
                    _logger.LogInformation(
                        "Application stopping. Deregister grpc service {ServiceName} on {Address}:{Port}",
                        registeredService.Value, _host,
                        _port);
                    await _consulClient.Agent.ServiceDeregister(registeredService.Key);
                }

                _disposed = true;
            }
        }

        public async Task RegisterAsync<T>() where T : class
        {
            var serviceName = GetServiceName<T>();
            var id = GetServiceId<T>();
            if (_consulClient != null)
            {
                var registration = new AgentServiceRegistration
                {
                    ID = id,
                    Name = serviceName,
                    Address = _host,
                    Port = _port,
                    Check = new AgentServiceCheck
                    {
                        TTL = Options.ChecksInterval, DeregisterCriticalServiceAfter = Options.DeregisterTimeout
                    },
                    Tags = new[] {"grpc", $"version:{_application.Version}"}
                };
                _logger.LogInformation("Register grpc service {ServiceName} on {Address}:{Port}", serviceName, _host,
                    _port);
                await _consulClient.Agent.ServiceDeregister(id);
                var result = await _consulClient.Agent.ServiceRegister(registration);
                _logger.LogInformation("Consul response code: {Code}", result.StatusCode);
            }

            _registeredServices.TryAdd(id, serviceName);
        }

        public async Task<HealthCheckResult> CheckHealthAsync<T>(CancellationToken cancellationToken = default)
            where T : class
        {
            if (_consulClient == null)
            {
                return HealthCheckResult.Unhealthy("No consul client");
            }

            var id = GetServiceId<T>();
            var serviceName = GetServiceName<T>();

            var serviceResponse = await _consulClient.Catalog.Service(serviceName, "grpc", cancellationToken);
            if (serviceResponse.StatusCode == HttpStatusCode.OK)
            {
                if (serviceResponse.Response.Any())
                {
                    if (serviceResponse.Response.Any(service => service.ServiceID == id))
                    {
                        return HealthCheckResult.Healthy();
                    }

                    return HealthCheckResult.Degraded($"Service {serviceName} exists but with another id");
                }

                if (Options.AutoFixRegistration)
                {
                    //no services. fix registration
                    await RegisterAsync<T>();
                }

                return HealthCheckResult.Degraded($"No grpc service registered with name {serviceName}");
            }

            return HealthCheckResult.Unhealthy($"Error response from consul: {serviceResponse.StatusCode}");
        }

        private string GetServiceName<T>()
        {
            var serviceName = typeof(T).BaseType?.DeclaringType?.Name;
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception($"Can't find service name for {typeof(T)}");
            }

            return serviceName;
        }

        private string GetServiceId<T>()
        {
            var serviceName = GetServiceName<T>();
            return _inContainer ? $"{serviceName}_{_host}_{_port}" : serviceName;
        }

        private async Task UpdateServicesTtlAsync(CancellationToken token)
        {
            if (!token.IsCancellationRequested && _consulClient != null && _registeredServices.Any())
            {
                _logger.LogDebug("Update TTL for gRPC services");
                foreach (var service in _registeredServices)
                {
                    _logger.LogDebug("Service: {ServiceId}/{ServiceName}", service.Key, service.Value);
                    try
                    {
                        await _consulClient.Agent.UpdateTTL("service:" + service.Key,
                            $"Last update: {DateTime.UtcNow:O}", TTLStatus.Pass,
                            token);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Error updating TTL for {ServiceId}/{ServiceName}: {ErrorText}",
                            service.Key, service.Value, exception.ToString());
                    }
                }

                _logger.LogDebug("All gRPC services TTL updated");
            }
        }
    }
}
