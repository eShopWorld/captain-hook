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
    public class ConsoleSubscriberWriterTests
    {
        private readonly ConsoleSubscriberWriter _consoleSubscriberWriter;
        private readonly Mock<TextWriter> _streamWriter;

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

        public ConsoleSubscriberWriterTests()
        {
            _streamWriter = new Mock<TextWriter>(MockBehavior.Default);

            var mockConsole = new Mock<IConsole>();
            mockConsole.Setup(c => c.Out).Returns(_streamWriter.Object);
            _consoleSubscriberWriter = new ConsoleSubscriberWriter(mockConsole.Object);
        }

        [Theory, IsUnit]
        [MemberData(nameof(TestOutputValidSubscribers))]
        public void OutputSubscribers_WhenValidSubscribers_WritesNormals(IEnumerable<PutSubscriberFile> files, string rootDir, IEnumerable<string> expectedOutputs)
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
        public void OutputSubscribers_WhenInvalidSubscribers_WritesWarnings(IEnumerable<PutSubscriberFile> files, string rootDir, IEnumerable<string> expectedOutputs)
        {
            _consoleSubscriberWriter.OutputSubscribers(files, rootDir);

            foreach (var expectedOutput in expectedOutputs)
            {
                _streamWriter.Verify(writer => writer.WriteLine(FormatWarningText(expectedOutput)));
            }
            _streamWriter.VerifyNoOtherCalls();
        }

        private static string FormatDefaultText(string outputString) => $"{IConsoleExtensions.Colors.Cyan}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatSuccessText(string outputString) => $"{IConsoleExtensions.Colors.Green}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatErrorText(string outputString) => $"{IConsoleExtensions.Colors.Red}{outputString}{IConsoleExtensions.Colors.Reset}";
        private static string FormatWarningText(string outputString) => $"{IConsoleExtensions.Colors.Yellow}{outputString}{IConsoleExtensions.Colors.Reset}";
    }
}
