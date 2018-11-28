namespace CaptainHook.EventHandlerActor
{
    using System;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// todo nuke this
    /// </summary>
    public static class ModelParser
    {
        public static string ParseDomainType(string payload)
        {
            var jObject = JObject.Parse(payload);
            var domainType = ((JProperty)jObject.Parent).Name;
            return domainType;
        }

        public static Guid ParseOrderCode(string payload)
        {
            var jObject = JObject.Parse(payload);
            var orderCode = jObject.SelectToken("OrderCode").Value<Guid>();
            return orderCode;
        }
        
        public static string ParseBrandType(string payload)
        {
            var jObject = JObject.Parse(payload);
            var brandType = jObject.SelectToken("BrandType").Value<string>();
            return brandType;
        }
    }
}