using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CaptainHook.Cli.ConfigurationProvider
{
    public class EswPsFileConfigurationProvider : FileConfigurationProvider
    {
        private static Regex parseFileRegex = new Regex(@"setConfig\s+'(event--\d+--.*?)'\s+(?:'(.*?)'|(\d))\s+\$KeyVault", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                    var key = match.Groups[1].Value.Replace("--", ConfigurationPath.KeyDelimiter);
                    var value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value;

                    if (data.ContainsKey(key))
                    {
                        throw new FormatException($"Key '{key}' is duplicated");
                    }

                    data.Add(key, value);
                }
            }

            Data = data;
        }
    }
}
