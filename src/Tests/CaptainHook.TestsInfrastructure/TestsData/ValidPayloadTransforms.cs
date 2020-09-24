using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public class ValidPayloadTransforms : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "$" };
            yield return new object[] { "$.Request" };
            yield return new object[] { "$.Response" };
            yield return new object[] { "$.OrderConfirmation" };
            yield return new object[] { "$.PlatformOrderConfirmation" };
            yield return new object[] { "$.EmptyCart" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
