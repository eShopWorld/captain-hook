using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class MappingError : ErrorBase
    {
        public MappingError(string message, params IFailure[] failures) : base(message, failures)
        {
        }
    }
}
