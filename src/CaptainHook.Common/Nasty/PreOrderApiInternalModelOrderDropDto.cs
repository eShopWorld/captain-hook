namespace CaptainHook.Common.Nasty
{
    using Newtonsoft.Json;

    public partial class PreOrderApiInternalModelOrderDropDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelOrderDropDto class.
        /// </summary>
        public PreOrderApiInternalModelOrderDropDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// PreOrderApiInternalModelOrderDropDto class.
        /// </summary>
        /// <param name="httpStatus">Http status.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="orderDropped">Order dropped.</param>
        public PreOrderApiInternalModelOrderDropDto(int? httpStatus = default(int?), string errorMessage = default(string), string errorCode = default(string), bool? orderDropped = default(bool?))
        {
            HttpStatus = httpStatus;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            OrderDropped = orderDropped;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets http status.
        /// </summary>
        [JsonProperty(PropertyName = "HttpStatus")]
        public int? HttpStatus { get; set; }

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        [JsonProperty(PropertyName = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets order dropped.
        /// </summary>
        [JsonProperty(PropertyName = "OrderDropped")]
        public bool? OrderDropped { get; set; }

    }
}