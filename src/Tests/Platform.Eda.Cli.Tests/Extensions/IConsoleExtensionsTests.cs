using System;
using System.Collections.Generic;
using System.IO;
using Eshopworld.Tests.Core;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Platform.Eda.Cli.Extensions;
using Xunit;

namespace Platform.Eda.Cli.Tests.Extensions
{
    public class IConsoleExtensionsTests
    {
        public static IEnumerable<object[]> WriteTextTests = new List<object[]>
        {
            new object[] { "" },
            new object[] { "Error Text" },
            new object[] { "Very long text $$$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" +
                           "$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$ $$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$" }
        };

        private readonly Mock<TextWriter> _outWriter;
        private readonly Mock<TextWriter> _errorWriter;
        private readonly IConsole _console;

        public IConsoleExtensionsTests()
        {
            _outWriter = new Mock<TextWriter>(MockBehavior.Default);
            _errorWriter = new Mock<TextWriter>(MockBehavior.Default);

            var mockConsole = new Mock<IConsole>();
            mockConsole.Setup(c => c.Out).Returns(_outWriter.Object);
            mockConsole.Setup(c => c.Error).Returns(_errorWriter.Object);
            _console = mockConsole.Object;
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteNormal_DifferentStrings_OutputsDefaultForegroundColor(string outputString)
        {
            _console.WriteNormal(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatDefaultText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteNormal_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _console.WriteNormal(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatDefaultText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteError_DifferentStrings_OutputsRedText(string outputString)
        {
            _console.WriteError(outputString);

            _errorWriter.Verify(writer => writer.WriteLine(FormatErrorText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteError_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _console.WriteError(outputString);

            _errorWriter.Verify(writer => writer.WriteLine(FormatErrorText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteSuccess_DifferentStrings_OutputsGreenText(string outputString)
        {
            _console.WriteSuccess(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatSuccessText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteSuccess_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _console.WriteSuccess(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatSuccessText(outputString)), Times.Once);
        }

        [Fact, IsUnit]
        public void WriteNormalBox_SingleValue_OutputsBoxedText()
        {
            _console.WriteNormalBox("Test");

            _outWriter.Verify(writer => writer.WriteLine(FormatDefaultText(
                $"{new string('=', 80)}{Environment.NewLine}Test{Environment.NewLine}{new string('=', 80)}")), Times.Once);
        }

        private static string FormatDefaultText(string outputString) => $"{IConsoleExtensions.Colors.Cyan}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatSuccessText(string outputString) => $"{IConsoleExtensions.Colors.Green}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatErrorText(string outputString) => $"{IConsoleExtensions.Colors.Red}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatWarningText(string outputString) => $"{IConsoleExtensions.Colors.Yellow}{outputString}{IConsoleExtensions.Colors.Reset}";
    }
}