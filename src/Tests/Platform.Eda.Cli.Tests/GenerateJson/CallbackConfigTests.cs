﻿using Eshopworld.Tests.Core;
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

        [Fact, IsUnit]
        public async Task NameIsCorrect()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonToken["Name"].ToString()
                .Should().Be("activity1.domain.infrastructure.domainevents.activityorderconfirmationdomainevent-callback");
        }

        [Fact, IsUnit]
        public async Task HasRequestRulesSection()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            JsonToken["WebhookRequestRules"]
                .Should().BeOfType<JArray>().And.NotBeEmpty();
        }
    }
}
