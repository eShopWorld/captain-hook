using System;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class UnhandledExceptionError : ErrorBase
    {
        private readonly Exception _exception;

        public UnhandledExceptionError(string message, Exception exception) 
            : base(message)
        {
            _exception = exception;
        }
    }
}