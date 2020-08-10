using CaptainHook.Domain.Results;
using System;

namespace CaptainHook.Domain.Errors
{
    public class CannotQueryEntityError : ErrorBase
    {
        public CannotQueryEntityError(string type, Exception exception) :
            base("Cannot query entity", new Failure("CannotQuery", $"Cannot query an entity of type '{type}'. {exception.Message}"))
        {
        }
    }
}
