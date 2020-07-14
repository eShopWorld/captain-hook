using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class BuildUriContext
    {
        private readonly string _originalUri;
        private readonly string _selector;
        private readonly Action<string> _publishUnroutableEvent;
        private string _replacedUri;

        public BuildUriContext([NotNull] string uri, Action<string> publishUnroutableEvent)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(uri));
            }
            
            _originalUri = uri;
            _publishUnroutableEvent = publishUnroutableEvent;
        }

        public BuildUriContext ApplyReplace(IDictionary<string, string> replacements)
        {
            _replacedUri = _originalUri;
            foreach (var (key, value) in replacements)
            {
                _replacedUri = _replacedUri.Replace($"{{{key}}}", value);
            }

            return this;
        }

        public Uri CheckIfRoutableAndReturn()
        {
            if (_replacedUri.Contains("{"))
            {
                _publishUnroutableEvent($"Uri still contains unpopulated variables: {_replacedUri}");
                return null;
            }

            return new Uri(_replacedUri);
        }
    }
}