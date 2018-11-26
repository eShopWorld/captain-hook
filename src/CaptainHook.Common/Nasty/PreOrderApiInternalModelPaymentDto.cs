namespace CaptainHook.Common.Nasty
{
    using Newtonsoft.Json;

    public class PreOrderApiInternalModelPaymentDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelPaymentDto class.
        /// </summary>
        public PreOrderApiInternalModelPaymentDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelPaymentDto class.
        /// </summary>
        /// <param name="paymentAttemptRef">Payment attempt reference.</param>
        /// <param name="authCode">Authorization code.</param>
        /// <param name="paymentState">Payment state.</param>
        /// <param name="paymentMethodCode">Payment method code of payment
        /// method used.</param>
        /// <param name="merchantTransactionId">Payment provider order
        /// id.</param>
        /// <param name="paymentBrand">Payment brand.</param>
        /// <param name="authResult">Authorisation result.</param>
        /// <param name="settleAfterUtc">Represent UTC of settlement.</param>
        /// <param name="paymentTime">Payment time.</param>
        /// <param name="ipAddress">IP address.</param>
        public PreOrderApiInternalModelPaymentDto(System.Guid? paymentAttemptRef = default(System.Guid?), string authCode = default(string), string paymentState = default(string), string paymentMethodCode = default(string), string merchantTransactionId = default(string), string paymentBrand = default(string), string authResult = default(string), System.DateTime? settleAfterUtc = default(System.DateTime?), System.DateTime? paymentTime = default(System.DateTime?), string ipAddress = default(string))
        {
            PaymentAttemptRef = paymentAttemptRef;
            AuthCode = authCode;
            PaymentState = paymentState;
            PaymentMethodCode = paymentMethodCode;
            MerchantTransactionId = merchantTransactionId;
            PaymentBrand = paymentBrand;
            AuthResult = authResult;
            SettleAfterUtc = settleAfterUtc;
            PaymentTime = paymentTime;
            IpAddress = ipAddress;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets payment attempt reference.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentAttemptRef")]
        public System.Guid? PaymentAttemptRef { get; set; }

        /// <summary>
        /// Gets or sets authorization code.
        /// </summary>
        [JsonProperty(PropertyName = "AuthCode")]
        public string AuthCode { get; set; }

        /// <summary>
        /// Gets or sets payment state.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentState")]
        public string PaymentState { get; set; }

        /// <summary>
        /// Gets or sets payment method code of payment method used.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentMethodCode")]
        public string PaymentMethodCode { get; set; }

        /// <summary>
        /// Gets or sets payment provider order id.
        /// </summary>
        [JsonProperty(PropertyName = "MerchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        /// <summary>
        /// Gets or sets payment brand.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentBrand")]
        public string PaymentBrand { get; set; }

        /// <summary>
        /// Gets or sets authorisation result.
        /// </summary>
        [JsonProperty(PropertyName = "AuthResult")]
        public string AuthResult { get; set; }

        /// <summary>
        /// Gets or sets represent UTC of settlement.
        /// </summary>
        [JsonProperty(PropertyName = "SettleAfterUtc")]
        public System.DateTime? SettleAfterUtc { get; set; }

        /// <summary>
        /// Gets or sets payment time.
        /// </summary>
        [JsonProperty(PropertyName = "PaymentTime")]
        public System.DateTime? PaymentTime { get; set; }

        /// <summary>
        /// Gets or sets IP address.
        /// </summary>
        [JsonProperty(PropertyName = "IpAddress")]
        public string IpAddress { get; set; }

    }
}