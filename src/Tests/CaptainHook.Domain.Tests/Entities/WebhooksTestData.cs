using CaptainHook.Domain.Entities;
using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.Domain.Tests.Entities
{
    public class WebhooksAndCallbacks : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { WebhooksEntityType.Webhooks };
            yield return new object[] { WebhooksEntityType.Callbacks };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
