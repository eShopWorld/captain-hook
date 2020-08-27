using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Entities.Comparers;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities.Comparers
{
    public class SelectorEndpointEntityEqualityComparerTests
    {
        private readonly SelectorEndpointEntityEqualityComparer _comparer = new SelectorEndpointEntityEqualityComparer();

        [Fact, IsUnit]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            var entity = new EndpointEntity("uri", null, "PUT", "*");

            // Act
            var result = _comparer.Equals(entity, entity);

            // Assert
            result.Should().BeTrue("because same references were provided");
        }

        [Fact, IsUnit]
        public void Equals_DifferentObjectsSameSelectors_ReturnsTrue()
        {
            // Arrange
            var entity1 = new EndpointEntity("uri", null, "PUT", "*");
            var entity2 = new EndpointEntity("uri2", null, "DELETE", "*");

            // Act
            var result = _comparer.Equals(entity1, entity2);

            // Assert
            result.Should().BeTrue("because the same selector has been provided");
        }

        [Fact, IsUnit]
        public void Equals_DifferentObjectsDifferentSelectors_ReturnsFalse()
        {
            // Arrange
            var entity1 = new EndpointEntity("uri", null, "PUT", "*");
            var entity2 = new EndpointEntity("uri", null, "PUT", "abc");

            // Act
            var result = _comparer.Equals(entity1, entity2);

            // Assert
            result.Should().BeFalse("because different selectors have been provided");
        }
    }
}