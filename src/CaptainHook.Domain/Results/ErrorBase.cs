namespace CaptainHook.Domain.Results
{
    public abstract class ErrorBase
    {
        public string Message { get; set; }
        public Failure[] Failures { get; set; }

        protected ErrorBase(string message, params Failure[] failures)
        {
            Message = message;
            Failures = failures;
        }
    }
}