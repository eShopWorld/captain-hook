using System.Net.Http;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    /// <summary>
    /// E2E integration tests for various identified flows - using peter pan
    /// </summary>
    [Collection(nameof(E2EFlowTestsCollection))]
    public class FlowIntegrationTests
    {
        private readonly E2EFlowTestsFixture _fixture;

        public FlowIntegrationTests(E2EFlowTestsFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// no routing rules
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        [Fact, IsLayer2]
        public async Task BasicWebHookFlowAuthNoRulesPostVerb()
        {
            await _fixture.RunMessageFlow(new WebHookFlowTestEvent(),
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
        }
    }
}
