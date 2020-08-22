using CaptainHook.Storage.Cosmos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaptainHook.Storage.Cosmos
{
    internal class AuthenticationSubdocumentJsonConverter : JsonConverter
    {
        // This converter handles only deserialization, not serialization.
        public override bool CanRead => true;
        public override bool CanWrite => false;

        private static readonly Type AuthenticationSubdocumentType = typeof(AuthenticationSubdocument);

        public override bool CanConvert(Type objectType)
        {
            // Only if the target type is the abstract base class
            return objectType == AuthenticationSubdocumentType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // First, just read the JSON as a JObject
            var obj = JObject.Load(reader);

            // Then look at the type property:
            var typeDesc = obj["type"]?.Value<string>()?.ToLower();
            
            return typeDesc switch
            {
                "basic" => obj.ToObject<BasicAuthenticationSubdocument>(serializer),// Deserialize as a FileItem
                "oidc" => obj.ToObject<OidcAuthenticationSubdocument>(serializer),// Deserialize as a FolderItem
                _ => throw new InvalidOperationException($"Unknown authentication document type '{typeDesc}'."),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
