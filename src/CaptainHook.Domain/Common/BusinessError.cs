using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Domain.Common
{
    public class EntityNotFoundError : ErrorBase
    {
        public EntityNotFoundError(string type, string key) : 
            base("Entity not found", new Failure("NotFound", $"Can't find an entity of type '{type}' by '{key}'"))
        {
        }
    }

    public class BusinessError : ErrorBase
    {
        public BusinessError(string message, params Failure[] failures) : base(message, failures)
        {
        }
    }

    public class ValidationError : ErrorBase
    {
        public ValidationError(string message, params Failure[] failures) : base(message, failures)
        {
        }
    }

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