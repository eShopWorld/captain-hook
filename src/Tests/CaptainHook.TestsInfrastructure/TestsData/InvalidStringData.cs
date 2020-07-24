using System.Collections;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.TestsData
{
    public abstract class InvalidStringData : IEnumerable<object[]>
    {
        protected abstract IEnumerable<string> Values { get; }

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var value in Values)
            {
                yield return new object[] { value };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class EmptyStrings : InvalidStringData
    {
        protected override IEnumerable<string> Values => new[] { null, string.Empty, "   " };
    }

    public class InvalidUris : InvalidStringData
    {
        protected override IEnumerable<string> Values => new[] { "not-a-uri", "https://no-domain", "https://.com" };
    }
}