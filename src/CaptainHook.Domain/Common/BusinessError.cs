using System.Collections.Generic;

namespace CaptainHook.Domain.Common
{
    public class BusinessError : ErrorBase
    {
        public BusinessError(string message, List<Failure> failures = default) : base(message, failures)
        {
        }
    }

    public class ValidationError : ErrorBase
    {
        public ValidationError(string message, List<Failure> failures = default) : base(message, failures)
        {
        }
    }

    public abstract class ErrorBase
    {
        public string Message { get; set; }
        public List<Failure> Failures { get; set; }

        protected ErrorBase(string message, List<Failure> failures = default)
        {
            Message = message;
            Failures = failures;
        }
    }

    public class Failure
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Property { get; set; }
    }
}