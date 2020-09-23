using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Platform.Eda.Cli.Common
{
    /// <summary>
    /// Don't serialize empty collections and timeout default values
    /// </summary>
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // when declaring type is SourceParserLocation, serialize with default config
            if (property.DeclaringType == typeof(SourceParserLocation))
            {
                return property;
            }

            // don't serialize empty collections
            if (property.PropertyType != typeof(string) && property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
            {
                property.ShouldSerialize =
                    instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
            }

            // don't serialize default timeout value of 100 seconds
            if (property.PropertyType == typeof(TimeSpan) && property.PropertyName.Equals("Timeout", StringComparison.InvariantCulture))
            {
                property.ShouldSerialize =
                    instance => ((TimeSpan)instance.GetType().GetProperty(property.PropertyName).GetValue(instance)).TotalSeconds != 100;
            }

            return property;
        }
    }
}
