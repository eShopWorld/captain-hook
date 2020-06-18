using CaptainHook.Repository.Models;
using CaptainHook.Repository.QueryBuilders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Storage.Cosmos.Tests
{
    public class SubscriberQueryBuilderTests
    {
        private readonly SubscriberQueryBuilder queryBuilder = new SubscriberQueryBuilder();

        [Fact, IsUnit]
        public void ShouldUsePartitionKey()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var query = queryBuilder.BuildSelectSubscribersListEndpoints(eventName);
            
            // Assert
            query.PartitionKey.Should().Be(EndpointDocument.GetPartitionKey(eventName));
        }
    }
}
