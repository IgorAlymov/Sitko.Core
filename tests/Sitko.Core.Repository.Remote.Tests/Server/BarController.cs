﻿using System;
using Microsoft.AspNetCore.Mvc;
using Sitko.Core.Repository.Remote.Server;
using Sitko.Core.Repository.Tests.Data;

namespace Sitko.Core.Repository.Remote.Tests.Server;

[Route("/api/BarModel")]
public class BarController : BaseRemoteRepositoryController<BarModel, Guid>
{
    public BarController(BarEFRepository repository) : base(repository)
    {
    }
}
