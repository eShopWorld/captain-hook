using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public class EmptyStrings : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { "   " };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}