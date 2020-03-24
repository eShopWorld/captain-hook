using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using FluentAssertions;
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

            //var payloadId = _fixture.PublishModel(new HookFlowTestEvent());
            var processedEvents = await _fixture.GetProcessedEvents("blah");

            processedEvents.Should().OnlyContain((m) => m.Verb == "POST" && m.Url.EndsWith("intake"));
        }
    }
}
