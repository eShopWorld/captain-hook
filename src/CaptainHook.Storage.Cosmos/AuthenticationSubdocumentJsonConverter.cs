﻿using CaptainHook.Storage.Cosmos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaptainHook.Storage.Cosmos
{
    internal class AuthenticationSubdocumentJsonConverter : JsonConverter
    {
        private static readonly Type AuthenticationSubdocumentType = typeof(AuthenticationSubdocument);

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            // Only if the target type is the abstract base class
            return objectType == AuthenticationSubdocumentType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // First, just read the JSON as a JToken
            JToken jToken = JToken.ReadFrom(reader);

            if(jToken.Type != JTokenType.Object)
            {
                return null;
            }

            // Then look at the type property:
            var typeDesc = jToken["type"]?.Value<string>();
            
            return typeDesc switch
            {
                BasicAuthenticationSubdocument.Type => jToken.ToObject<BasicAuthenticationSubdocument>(serializer),
                OidcAuthenticationSubdocument.Type => jToken.ToObject<OidcAuthenticationSubdocument>(serializer),
                _ => null,
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter handles only deserialization, not serialization.");
        }
    }
}
