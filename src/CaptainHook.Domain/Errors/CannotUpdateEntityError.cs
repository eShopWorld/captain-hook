using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class CannotUpdateEntityError : ErrorBase
    {
        public CannotUpdateEntityError(string type) :
            base("Cannot update entity", new Failure("CannotSave", $"Cannot update an entity of type '{type}'"))
        {
        }
    }
}
