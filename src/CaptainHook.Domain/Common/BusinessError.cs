namespace CaptainHook.Domain.Common
{
    public class BusinessError : ErrorBase
    {
        public string Message { get; }

        public BusinessError(string message)
        {
            Message = message;
        }
    }

    public abstract class ErrorBase
    {

    }
}