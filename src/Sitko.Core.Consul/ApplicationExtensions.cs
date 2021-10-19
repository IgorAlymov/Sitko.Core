﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sitko.Core.App;

namespace Sitko.Core.Consul
{
    public static class ApplicationExtensions
    {
        public static Application AddConsul(this Application application,
            Action<IConfiguration, IHostEnvironment, ConsulModuleOptions> configure, string? optionsKey = null) =>
            application.AddModule<ConsulModule, ConsulModuleOptions>(configure, optionsKey);

        public static Application AddConsul(this Application application,
            Action<ConsulModuleOptions>? configure = null, string? optionsKey = null) =>
            application.AddModule<ConsulModule, ConsulModuleOptions>(configure, optionsKey);
    }
}
