namespace CaptainHook.Common.Nasty
{
    using System;
    using Eshopworld.Core;

    /// <summary>
    /// Really temp dto
    /// </summary>
    public class HttpResponseDto
    {
        public int StatusCode { get; set; }

        public string ReasonPhase { get; set; }

        public string Content { get; set; }
    }


    /// <summary>
    /// PlatformOrderCreateDomainEvent
    /// </summary>
    public class PlatformOrderCreateDomainEvent : DomainEvent
    {
        /// <summary>
        /// The OrderCode identifying the evolution order
        /// </summary>
        public Guid OrderCode { get; set; }

        /// <summary>
        /// PreOrderApiInternalModelOrderRequestDto (the object for the platform create Order call)
        /// </summary>
        public PreOrderApiInternalModelOrderRequestDto PreOrderApiInternalModelOrderRequestDto { get; set; }
    }

    public class RetailerOrderConfirmationDomainEvent : DomainEvent
    {
        /// <summary>
        /// The OrderVCode identifying the evolution order
        /// </summary>
        public Guid OrderCode { get; set; }

        /// <summary>
        /// OrderConfirmationRequestDto (the object for the retailer OrderConfirmationApi call)
        /// </summary>
        public OrderConfirmationRequestDto OrderConfirmationRequestDto { get; set; }
    }
}
