using CaptainHook.Domain.Results;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class CliError : ErrorBase
    {
        public CliError(string message, params IFailure[] failures) : base(message, failures)
        {
        }
    }
}