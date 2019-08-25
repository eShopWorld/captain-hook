using System.Collections.Generic;
using Autofac.Features.Indexed;

namespace CaptainHook.Tests.Web.Authentication
{
    /// <summary>
    /// Rather than mocking this just providing an implementation that implements the same interface as IIndex<TK, TV>
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class IndexDictionary<TK, TV> : Dictionary<TK, TV>, IIndex<TK, TV>
    {

    }
}
