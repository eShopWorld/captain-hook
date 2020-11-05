using System;

namespace CaptainHook.Domain.Results
{
    public class ExceptionFailure : IFailure
    {
        public string ExceptionDetails { get; }

        public ExceptionFailure(Exception exception)
        {
            ExceptionDetails = exception.ToString();
        }
    }
}