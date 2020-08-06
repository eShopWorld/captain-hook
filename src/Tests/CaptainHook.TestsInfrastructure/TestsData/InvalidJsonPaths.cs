using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public class InvalidJsonPaths : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "$." };
            yield return new object[] { "TenantCode" };
            yield return new object[] { "\\" };
            yield return new object[] { "@" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
