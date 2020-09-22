using System;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    [Flags]
    public enum ReaderChangeTypes
    {
        None = 0x00,
        ToBeRemoved = 0x01,
        ToBeCreated = 0x02,
        ToBeUpdated = ToBeCreated | ToBeRemoved,
    }
}