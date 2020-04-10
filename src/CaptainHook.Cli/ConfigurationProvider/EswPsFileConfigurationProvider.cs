using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CaptainHook.Cli.ConfigurationProvider
{
    public class EswPsFileConfigurationProvider : FileConfigurationProvider
    {
        private static readonly Regex parseFileRegex = new Regex(@"setConfig\s+'(?<key>event--\d+--.*?)'\s+(?:'(?<value>.*?)'|(?<value>\d))\s+\$KeyVault", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public EswPsFileConfigurationProvider(EswPsFileConfigurationSource source) : base(source) { }

        /// <summary>
        /// Loads the INI data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load(Stream stream)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var reader = new StreamReader(stream))
            {
                var fileContent = reader.ReadToEnd();
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
            }

            Data = data;
        }
    }
}
