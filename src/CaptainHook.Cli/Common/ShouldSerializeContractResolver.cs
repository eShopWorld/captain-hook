using CaptainHook.Common.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace CaptainHook.Cli.Common
{
    /// <summary>
    /// Don't serialize empty collections and timeout default values
    /// </summary>
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

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

            // don't serialize empty AuthenticationConfig objects
            if(property.PropertyType == typeof(AuthenticationConfig) && property.PropertyName.Equals("AuthenticationConfig", StringComparison.InvariantCulture))
            {
                property.ShouldSerialize =
                    instance => ((AuthenticationConfig)instance.GetType().GetProperty(property.PropertyName).GetValue(instance)).Type != AuthenticationType.None;
            }

            // don't serialize default HttpVerb of Post
            if (property.PropertyName.Equals("HttpVerb", StringComparison.InvariantCulture))
            {
                property.ShouldSerialize =
                    instance => !((string)instance.GetType().GetProperty(property.PropertyName).GetValue(instance)).Equals(HttpMethod.Post.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            return property;
        }
    }
}
