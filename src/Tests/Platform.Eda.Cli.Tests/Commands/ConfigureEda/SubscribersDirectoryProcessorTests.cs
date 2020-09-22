using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using CaptainHook.Api.Client.Models;
using FluentAssertions;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
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
        public void FilePath(string s, Type exceptionType)
        {
            Assert.Throws(exceptionType, () =>
                _subscribersDirectoryProcessor.ProcessDirectory(s));
        }

        [Theory]
        [InlineData("this-path-might-not-exist")]
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
            result.IsError.Should().BeFalse();
            result.Data.Should().HaveCount(1);
            result.Data.Single().Request.Should().NotBeNull();
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
          ""uri"": ""https://webhook.site/testing"",
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
