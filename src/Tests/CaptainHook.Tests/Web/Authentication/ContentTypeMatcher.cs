using System.Net.Http;
using RichardSzalay.MockHttp;

namespace CaptainHook.Tests.Web.Authentication
{
    /// <inheritdoc />
    /// <summary>
    /// Works with mockHttp to match expected content types within a mocked request
    /// </summary>
    public class ContentTypeMatcher : IMockedRequestMatcher
    {
        private readonly string _expectedContentType;

        public ContentTypeMatcher(string contentType)
        {
            _expectedContentType = contentType;
        }

        public bool Matches(HttpRequestMessage message)
        {
            var msgContentType = message.Content.Headers.ContentType.ToString();
            if (_expectedContentType.Contains(";"))
            {
                return msgContentType == _expectedContentType;
            }

            return msgContentType == _expectedContentType
                   || msgContentType.StartsWith(_expectedContentType + ";");
        }
    }
}
