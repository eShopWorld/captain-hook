using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Platform.Eda.Cli.Extensions
{
    internal static class CommandOptionsListExtensions
    {
        internal static string[] ToConsoleStrings(this IEnumerable<CommandOption> list) =>
            list?.Select(t => $"--{t.LongName}: '{t.Value()}'").ToArray();
    }
}
