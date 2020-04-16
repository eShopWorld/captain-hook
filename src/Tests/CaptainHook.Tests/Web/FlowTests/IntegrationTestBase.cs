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
        protected readonly E2EFlowTestsFixture Fixture;
        protected readonly ITestOutputHelper TestOutputHelper;

        protected IntegrationTestBase(E2EFlowTestsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            TestOutputHelper = testOutputHelper;
        }
    }
}
