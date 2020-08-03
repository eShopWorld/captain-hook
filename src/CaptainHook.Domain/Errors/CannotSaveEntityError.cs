using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class CannotSaveEntityError : ErrorBase
    {
        public CannotSaveEntityError(string type) :
            base("Cannot save entity", new Failure("CannotSave", $"Cannot save an entity of type '{type}'"))
        {
        }
    }
}
