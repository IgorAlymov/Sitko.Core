using Microsoft.EntityFrameworkCore;

namespace Sitko.Core.Db.InMemory
{
    public class InMemoryDatabaseModuleConfig<TDbContext> : BaseDbModuleConfig<TDbContext> where TDbContext : DbContext
    {
    }
}