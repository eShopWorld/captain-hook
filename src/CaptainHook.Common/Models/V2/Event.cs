using System.Collections.Generic;

namespace CaptainHook.Common.Models.V2
{
    /// <summary>
    /// Event definition model/POCO
    /// </summary>
    public class Event
    {
        /// <summary>
        /// name of the event
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// underlying event (or message) full type name
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// scope of visibility for the event instance
        /// </summary>
        public EventVisibilityEnum Visibility { get; set; } = EventVisibilityEnum.Internal;
        /// <summary>
        /// list of tenant codes to constraint the event visibility (if applicable)
        /// </summary>
        public IList<string> TenantCodes { get; set; }
    }
}
