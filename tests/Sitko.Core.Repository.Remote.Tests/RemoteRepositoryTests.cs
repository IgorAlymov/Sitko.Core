﻿using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sitko.Core.Repository.Remote.Tests.Data;
using Sitko.Core.Repository.Tests;
using Sitko.Core.Repository.Tests.Data;
using Xunit;
using Xunit.Abstractions;

namespace Sitko.Core.Repository.Remote.Tests;

public class RemoteRepositoryTests : BasicRepositoryTests<RemoteRepositoryTestScope>
{
    public RemoteRepositoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task GetAll()
    {
        var scope = await GetScopeAsync();
        var repo = scope.GetService<TestRepository>();
        var result = await repo.GetAllAsync(q => q.Where(t=>t.Status == TestStatus.Enabled));

        Assert.NotNull(result.items);
    }

}
