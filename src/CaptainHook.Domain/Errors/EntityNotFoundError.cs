using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class EntityNotFoundError : ErrorBase
    {
        public EntityNotFoundError(string type, string key) : 
            base("Entity not found", new Failure("NotFound", $"Can't find an entity of type '{type}' by '{key}'"))
        {
        }
    }
}