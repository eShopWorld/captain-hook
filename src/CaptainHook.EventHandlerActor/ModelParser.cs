namespace CaptainHook.EventHandlerActor
{
    using System;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// todo nuke this in V1
    /// </summary>
    public static class ModelParser
    {
        public static string ParseDomainType(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }

            var domainType = ((JProperty)jObject.Parent).Name;
            return domainType;
        }

        public static Guid ParseOrderCode(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }

            var orderCode = jObject.SelectToken("OrderCode").Value<Guid>();
            return orderCode;
        }

        public static string ParseBrandType(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }
            var brandType = jObject.SelectToken("BrandType").Value<string>();
            return brandType;
        }

        public static (string brandType, string domainType) ParseBrandAndDomainType(string payload)
        {
            var jObject = JObject.Parse(payload);

            var brandType = ParseBrandType(payload, jObject);
            var domainType = ParseDomainType(payload, jObject);

            return (brandType, domainType);
        }
    }
}