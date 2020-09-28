using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class ConsoleSubscriberWriter : IConsoleSubscriberWriter
    {
        private readonly IConsole _console;

        public ConsoleSubscriberWriter(IConsole console)
        {
            _console = console;
        }

        public void WriteNormal(params string[] lines) => WriteInColor(_console.ForegroundColor, lines);

        public void WriteSuccess(params string[] lines) => WriteInColor(ConsoleColor.DarkGreen, lines);

        public void WriteError(params string[] lines) => WriteInColor(ConsoleColor.Red, lines);

        private void WriteBox(int length) => _console.WriteLine(new string('=', length));

        private void WriteInColor(ConsoleColor color, params string[] lines)
        {
            Action writeBox = null;

            if (lines.Length > 1 && string.Equals("box", lines[0], StringComparison.OrdinalIgnoreCase))
            {
                var longestLine = lines.Skip(1).Max(l => l.Length);
                writeBox = () => WriteBox(longestLine);
            }

            var line = string.Join(Environment.NewLine, writeBox == null ? lines : lines.Skip(1));
            _console.ForegroundColor = color;

            writeBox?.Invoke();
            _console.WriteLine(line);
            writeBox?.Invoke();
            _console.ResetColor();
        }
    }
}