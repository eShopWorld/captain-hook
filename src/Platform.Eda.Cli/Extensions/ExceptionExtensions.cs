using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Platform.Eda.Cli.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> InnerExceptions(this Exception exception)
        {
            var currentException = exception;

            while (currentException != null)
            {
                yield return currentException;
                currentException = currentException.InnerException;
            }
        }
    }
}
