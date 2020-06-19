using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public class EventTests : GenerateJsonCommandBase
    {
        public EventTests(ITestOutputHelper output): base(output)
        {
        }

        [Fact, IsLayer0]
        public async Task NameIsCorrect()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonResult["Name"].ToString()
                .Should().Be("activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent");
        }

        [Fact, IsLayer0]
        public async Task TypeIsCorrect()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonResult["Type"].ToString()
                .Should().Be("activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent");
        }

        [Fact, IsLayer0]
        public async Task HasWebhookConfigSection()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonResult["WebhookConfig"]
                .Should().BeOfType<JObject>();
        }

        [Fact, IsLayer0]
        public async Task HasCallbackConfigSection()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonResult["CallbackConfig"]
                .Should().BeOfType<JObject>();
        }

        [Fact, IsLayer0]
        public async Task SupportsHeartBeatConfig()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonResult["HeartBeatInterval"]
                .Should().BeOfType<JValue>().Which.Value<string>().Should().Be("00:00:05");
        }
    }
}
