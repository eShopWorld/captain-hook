using System;
using System.Diagnostics.CodeAnalysis;

namespace CaptainHook.Domain.Results
{
    [ExcludeFromCodeCoverage]
    public class ExceptionFailure : IFailure
    {
        public string ExceptionDetails { get; }

        public ExceptionFailure(Exception exception)
        {
            ExceptionDetails = exception.ToString();
        }
    }
}