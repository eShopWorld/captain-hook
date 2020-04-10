using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CaptainHook.Cli.Common
{
    /// <summary>
    /// Don't serialize empty collections and timeout default values
    /// </summary>
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType != typeof(string))
            {
                if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                {
                    property.ShouldSerialize =
                        instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                }

                if (property.PropertyType == typeof(TimeSpan) && property.PropertyName.Equals("Timeout", StringComparison.InvariantCulture))
                {
                    property.ShouldSerialize =
                        instance => ((TimeSpan)instance.GetType().GetProperty(property.PropertyName).GetValue(instance)).TotalSeconds != 100;
                }
            }
            return property;
        }
    }
}
