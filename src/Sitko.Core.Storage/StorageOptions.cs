using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitko.Core.Storage.Cache;
using Sitko.Core.Storage.Metadata;

namespace Sitko.Core.Storage
{
    public abstract class StorageOptions
    {
        public Uri? PublicUri { get; set; }

        public string? Prefix { get; set; }

        public Action<IHostEnvironment, IConfiguration, IServiceCollection>? ConfigureCache { get; protected set; }
        public Action<IHostEnvironment, IConfiguration, IServiceCollection>? ConfigureMetadata { get; protected set; }

        public StorageOptions EnableCache<TCache, TCacheOptions>(Action<TCacheOptions>? configure = null)
            where TCache : class, IStorageCache<TCacheOptions> where TCacheOptions : StorageCacheOptions
        {
            ConfigureCache = (_, _, services) =>
            {
                var options = Activator.CreateInstance<TCacheOptions>();
                configure?.Invoke(options);
                services.AddSingleton(options);
                services.AddSingleton<IStorageCache, TCache>();
            };
            return this;
        }

        public StorageOptions EnableMetadata<TMetadataProvider, TMetadataProviderOptions>(
            Action<TMetadataProviderOptions>? configure = null)
            where TMetadataProvider : class, IStorageMetadataProvider<TMetadataProviderOptions>
            where TMetadataProviderOptions : StorageMetadataProviderOptions
        {
            ConfigureMetadata = (_, _, services) =>
            {
                var options = Activator.CreateInstance<TMetadataProviderOptions>();
                configure?.Invoke(options);
                services.AddSingleton(options);
                services.AddSingleton<IStorageMetadataProvider, TMetadataProvider>();
            };
            return this;
        }
    }
}
