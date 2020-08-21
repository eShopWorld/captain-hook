using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaptainHook.Contract
{
    public class AuthenticationDtoJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(AuthenticationDto).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            // Using a nullable bool here in case "is_album" is not present on an item
            string type = (string)jObject["type"];

            AuthenticationDto item;

            item = type?.ToLower() switch
            {
                "oidc" => new OidcAuthenticationDto(),
                "basic" => new BasicAuthenticationDto(),
                _ => null
            };


            serializer.Populate(jObject.CreateReader(), item);

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
