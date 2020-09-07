namespace CaptainHook.Domain.Results
{
    public class ValidationFailure : IFailure
    {
        public string Code { get; }
        public string Message { get; }
        public string Property { get; }

        public ValidationFailure(string code, string message, string property)
        {
            Code = code;
            Message = message;
            Property = property;
        }
    }
}