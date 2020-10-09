using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Platform.Eda.Cli.Extensions
{
    internal static class CommandOptionsListExtensions
    {
        internal static string[] ToConsoleStrings(this CommandLineApplication command)
        {
            var text = new[]
            {
                $"Command: '{command?.Name}'",
                "Options:"
            }.Concat(command?.Options?.Select(ConvertToText) ?? new[] {"No options defined"});

            return text.ToArray();
        }

        private static string ConvertToText(CommandOption option)
        {
            string JoinValues() => string.Join(";", option.Values);

            var valueToPrint = option.OptionType switch
            {
                CommandOptionType.NoValue => (option.HasValue() ? "is SET" : "is NOT SET"),
                CommandOptionType.SingleOrNoValue => (option.HasValue() ? $"'{JoinValues()}'" : "is NOT SET"),
                _ => $"'{JoinValues()}'"
            };

            return $"--{option.LongName}: {valueToPrint}";
        }
    }
}
