namespace CaptainHook.Cli.Commands.GeneratePowerShell.Internal
{
    internal class PsCommand
    {
        private readonly string name;
        private readonly object value;
        private readonly bool withoutQuotes;

        public PsCommand(string name, object value, bool withoutQuotes = false)
        {
            this.name = name;
            this.value = value;
            this.withoutQuotes = withoutQuotes;
        }

        public override string ToString()
        {
            var valueText = withoutQuotes ? value : $"'{value}'";
            return $"setConfig '{name}' {valueText} $KeyVault";
        }
    }
}