using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaptainHook.Contract
{
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
            JObject jObject = JObject.Load(reader);

            var typeDesc = jObject["type"]?.Value<string>();

            AuthenticationDto item = typeDesc switch
            {
                OidcAuthenticationDto.Type => new OidcAuthenticationDto(),
                BasicAuthenticationDto.Type => new BasicAuthenticationDto(),
                _ => null
            };

            if (item != null)
            {
                serializer.Populate(jObject.CreateReader(), item);
            }

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
