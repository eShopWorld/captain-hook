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
    public class AuthenticationConfigTests : GenerateJsonCommandBase
    {
        private IEnumerable<JToken> AllAuthConfigSections => JsonResult.SelectTokens("$..AuthenticationConfig");

        public AuthenticationConfigTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsLayer0]
        public async Task HasValidType()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            AllAuthConfigSections
                .Should().Contain(x => x["Type"].ToString().Length > 0);
        }

        [Fact]
        [IsLayer0]
        public async Task IsValidOidc()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            AllAuthConfigSections
                .Where(x => x.Value<string>("Type") == "OIDC")
                .Should()
                .Contain(x =>
                    x.Value<string>("ClientId").Length > 0 &&
                    x.Value<string>("Uri").Length > 0 &&
                    x.Value<string>("ClientSecret").Length > 0 &&
                    x["Scopes"] is JArray);
        }
    }
}
