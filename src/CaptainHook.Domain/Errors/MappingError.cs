using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class MappingError : ErrorBase
    {
        //public MappingError(string message) : base(message)
        //{
        //}

        public MappingError(string message, params FailureBase[] failures) : base(message, failures)
        {
        }
    }
}
