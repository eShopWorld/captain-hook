using Xunit;

namespace CaptainHook.Api.Tests.Config
{
    [CollectionDefinition(TestFixtureName, DisableParallelization = true)]
    public class ApiClientCollection : ICollectionFixture<ApiClientFixture>
    {
        public const string TestFixtureName = "ApiClient";
    }
}