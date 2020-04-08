using System.IO;
using System.Runtime.CompilerServices;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    public abstract class GenerateJsonBase
    {
        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading. </param>
        /// <returns>A string containing all lines of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string ReadText(string path)
        {
            return File.ReadAllText(path);
        }

        public string ConvertToJson(string input)
        {

        }
    }
}
