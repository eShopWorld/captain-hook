using CaptainHook.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaptainHook.Api
{
    /// <summary>
    /// JSON converter for Authentication DTO
    /// </summary>
    public class AuthenticationDtoJsonConverter : JsonConverter
    {
        private static readonly Type AuthenticationDtoType = typeof(AuthenticationDto);

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return AuthenticationDtoType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // First, just read the JSON as a JToken
            JToken jToken = JToken.ReadFrom(reader);

            if (jToken.Type != JTokenType.Object)
            {
                return null;
            }

            // Then look at the type property:
            var typeDesc = jToken["type"]?.Value<string>();

            return typeDesc switch
            {
                OidcAuthenticationDto.Type => jToken.ToObject<OidcAuthenticationDto>(serializer),
                BasicAuthenticationDto.Type => jToken.ToObject<BasicAuthenticationDto>(serializer),
                _ => null
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
