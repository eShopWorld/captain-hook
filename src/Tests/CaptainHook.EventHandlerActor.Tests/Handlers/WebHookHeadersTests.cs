using System;
using System.Collections.Generic;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.EventHandlerActor.Tests.Handlers
{
    public class WebHookHeadersTests
    {
        private readonly WebHookHeaders _sut = new WebHookHeaders();

        private const string TestName = "testname";
        private const string TestValue = "testvalue";
        private Dictionary<string, string> TestDictionary => new Dictionary<string, string>
            {{TestName, TestValue}};
        private Dictionary<string, string> EmptyDictionary => new Dictionary<string, string>();

        public static IEnumerable<object[]> InvalidHeaderAndValue =>
            new List<object[]>
            {
                new object[] { null, TestValue },
                new object[] { " ", TestValue },
                new object[] { "", TestValue },
                new object[] { TestName, null },
                new object[] { TestName, " " },
                new object[] { TestName, "" },
                new object[] { null, null },
                new object[] { "", "" },
                new object[] { " ", " " },
                new object[] { "", " " },
                new object[] { " ", " " }
            };

        public static IEnumerable<object[]> InvalidData =>
            new List<object[]>
            {
                new object[] { null },
                new object[] { " " },
                new object[] { "" }
            };

        [Fact, IsUnit]
        public void AddContentHeader_When_ContentHeaderIsAdded_Then_ContentHeadersIsUpdated()
        {
            // Arrange

            // Act
            _sut.AddContentHeader(TestName, TestValue);
            
            // Assert
            _sut.ContentHeaders.Should().BeEquivalentTo(TestDictionary);
        }

        [Theory, IsUnit]
        [MemberData(nameof(InvalidHeaderAndValue))]
        public void AddContentHeader_When_ParameterIsNull_Then_ThrowsArgumentNullException(string name, string value)
        {
            // Arrange

            // Act
            Action fn = () => _sut.AddContentHeader(name, value);

            // Assert
            fn.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, IsUnit]
        public void AddRequestHeader_When_RequestHeaderIsAdded_Then_RequestHeadersIsUpdated()
        {
            // Arrange

            // Act
            _sut.AddRequestHeader(TestName, TestValue);

            // Assert
            _sut.RequestHeaders.Should().BeEquivalentTo(TestDictionary);
        }

        [Theory, IsUnit]
        [MemberData(nameof(InvalidHeaderAndValue))]
        public void AddRequestHeader_When_ParameterIsNull_Then_ThrowsArgumentNullException(string name, string value)
        {
            // Arrange

            // Act
            Action fn = () => _sut.AddRequestHeader(name, value);

            // Assert
            fn.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, IsUnit]
        public void RemoveContentHeader_When_ExistingNameIsProvided_Then_HeaderIsRemoved()
        {
            // Arrange
            _sut.AddContentHeader(TestName, TestValue);

            // Act
            _sut.RemoveContentHeader(TestName);

            // Assert
            _sut.ContentHeaders.Should().BeEquivalentTo(EmptyDictionary);
        }

        [Theory, IsUnit]
        [MemberData(nameof(InvalidData))]
        public void RemoveContentHeader_When_InvalidNameIsProvided_Then_ThrowsArgumentNullException(string name)
        {
            // Arrange
            _sut.AddContentHeader(TestName, TestValue);

            // Act
            Action fn = () => _sut.RemoveContentHeader(name);

            // Assert
            fn.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, IsUnit]
        public void RemoveRequestHeader_When_ExistingNameIsProvided_Then_HeaderIsRemoved()
        {
            // Arrange
            _sut.AddRequestHeader(TestName, TestValue);

            // Act
            _sut.RemoveRequestHeader(TestName);

            // Assert
            _sut.RequestHeaders.Should().BeEquivalentTo(EmptyDictionary);
        }

        [Theory, IsUnit]
        [MemberData(nameof(InvalidData))]
        public void RemoveRequestHeader_When_InvalidNameIsProvided_Then_ThrowsArgumentNullException(string name)
        {
            // Arrange
            _sut.AddRequestHeader(TestName, TestValue);

            // Act
            Action fn = () => _sut.RemoveRequestHeader(name);

            // Assert
            fn.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, IsUnit]
        public void ClearContentHeaders_WhenInvoked_AllContentHeadersOnlyAreRemoved()
        {
            // Arrange
            _sut.AddContentHeader(TestName, TestValue);
            _sut.AddRequestHeader(TestName, TestValue);

            // Act
            _sut.ClearContentHeaders();

            // Assert
            _sut.ContentHeaders.Should().BeEquivalentTo(EmptyDictionary);
            _sut.RequestHeaders.Should().BeEquivalentTo(TestDictionary);
        }

        [Fact, IsUnit]
        public void ClearRequestHeaders_WhenInvoked_AllRequestHeadersOnlyAreRemoved()
        {
            // Arrange
            _sut.AddRequestHeader(TestName, TestValue);
            _sut.AddContentHeader(TestName, TestValue);

            // Act
            _sut.ClearRequestHeaders();

            // Assert
            _sut.RequestHeaders.Should().BeEquivalentTo(EmptyDictionary);
            _sut.ContentHeaders.Should().BeEquivalentTo(TestDictionary);
        }

        [Fact, IsUnit]
        public void ClearHeaders_WhenInvoked_AllHeadersAreRemoved()
        {
            // Arrange
            _sut.AddRequestHeader(TestName, TestValue);
            _sut.AddContentHeader(TestName, TestValue);

            // Act
            _sut.ClearHeaders();

            // Assert
            _sut.RequestHeaders.Should().BeEquivalentTo(EmptyDictionary);
            _sut.ContentHeaders.Should().BeEquivalentTo(EmptyDictionary);
        }

    }
}
