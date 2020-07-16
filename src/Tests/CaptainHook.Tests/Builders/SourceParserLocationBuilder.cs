using System.Collections.Generic;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class SourceParserLocationBuilder
    {
        private string _path = null;
        private Location _location = Location.Uri;
        private DataType _type = DataType.HttpContent;
        private RuleAction _ruleAction = RuleAction.Route;
        private IDictionary<string, string> _replace;

        public SourceParserLocationBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public SourceParserLocationBuilder WithLocation(Location location)
        {
            _location = location;
            return this;
        }

        public SourceParserLocationBuilder WithType(DataType type)
        {
            _type = type;
            return this;
        }

        public SourceParserLocationBuilder WithRuleAction(RuleAction ruleAction)
        {
            _ruleAction = ruleAction;
            return this;
        }

        public SourceParserLocationBuilder AddReplace(string key, string value)
        {
            if (_replace == null)
                _replace = new Dictionary<string, string>();

            _replace.Add(key, value);
            return this;
        }

        public SourceParserLocation Create()
        {
            var source = new SourceParserLocation
            {
                Path = _path,
                Location = _location,
                Type = _type,
                RuleAction = _ruleAction,
                Replace = _replace
            };

            return source;
        }
    }
}