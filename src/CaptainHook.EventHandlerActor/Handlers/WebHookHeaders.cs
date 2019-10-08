using System;
using System.Collections.Generic;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// A webhook bag to be passed around within a request flow to supply the http client with header types which should be added to the request message.
    /// Internal data structures are not thread-safe.
    /// </summary>
    public class WebHookHeaders
    {
        public WebHookHeaders()
        {
            ContentHeaders = new Dictionary<string, string>();
            RequestHeaders = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// The Content header collection
        /// </summary>
        public Dictionary<string, string> ContentHeaders { get; }

        /// <summary>
        /// The Request Header Collection
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; }

        /// <summary>
        /// Adds a specific request header to the Request Header Collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddContentHeader(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            ContentHeaders.Add(name, value);
        }

        /// <summary>
        /// Removes a specific request header from the header collection
        /// </summary>
        /// <param name="name"></param>
        public void RemoveContentHeader(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (ContentHeaders.ContainsKey(name))
            {
                ContentHeaders.Remove(name);
            }
        }

        /// <summary>
        /// Adds a specific request header to the Request Header Collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddRequestHeader(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            RequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Removes a specific request header from the header collection
        /// </summary>
        /// <param name="name"></param>
        public void RemoveRequestHeader(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (RequestHeaders.ContainsKey(name))
            {
                RequestHeaders.Remove(name);
            }
        }

        /// <summary>
        /// Clears the request headers
        /// </summary>
        public void ClearRequestHeaders()
        {
            RequestHeaders.Clear();
        }

        /// <summary>
        /// Clears the content headers
        /// </summary>
        public void ClearContentHeaders()
        {
            ContentHeaders.Clear();
        }

        /// <summary>
        /// Clears all internal data structures which contain headers
        /// </summary>
        public void ClearHeaders()
        {
            ContentHeaders.Clear();
            RequestHeaders.Clear();
        }
    }
}