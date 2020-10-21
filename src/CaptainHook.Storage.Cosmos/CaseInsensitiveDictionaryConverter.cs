using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CaptainHook.Storage.Cosmos
{
    internal class CaseInsensitiveDictionaryConverter : JsonConverter
    {
        private static readonly Type DictionaryType = typeof(Dictionary<string, string>);

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == DictionaryType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);

            var originalDictionary = jsonObject.ToObject<Dictionary<string, string>>(serializer);
            return originalDictionary == null ? null : new Dictionary<string, string>(originalDictionary, StringComparer.InvariantCultureIgnoreCase);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
