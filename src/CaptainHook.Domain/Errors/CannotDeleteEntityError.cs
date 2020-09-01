using CaptainHook.Domain.Results;
using System;

namespace CaptainHook.Domain.Errors
{
    public class CannotDeleteEntityError : ErrorBase
    {
        public CannotDeleteEntityError(string type, Exception exception) :
            base("Cannot delete entity", new Failure("CannotDelete", $"Cannot delete an entity of type '{type}'. {exception.Message}"))
        {
        }

        public CannotDeleteEntityError(string type) :
            base("Cannot delete entity", new Failure("CannotDelete", $"Cannot delete an entity of type '{type}'"))
        {
        }
    }
}
