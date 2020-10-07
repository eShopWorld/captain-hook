﻿using System;
using System.Collections.Generic;
using System.IO;
using Eshopworld.Tests.Core;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Extensions;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
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

        [Fact, IsUnit]
        public void ConsoleWriteError_DifferentStrings_OutputsRedText()
        {
            var commandOptions = new List<CommandOption>
            {
                new CommandOption("--test-option", CommandOptionType.NoValue)
            };

            _console.EmitException(new Exception("test exception"), typeof(ConsoleSubscriberWriterTests), commandOptions);

            _outWriter.Verify(writer => writer.WriteLine(It.IsAny<string>()), Times.Never);
            _errorWriter.Verify(writer => writer.WriteLine(It.IsAny<string>()), Times.Once);
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

            _outWriter.Verify(writer => writer.WriteLine(FormatRedText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteError_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _console.WriteError(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatRedText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteSuccess_DifferentStrings_OutputsGreenText(string outputString)
        {
            _console.WriteSuccess(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatGreenText(outputString)), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteSuccess_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _console.WriteSuccess(outputString);

            _outWriter.Verify(writer => writer.WriteLine(FormatGreenText(outputString)), Times.Once);
        }

        [Fact, IsUnit]
        public void WriteNormalBox_SingleValue_OutputsBoxedText()
        {
            _console.WriteNormalBox("Test");

            _outWriter.Verify(writer => writer.WriteLine(FormatDefaultText(new string('=', 80))), Times.Exactly(2));
            _outWriter.Verify(writer => writer.WriteLine(FormatDefaultText("Test")), Times.Once);
        }



        private static string FormatDefaultText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Cyan}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatGreenText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Green}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatRedText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Red}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatYellowText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Yellow}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
    }

    public class ConsoleSubscriberWriterTests
    {
        private readonly ConsoleSubscriberWriter _consoleSubscriberWriter;
        private readonly Mock<TextWriter> _streamWriter;

        public static IEnumerable<object[]> WriteTextTests = new List<object[]>
        {
            new object[] { "" },
            new object[] { "Error Text" },
            new object[] { "Very long text $$$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" +
                           "$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$ $$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$" }
        };

        public static IEnumerable<object[]> TestOutputValidSubscribers = new List<object[]>
        {
            new object[]
            {
                new List<PutSubscriberFile>
                {
                    new PutSubscriberFile {File = new FileInfo("C:\\dir1\\RandomFile1.txt")},
                    new PutSubscriberFile {File = new FileInfo("C:\\dir1\\RandomFile2.txt")},
                    new PutSubscriberFile {File = new FileInfo("C:\\dir1\\RandomFile3.txt")}
                },
                "C:\\",
                new List<string>
                {
                    "File 'dir1\\RandomFile1.txt' has been found",
                    "File 'dir1\\RandomFile2.txt' has been found",
                    "File 'dir1\\RandomFile3.txt' has been found"
                }
            },
             new object[] {
                new List<PutSubscriberFile>
                {
                    new PutSubscriberFile { File = new FileInfo("C:\\dir1\\RandomFile1.txt") },
                    new PutSubscriberFile { File = new FileInfo("C:\\dir1\\dir2\\RandomFile2.txt") },
                    new PutSubscriberFile { File = new FileInfo("C:\\dir3\\RandomFile3.txt") }
                },
                "C:\\",
                new List<string>
                {
                    "File 'dir1\\RandomFile1.txt' has been found",
                    "File 'dir1\\dir2\\RandomFile2.txt' has been found",
                    "File 'dir3\\RandomFile3.txt' has been found"
                }
            },
            new object[] {
                new List<PutSubscriberFile>
                {
                    new PutSubscriberFile { File = new FileInfo("C:\\dir1\\RandomFile1.txt") },
                    new PutSubscriberFile { File = new FileInfo("C:\\dir1\\dir2\\RandomFile2.txt") },
                    new PutSubscriberFile { File = new FileInfo("C:\\dir3\\RandomFile3.txt") }
                },
                "C:\\dir1",
                new List<string>
                {
                    "File 'RandomFile1.txt' has been found",
                    "File 'dir2\\RandomFile2.txt' has been found",
                    "File '..\\dir3\\RandomFile3.txt' has been found",
                }
            }
        };

        public static IEnumerable<object[]> TestOutputInvalidSubscribers = new List<object[]>
        {
            new object[] {
                new List<PutSubscriberFile>(),
                "C:\\",
                new List<string>
                {
                    "No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions."
                }
            },
            new object[] {
                null,
                "C:\\",
                new List<string>
                {
                    "No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions."
                }
            },
        };

        private Mock<IConsole> _mockConsole;

        public ConsoleSubscriberWriterTests()
        {
            _streamWriter = new Mock<TextWriter>(MockBehavior.Default);

            _mockConsole = new Mock<IConsole>();
            _mockConsole.Setup(c => c.Out).Returns(_streamWriter.Object);
            _consoleSubscriberWriter = new ConsoleSubscriberWriter(_mockConsole.Object);
        }

        private static string FormatDefaultText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Cyan}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatGreenText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Green}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatRedText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Red}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";
        private static string FormatYellowText(string outputString) => $"{ConsoleSubscriberWriter.Colors.Yellow}{outputString}{ConsoleSubscriberWriter.Colors.Reset}";


        [Theory, IsUnit]
        [MemberData(nameof(TestOutputValidSubscribers))]
        public void OutputSubscribers_WritesExpectedOutputs(IEnumerable<PutSubscriberFile> files, string rootDir, IEnumerable<string> expectedOutputs)
        {
            _consoleSubscriberWriter.OutputSubscribers(files, rootDir);

            foreach (var expectedOutput in expectedOutputs)
            {
                _streamWriter.Verify(writer => writer.WriteLine(FormatDefaultText(expectedOutput)));
            }
            _streamWriter.VerifyNoOtherCalls();
        }


        [Theory, IsUnit]
        [MemberData(nameof(TestOutputInvalidSubscribers))]
        public void InvalidOutputSubscribers_WritesExpectedOutputs(IEnumerable<PutSubscriberFile> files, string rootDir, IEnumerable<string> expectedOutputs)
        {
            _consoleSubscriberWriter.OutputSubscribers(files, rootDir);

            foreach (var expectedOutput in expectedOutputs)
            {
                _streamWriter.Verify(writer => writer.WriteLine(FormatYellowText(expectedOutput)));
            }
            _streamWriter.VerifyNoOtherCalls();
        }
    }
}
