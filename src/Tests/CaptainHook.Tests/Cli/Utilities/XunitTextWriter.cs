using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace CaptainHook.Tests.Cli.Utilities
{
    public class XunitTextWriter : TextWriter
    {
        private readonly ITestOutputHelper output;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public XunitTextWriter(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(char ch)
        {
            if (ch == '\n')
            {
                output.WriteLine(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            else
            {
                stringBuilder.Append(ch);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (stringBuilder.Length > 0)
                {
                    output.WriteLine(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            }

            base.Dispose(disposing);
        }
    }
}
