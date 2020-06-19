using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public class WebhookRequestRulesTests: GenerateJsonCommandBase
    {
        private IEnumerable<JToken> AllWebhookRequestRules => JsonResult.SelectTokens("$..WebhookRequestRules[*]");
        
        public WebhookRequestRulesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsLayer0]
        public async Task HasSource()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            AllWebhookRequestRules.Should().Contain(x => x["Source"] is JObject);
        }

        [Fact, IsLayer0]
        public async Task HasDestination()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            AllWebhookRequestRules.Should().Contain(x => x["Destination"] is JObject);
        }

        [Fact, IsLayer0]
        public async Task HasRoutes()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            new JArray(AllWebhookRequestRules)
                .SelectTokens("[?(@.Destination.RuleAction=='Route')]")
                .Should().Contain(x => x["Routes"].Count() > 0);
        }
    }
}
