using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Storage.Cosmos.Tests
{
    public class SubscriberQueryBuilderTests
    {
        private readonly SubscriberQueryBuilder _queryBuilder = new SubscriberQueryBuilder();

        [Fact, IsUnit]
        public void ShouldUsePartitionKey()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var query = _queryBuilder.BuildSelectSubscribersListEndpoints(eventName);
            
            // Assert
            query.PartitionKey.Should().Be(EndpointDocument.GetPartitionKey(eventName));
        }
    }
}
