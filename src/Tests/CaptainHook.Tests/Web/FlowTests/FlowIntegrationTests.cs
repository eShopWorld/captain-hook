using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    [Collection(nameof(E2EFlowTestsCollection))]
    public class FlowIntegrationTests
    {
        private readonly E2EFlowTestsFixture _fixture;

        public FlowIntegrationTests(E2EFlowTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, IsLayer1]
        public async Task BasicFlow()
        {
            await _fixture.RunMessageFlow(new HookFlowTestEvent(),
                builder => builder.CheckOidcAuthScopes("eda.peterpan.delivery.api.all").CheckUrl());
        }
    }
}
