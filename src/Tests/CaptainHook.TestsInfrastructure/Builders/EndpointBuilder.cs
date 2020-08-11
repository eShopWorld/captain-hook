using CaptainHook.Domain.Entities;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class EndpointBuilder
    {
        private string _uri;

        private string _selector;

        private string _httpVerb;

        private AuthenticationEntity _authentication;

        private UriTransformEntity _uriTransform;

        public EndpointBuilder WithUri(string uri)
        {
            _uri = uri;
            return this;
        }

        public EndpointBuilder WithSelector(string selector)
        {
            _selector = selector;
            return this;
        }

        public EndpointBuilder WithHttpVerb(string httpVerb)
        {
            _httpVerb = httpVerb;
            return this;
        }

        public EndpointBuilder WithAuthentication(AuthenticationEntity authentication)
        {
            _authentication = authentication;
            return this;
        }

        public EndpointBuilder WithUriTransform(UriTransformEntity uriTransform)
        {
            _uriTransform = uriTransform;
            return this;
        }

        public EndpointEntity Create()
        {
            return new EndpointEntity(_uri, _authentication, _httpVerb, _selector, uriTransform: _uriTransform);
        }
    }
}