using System;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class UnhandledExceptionError : ErrorBase
    {
        public UnhandledExceptionError(string message, Exception exception)
            : base($"{message} {exception.Message}")
        {
        }
    }
}