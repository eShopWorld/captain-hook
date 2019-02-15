using System;
using RichardSzalay.MockHttp;

namespace CaptainHook.Tests.Authentication
{
    /// <summary>
    /// Extension for MockHttp
    /// </summary>
    public static class ContentTypeExtensions
    {
        /// <summary>
        /// Extension for mockHttp to test content type
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="contentType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static MockedRequest WithContentType(this MockedRequest handler, string contentType, string content)
        {
            handler.WithContent(content);
            handler.With(new ContentTypeMatcher(contentType));
            return handler;
        }

    }
}
