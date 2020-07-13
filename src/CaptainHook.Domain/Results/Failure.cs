namespace CaptainHook.Domain.Results
{
    public class Failure
    {
        public string Code { get; }
        public string Message { get; }
        public string Property { get; }

        public Failure(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public Failure(string code, string message, string property)
        {
            Code = code;
            Message = message;
            Property = property;
        }
    }
}