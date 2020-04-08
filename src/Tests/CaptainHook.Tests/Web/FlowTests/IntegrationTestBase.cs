using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    /// <summary>
    /// base class for test classes
    ///
    /// we use individual classes to run the tests in parallel 
    /// </summary>
    public abstract class IntegrationTestBase : IClassFixture<E2EFlowTestsFixture>
    {
        protected readonly E2EFlowTestsFixture _fixture;

        protected IntegrationTestBase(E2EFlowTestsFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
