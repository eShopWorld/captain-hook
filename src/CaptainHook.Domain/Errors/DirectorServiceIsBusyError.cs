using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class DirectorServiceIsBusyError : ErrorBase
    {
        public DirectorServiceIsBusyError()
            : base("DirectorService is currently busy")
        {
        }
    }
}