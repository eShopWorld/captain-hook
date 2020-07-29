using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public class InvalidUris : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "not-a-uri" };
            yield return new object[] { "https:/invalid.com" };
            yield return new object[] { "https://.com" };
            yield return new object[] { "https://bla.{something}.eshopworld.com" };
            yield return new object[] { "/blah/endpoint" };
            yield return new object[] { "/blah/endpoint.com" };
            yield return new object[] { "../blah/endpoint" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}