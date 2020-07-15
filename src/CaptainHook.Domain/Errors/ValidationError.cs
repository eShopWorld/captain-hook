using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ValidationError : ErrorBase
    {
        public ValidationError(string message, params Failure[] failures) : base(message, failures)
        {
        }
    }
}