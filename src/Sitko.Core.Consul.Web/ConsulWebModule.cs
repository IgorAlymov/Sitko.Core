using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sitko.Core.App;

namespace Sitko.Core.Consul.Web
{
    public class ConsulWebModule : BaseApplicationModule<ConsulWebModuleConfig>
    {
        public ConsulWebModule(ConsulWebModuleConfig config, Application application) : base(config, application)
        {
        }

        public override List<Type> GetRequiredModules()
        {
            return new List<Type> {typeof(ConsulModule)};
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddSingleton<ConsulWebClient>();
            services.AddHealthChecks().AddCheck<ConsulWebHealthCheck>("Consul registration");
        }

        public override Task ApplicationStarted(IConfiguration configuration, IHostEnvironment environment,
            IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetRequiredService<ConsulWebClient>();
            return client.RegisterAsync();
        }

        public override async Task ApplicationStopping(IConfiguration configuration, IHostEnvironment environment,
            IServiceProvider serviceProvider)
        {
            var consulClient = serviceProvider.GetRequiredService<IConsulClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<ConsulWebModule>>();
            logger.LogInformation("Remove service from Consul");
            await consulClient.Agent.ServiceDeregister(environment.ApplicationName);
        }
    }
}
