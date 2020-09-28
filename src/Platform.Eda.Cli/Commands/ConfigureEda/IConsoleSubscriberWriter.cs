namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IConsoleSubscriberWriter
    {
        public void WriteNormal(params string[] lines);

        public void WriteSuccess(params string[] lines);

        public void WriteError(params string[] lines);
    }
}