using System;
using System.Collections.Generic;

namespace CaptainHook.Domain.Entities.Comparers
{
    public sealed class SelectorEndpointEntityEqualityComparer: IEqualityComparer<EndpointEntity>
    {
        public bool Equals(EndpointEntity x, EndpointEntity y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Selector, y.Selector, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(EndpointEntity obj)
        {
            return obj.Selector != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Selector) : 0;
        }
    }
}