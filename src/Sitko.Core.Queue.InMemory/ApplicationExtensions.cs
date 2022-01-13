﻿using System;
using Microsoft.Extensions.Configuration;
using Sitko.Core.App;

namespace Sitko.Core.Queue.InMemory;

public static class ApplicationExtensions
{
    public static Application AddInMemoryQueue(this Application application,
        Action<IConfiguration, IAppEnvironment, InMemoryQueueModuleOptions> configure,
        string? optionsKey = null) =>
        application.AddModule<InMemoryQueueModule, InMemoryQueueModuleOptions>(configure, optionsKey);

    public static Application AddInMemoryQueue(this Application application,
        Action<InMemoryQueueModuleOptions>? configure = null,
        string? optionsKey = null) =>
        application.AddModule<InMemoryQueueModule, InMemoryQueueModuleOptions>(configure, optionsKey);
}
