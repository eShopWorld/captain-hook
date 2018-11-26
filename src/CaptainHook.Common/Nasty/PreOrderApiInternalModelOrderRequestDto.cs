namespace CaptainHook.Common.Nasty
{
    using Newtonsoft.Json;

    public partial class PreOrderApiInternalModelOrderRequestDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelOrderRequestDto class.
        /// </summary>
        public PreOrderApiInternalModelOrderRequestDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelOrderRequestDto class.
        /// </summary>
        /// <param name="payment">Request object to hold Payment
        /// details.</param>
        /// <param name="orderDrop">Request object to hold OrderDrop
        /// details.</param>
        /// <param name="order">The full order object.</param>
        /// <param name="preOrderCode">Evolution generated PreOrder
        /// code</param>
        public PreOrderApiInternalModelOrderRequestDto(PreOrderApiInternalModelPaymentDto payment, PreOrderApiInternalModelOrderDropDto orderDrop = default(PreOrderApiInternalModelOrderDropDto), PreOrderApiInternalModelOrderDto order = default(PreOrderApiInternalModelOrderDto), System.Guid? preOrderCode = default(System.Guid?))
        {
            Payment = payment;
            OrderDrop = orderDrop;
            Order = order;
            PreOrderCode = preOrderCode;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets request object to hold Payment details.
        /// </summary>
        [JsonProperty(PropertyName = "Payment")]
        public PreOrderApiInternalModelPaymentDto Payment { get; set; }

        /// <summary>
        /// Gets or sets request object to hold OrderDrop details.
        /// </summary>
        [JsonProperty(PropertyName = "OrderDrop")]
        public PreOrderApiInternalModelOrderDropDto OrderDrop { get; set; }

        /// <summary>
        /// Gets or sets the full order object.
        /// </summary>
        [JsonProperty(PropertyName = "Order")]
        public PreOrderApiInternalModelOrderDto Order { get; set; }

        /// <summary>
        /// Gets or sets evolution generated PreOrder code
        /// </summary>
        [JsonProperty(PropertyName = "PreOrderCode")]
        public System.Guid? PreOrderCode { get; set; }

    }
}