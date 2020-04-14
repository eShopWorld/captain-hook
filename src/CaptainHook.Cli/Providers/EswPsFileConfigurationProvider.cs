using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace CaptainHook.Cli.Providers
{
    public class EswPsFileConfigurationProvider : ConfigurationProvider
    {
        private static readonly Regex parseFileRegex = new Regex(@"^setConfig\s+'(?<key>event--\d+--.*?)'\s+(?:'(?<value>.*?)'|(?<value>\d))\s+\$KeyVault", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private readonly string path;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public EswPsFileConfigurationProvider(string path, IFileSystem fileSystem)
        {
            this.path = path;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Loads the INI data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var fileContent = fileSystem.File.ReadAllText(path);
            var matches = parseFileRegex.Matches(fileContent);
                
            foreach(Match match in matches)
            {
                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;

                var replacedKey = key.Replace("--", ConfigurationPath.KeyDelimiter);
                if (data.ContainsKey(replacedKey))
                {
                    throw new FormatException($"Key '{key}' is duplicated");
                }

                data.Add(replacedKey, value);
            }

            Data = data;
        }
    }
}
