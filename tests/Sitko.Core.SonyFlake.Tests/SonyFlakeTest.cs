using System.Threading.Tasks;
using Sitko.Core.IdProvider;
using Sitko.Core.IdProvider.SonyFlake;
using Sitko.Core.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Sitko.Core.SonyFlake.Tests
{
    public class SonyFlakeTest : BaseTest<SonyFlakeTestScope>
    {
        public SonyFlakeTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Generate()
        {
            var scope = await GetScopeAsync();

            var provider = scope.Get<IIdProvider>();

            var id = await provider.NextAsync();

            Assert.True(id > 0);
        }
    }

    public class SonyFlakeTestScope : BaseTestScope
    {
        protected override TestApplication ConfigureApplication(TestApplication application, string name)
        {
            return base.ConfigureApplication(application, name).AddModule<SonyFlakeModule, SonyFlakeModuleConfig>(
                (configuration, _, moduleConfig) =>
                    moduleConfig.SonyflakeUri = configuration["SONYFLAKE_URI"]);
        }
    }
}