using System;
using CaptainHook.Common.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Common.Json
{
    public class AuthenticationConfigConverter : JsonConverter
    {
        private static readonly Type AuthenticationConfigType = typeof(AuthenticationConfig);

        private static readonly AuthenticationConfig NoAuthentication = new AuthenticationConfig();

        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return AuthenticationConfigType.IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // First, just read the JSON as a JToken
            JToken jToken = JToken.ReadFrom(reader);

            if (jToken.Type != JTokenType.Object)
            {
                return NoAuthentication;
            }

            // Then look at the type property:
            var typeDesc = jToken["Type"]?.Value<string>();

            var canParse = Enum.TryParse(typeof(AuthenticationType), typeDesc, true, out var oType);

            AuthenticationConfig item = (AuthenticationType?)oType switch
            {
                AuthenticationType.Basic => new BasicAuthenticationConfig(),
                AuthenticationType.OIDC => new OidcAuthenticationConfig(),
                AuthenticationType.Custom => new AuthenticationConfig(),
                AuthenticationType.None => new AuthenticationConfig(),
                _ => new AuthenticationConfig()
            };

            if (canParse)
            {
                serializer.Populate(jToken.CreateReader(), item);
            }

            return item;
        }
    }
}