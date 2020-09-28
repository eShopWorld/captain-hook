using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CaptainHook.Cli.Tests;
using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Xunit;
using Xunit.Abstractions;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ConsoleSubscriberWriterTests
    {
        private readonly ConsoleSubscriberWriter _consoleSubscriberWriter;
        private readonly Mock<IConsole> _mockConsole;
        private readonly Mock<TextWriter> _streamWriter;

        public static IEnumerable<object[]> WriteTextTests = new List<object[]>
        {
            new object[] { "" },
            new object[] { "Error Text" },
            new object[] { "Very long text $$$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" +
                           "$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$ $$$$$$$$$$$$ $$$$$$$$$$$$$$ $$$$$$$$$$$$$$$$$$$$$$$" }
        };

        public static IEnumerable<object[]> TestOutputSubscribersParams = new List<object[]>
        {
            new object[] {
                new List<PutSubscriberFile>
                {
                    new PutSubscriberFile("C:\\dir1\\RandomFile1.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir1\\RandomFile2.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir1\\RandomFile3.txt", new PutSubscriberRequest())
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
            new object[] {
                new List<PutSubscriberFile>
                {
                    new PutSubscriberFile("C:\\dir1\\RandomFile1.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir1\\dir2\\RandomFile2.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir3\\RandomFile3.txt", new PutSubscriberRequest())
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
                    new PutSubscriberFile("C:\\dir1\\RandomFile1.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir1\\dir2\\RandomFile2.txt", new PutSubscriberRequest()),
                    new PutSubscriberFile("C:\\dir3\\RandomFile3.txt", new PutSubscriberRequest())
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

        public ConsoleSubscriberWriterTests()
        {
            _mockConsole = new Mock<IConsole>();
            _streamWriter = new Mock<TextWriter>(MockBehavior.Default);

            _mockConsole.Setup(c => c.Out)
                .Returns(_streamWriter.Object);
            _consoleSubscriberWriter = new ConsoleSubscriberWriter(_mockConsole.Object);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteNormal_DifferentStrings_OutputsDefaultForegroundColor(string outputString)
        {
            _consoleSubscriberWriter.WriteNormal(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = _mockConsole.Object.ForegroundColor);
            _streamWriter.Verify(writer => writer.WriteLine(outputString), Times.Once);
            _mockConsole.Verify(console => console.ResetColor());
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteNormal_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _consoleSubscriberWriter.WriteNormal(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = _mockConsole.Object.ForegroundColor, Times.Once);
            _streamWriter.Verify(writer => writer.WriteLine(""), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteError_DifferentStrings_OutputsRedText(string outputString)
        {
            _consoleSubscriberWriter.WriteError(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = ConsoleColor.Red, Times.Once);
            _streamWriter.Verify(writer => writer.WriteLine(outputString), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteError_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _consoleSubscriberWriter.WriteError(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = ConsoleColor.Red, Times.Once);
            _streamWriter.Verify(writer => writer.WriteLine(""), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(WriteTextTests))]
        public void WriteSuccess_DifferentStrings_OutputsGreenText(string outputString)
        {
            _consoleSubscriberWriter.WriteSuccess(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = ConsoleColor.DarkGreen, Times.Once);
            _streamWriter.Verify(writer => writer.WriteLine(outputString), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        public void WriteSuccess_WhenStringNull_OutputsEmptyLine(string outputString)
        {
            _consoleSubscriberWriter.WriteSuccess(outputString);

            _mockConsole.VerifySet(console => console.ForegroundColor = ConsoleColor.DarkGreen, Times.Once);
            _streamWriter.Verify(writer => writer.WriteLine(""), Times.Once);
        }

        [Theory, IsUnit]
        [InlineData("box", "Test")]
        public void WriteNormal_WhenFirstParamBox_OutputsBoxedText(params string[] outputStrings)
        {
            _consoleSubscriberWriter.WriteNormal(outputStrings);

            _mockConsole.VerifySet(console => console.ForegroundColor = It.IsAny<ConsoleColor>(), Times.Once);
            _mockConsole.Verify(console => console.Out.WriteLine(), Times.Never);
            _streamWriter.Verify(writer=>writer.WriteLine("===="), Times.Exactly(2));
            _streamWriter.Verify(writer=>writer.WriteLine("Test"), Times.Once);
        }
    }
}
