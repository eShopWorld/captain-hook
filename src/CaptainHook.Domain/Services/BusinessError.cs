namespace CaptainHook.Domain.Services
{
    public class BusinessError
    {
        public string Message { get; }

        public BusinessError(string message)
        {
            Message = message;
        }
    }
}