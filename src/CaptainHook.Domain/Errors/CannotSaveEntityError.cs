using CaptainHook.Domain.Results;
using System;

namespace CaptainHook.Domain.Errors
{
    public class CannotSaveEntityError : ErrorBase
    {
        public CannotSaveEntityError(string type, Exception exception) :
            base("Cannot save entity", new Failure("CannotSave", $"Cannot save an entity of type '{type}'. {exception.Message}"))
        {
        }
    }
}
