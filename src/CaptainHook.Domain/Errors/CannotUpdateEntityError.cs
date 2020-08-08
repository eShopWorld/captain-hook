using CaptainHook.Domain.Results;
using System;

namespace CaptainHook.Domain.Errors
{
    public class CannotUpdateEntityError : ErrorBase
    {
        public CannotUpdateEntityError(string type, Exception exception) :
            base("Cannot update entity", new Failure("CannotSave", $"Cannot update an entity of type '{type}'. {exception.Message}"))
        {
        }
    }
}
