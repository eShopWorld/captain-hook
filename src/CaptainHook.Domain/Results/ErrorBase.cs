namespace CaptainHook.Domain.Results
{
    public abstract class ErrorBase
    {
        public string Message { get; set; }
        public IFailure[] Failures { get; set; }

        protected ErrorBase(string message, params IFailure[] failures)
        {
            Message = message;
            Failures = failures;
        }
    }
}