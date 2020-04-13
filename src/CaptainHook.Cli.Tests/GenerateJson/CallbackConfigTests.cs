using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public class CallbackConfigTests : GenerateJsonCommandBase
    {
        private JToken JsonToken => JsonResult["CallbackConfig"];

        public CallbackConfigTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsLayer0]
        public async Task NameIsCorrect()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonToken["Name"].ToString()
                .Should().Be("activity1.domain.infrastructure.domainevents.activityorderconfirmationdomainevent-callback");
        }

        [Fact, IsLayer0]
        public async Task HasRequestRulesSection()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonToken["WebhookRequestRules"]
                .Should().BeOfType<JArray>().And.NotBeEmpty();
        }

        [Fact, IsLayer0]
        public async Task HasHttpVerb()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonToken["HttpVerb"].ToString()
                .Should().NotBeNullOrWhiteSpace();
        }
    }
}
