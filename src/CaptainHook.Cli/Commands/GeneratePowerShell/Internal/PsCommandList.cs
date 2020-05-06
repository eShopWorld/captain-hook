using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Cli.Commands.GeneratePowerShell.Internal
{
    internal class PsCommandList
    {
        private readonly List<PsCommand> commands = new List<PsCommand>();

        public void Add(string name, object value, bool withoutQuotes = false)
        {
            commands.Add(new PsCommand(name, value, withoutQuotes));
        }

        public void Add(string name, IEnumerable<string> strings)
        {
            if (strings == null || !strings.Any())
            {
                Add(name, string.Empty);
            }
            else
            {
                Add(name, string.Join(',', strings));
            }
        }

        public IEnumerable<string> ToCommandLines()
        {
            return commands.Select(c => c.ToString());
        }
    }
}