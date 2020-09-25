using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public class ValidHttpVerbs : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "GET" };
            yield return new object[] { "PUT" };
            yield return new object[] { "POST" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
