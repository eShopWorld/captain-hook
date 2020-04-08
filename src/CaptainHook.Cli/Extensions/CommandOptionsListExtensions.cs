using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Cli.Extensions
{
    internal static class CommandOptionsListExtensions
    {
        internal static string ToConsoleString(this IList<CommandOption> list) =>
                list != null && list.Any()
                    ? string.Join(',', list.Select(t => $"{t.LongName}-'{t.Value()}'"))
                    : string.Empty;
    }
}
