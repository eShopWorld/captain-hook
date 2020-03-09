using System.Collections.Generic;

namespace CaptainHook.Common.Models.V2
{
    /// <summary>
    /// Event definition model/POCO
    /// </summary>
    public class Event
    {
        /// <summary>
        /// underlying event (or message) full type name
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// scope of visibility for the event instance
        /// </summary>
        public EventVisibility Visibility { get; set; }
        /// <summary>
        /// list of tenant codes to constraint the event visibility (if applicable)
        /// </summary>
        public IList<string> TenantCodes { get; set; }
    }
}
