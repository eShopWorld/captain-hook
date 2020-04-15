using Xunit;
using Xunit.Abstractions;

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
        protected readonly ITestOutputHelper _testOutputHelper;

        protected IntegrationTestBase(E2EFlowTestsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }
    }
}
