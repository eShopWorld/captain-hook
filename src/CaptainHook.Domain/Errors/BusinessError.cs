using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class BusinessError : ErrorBase
    {
        public BusinessError(string message, params Failure[] failures) : base(message, failures)
        {
        }
    }
}