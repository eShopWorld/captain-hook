using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class BuildUriContext
    {
        private readonly string _originalUri;
        
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
                _replacedUri = _replacedUri.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
            }

            return this;
        }

        public Uri CheckIfRoutableAndReturn() => CreateUriOrPublishUnroutable(_replacedUri, _publishUnroutableEvent);

        public static Uri CreateUriOrPublishUnroutable(string uri, Action<string> publish)
        {
            var uriCreated = Uri.TryCreate(uri, UriKind.Absolute, out var newUri);
            if (!uriCreated || uri.Contains("{"))
            {
                publish($"Value '{uri}' cannot be used to create a Uri");
                return null;
            }

            return newUri;

        }
    }
}