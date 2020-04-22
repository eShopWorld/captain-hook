using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Cli.Commands.GeneratePowerShell;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Cli.Tests.GeneratePowerShell
{
    public class EventHandlerConfigToPsConverterTests
    {
        [Fact, IsLayer0]
        public async Task ValidEventHandlerConfig_ConvertedCorrectly()
        {
            var converter = new EventHandlerConfigToPowerShellConverter();

            var eventHandlerConfig = new EventHandlerConfig
            {

            };

            var result = await converter.Convert(eventHandlerConfig);

        }
    }
}
