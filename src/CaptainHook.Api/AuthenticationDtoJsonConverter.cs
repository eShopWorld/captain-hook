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
        private static readonly NoAuthenticationDto NoAuthentication = new NoAuthenticationDto();
        private static readonly InvalidAuthenticationDto InvalidAuthentication = new InvalidAuthenticationDto();

        /// <summary>
        /// Tells whether this converter can read JSON
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Tells whether this converter can write JSON
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Tells whether this converter can convert from the specified type
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return AuthenticationDtoType.IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Reads JSON data
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // First, just read the JSON as a JToken
            JToken jToken = JToken.ReadFrom(reader);

            if(jToken.Type == JTokenType.Null)
            {
                return NoAuthentication;
            }

            if (jToken.Type != JTokenType.Object)
            {
                return InvalidAuthentication;
            }

            // Then look at the type property:
            var typeDesc = jToken["type"]?.Value<string>();

            AuthenticationDto item = typeDesc switch
            {
                OidcAuthenticationDto.Type => new OidcAuthenticationDto(),
                BasicAuthenticationDto.Type => new BasicAuthenticationDto(),
                NoAuthenticationDto.Type => new NoAuthenticationDto(),
                _ => InvalidAuthentication
            };

            if (item != null)
            {
                serializer.Populate(jToken.CreateReader(), item);
            }

            return item;
        }

        /// <summary>
        /// Writes JSON data
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
