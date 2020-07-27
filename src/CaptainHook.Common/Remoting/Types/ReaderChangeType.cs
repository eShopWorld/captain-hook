using System;

namespace CaptainHook.Common.Remoting.Types
{
    [Flags]
    public enum ReaderChangeType
    {
        None = 0x00,
        ToBeRemoved = 0x01,
        ToBeCreated = 0x02,
        ToBeUpdated = ToBeCreated | ToBeRemoved,
    }
}