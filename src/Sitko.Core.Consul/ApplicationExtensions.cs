﻿using System;
using Microsoft.Extensions.Configuration;
using Sitko.Core.App;

namespace Sitko.Core.Consul;

public static class ApplicationExtensions
{
    public static Application AddConsul(this Application application,
        Action<IConfiguration, IAppEnvironment, ConsulModuleOptions> configure, string? optionsKey = null) =>
        application.AddModule<ConsulModule, ConsulModuleOptions>(configure, optionsKey);

    public static Application AddConsul(this Application application,
        Action<ConsulModuleOptions>? configure = null, string? optionsKey = null) =>
        application.AddModule<ConsulModule, ConsulModuleOptions>(configure, optionsKey);
}
