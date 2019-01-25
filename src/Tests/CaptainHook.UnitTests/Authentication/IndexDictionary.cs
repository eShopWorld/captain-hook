using System.Collections.Generic;
using Autofac.Features.Indexed;

namespace CaptainHook.UnitTests.Authentication
{
    /// <summary>
    /// Rather than mocking this just providing an implementation that implements the same interface as IIndex<K, V>
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class IndexDictionary<K, V> : Dictionary<K, V>, IIndex<K, V>
    {

    }
}
