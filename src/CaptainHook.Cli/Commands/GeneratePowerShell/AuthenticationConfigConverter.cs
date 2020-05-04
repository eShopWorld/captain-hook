using System;
using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class AuthenticationConfigConverter : JsonConverter
    {
        private static readonly Dictionary<AuthenticationType, Type> typesMap = new Dictionary<AuthenticationType, Type>
        {
            [AuthenticationType.OIDC] = typeof(OidcAuthenticationConfig)
        };

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var authType = ParseEnumType(token);

            if (typesMap.TryGetValue(authType, out var actualType))
            {
                if (existingValue == null || existingValue.GetType() != actualType)
                {
                    var contract = serializer.ContractResolver.ResolveContract(actualType);
                    existingValue = contract.DefaultCreator();
                }
                using (var subReader = token.CreateReader())
                {
                    serializer.Populate(subReader, existingValue);
                }
                return existingValue;
            }

            return null;
        }

        private AuthenticationType ParseEnumType(JToken token)
        {
            var rawType = (string)token["Type"];
            if (rawType == null)
                throw new InvalidOperationException("Invalid authentication type data");

            if (Enum.TryParse(typeof(AuthenticationType), rawType, true, out var oType))
            {
                return (AuthenticationType)oType;
            }

            return AuthenticationType.None;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(AuthenticationConfig).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}