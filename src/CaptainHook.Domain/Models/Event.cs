using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CaptainHook.Domain.Models
{
    public class Event        
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("pk")]
        public string PartitionKey { get; set; }

        public IList<Subscriber> Subscribers { get; set; }
    }
}
