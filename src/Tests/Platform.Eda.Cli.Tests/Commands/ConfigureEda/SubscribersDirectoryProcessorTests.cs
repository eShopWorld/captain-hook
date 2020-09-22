using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class SubscribersDirectoryProcessorTests
    {
        readonly SubscribersDirectoryProcessor _subscribersDirectoryProcessor;

        public SubscribersDirectoryProcessorTests()
        {
            _subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(GetMockFileSystem());
        }

        public static MockFileSystem GetMockFileSystem()
        {
            return new MockFileSystem(GetMockFiles(), Environment.CurrentDirectory);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentException))]
        public void ProcessDirectory_WhenPathEmptyOrNull_ThrowsException(string s, Type exceptionType)
        {
            Assert.Throws(exceptionType, () =>
                _subscribersDirectoryProcessor.ProcessDirectory(s));
        }

        [Theory]
        [InlineData("a-nonexistent-path")]
        public void ProcessDirectory_WithNonExistentPath_ReturnsError(string s)
        {
            var result = _subscribersDirectoryProcessor.ProcessDirectory(s);
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CliExecutionError>();
        }

        [Theory]
        [InlineData("TestFiles")]
        public void ProcessDirectory_WithValidFile_ReturnsNonNullObject(string s)
        {
            var result = _subscribersDirectoryProcessor.ProcessDirectory(s);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().HaveCount(1);
                result.Data.Single().Request.Should().NotBeNull();
                result.Data.Single().Request.SubscriberName.Should().Be("test-sub");
            }
        }

        public static Dictionary<string, MockFileData> GetMockFiles()
        {
            Dictionary<string, MockFileData> mockFiles = new Dictionary<string, MockFileData>();
            mockFiles.Add("TestFiles/sample1.json", new MockFileData(@"
{
  ""subscriberName"": ""test-sub"",
  ""eventName"": ""test-event"",
  ""subscriber"": {
    ""webhooks"": {
      ""endpoints"": [
        {
          ""uri"": ""https://blah.blah/testing"",
          ""authentication"": {
            ""type"": ""Basic"",
            ""username"": ""test"",
            ""passwordKeyName"": ""AzureSubscriptionId""
          },
          ""httpVerb"": ""post""
        }
      ]
    }
  }
}
"));
            return mockFiles;
        }

    }
}
