using System.Collections.Generic;

namespace CaptainHook.Domain.Models
{
    public class Event        
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IList<Subscriber> Subscribers { get; set; }
    }
}
