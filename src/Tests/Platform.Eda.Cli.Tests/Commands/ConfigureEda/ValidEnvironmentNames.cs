using System.Collections;
using System.Collections.Generic;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ValidEnvironmentNames : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { null };
            yield return new object[] { "CI" };
            yield return new object[] { "TEST" };
            yield return new object[] { "PREP" };
            yield return new object[] { "SAND" };
            yield return new object[] { "PROD" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
