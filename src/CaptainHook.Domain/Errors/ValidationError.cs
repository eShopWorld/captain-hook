using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ValidationError : ErrorBase
    {
        public ValidationError(string message, params ValidationFailure[] failures) : base(message, failures)
        {
        }
    }
}