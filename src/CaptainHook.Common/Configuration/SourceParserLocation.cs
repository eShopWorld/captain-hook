using System.Collections.Generic;

namespace CaptainHook.Common.Configuration
{
    public class SourceParserLocation: ParserLocation
    {
        public IDictionary<string, string> Replace { get; set; }
    }
}