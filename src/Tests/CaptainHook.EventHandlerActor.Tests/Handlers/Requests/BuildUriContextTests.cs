using System.Collections.Generic;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.EventHandlerActor.Tests.Handlers.Requests
{
    public class BuildUriContextTests
    {
        [Fact, IsUnit]
        public void ApplyReplace_MultipleReplacementsWithVariousCasing_ValidReplaceHappens()
        {
            // Arrange
            var uriWithReplacements = "https://abc-{selector}.com/{OrderNumber}";
            var replacements = new Dictionary<string, string>
            {
                { "selector", "def" },
                { "ordernumber", "AB123" }
            };

            // Act
            var result = new BuildUriContext(uriWithReplacements, a => { })
                .ApplyReplace(replacements)
                .CheckIfRoutableAndReturn();

            // Assert
            result.Should().Be("https://abc-def.com/AB123");
        }

        [Fact, IsUnit]
        public void ApplyReplace_InsufficientReplacements_NoReplaceHappens()
        {
            // Arrange
            var uriWithReplacements = "https://abc-{selector}.com/{OrderNumber}";
            var replacements = new Dictionary<string, string>
            {
                { "selector", "def" },
            };

            // Act
            var result = new BuildUriContext(uriWithReplacements, a => { })
                .ApplyReplace(replacements)
                .CheckIfRoutableAndReturn();

            // Assert
            result.Should().BeNull();
        }
    }
}