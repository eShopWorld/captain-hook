// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class CaptainHookDomainErrorsValidationError
    {
        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookDomainErrorsValidationError class.
        /// </summary>
        public CaptainHookDomainErrorsValidationError()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookDomainErrorsValidationError class.
        /// </summary>
        public CaptainHookDomainErrorsValidationError(string message = default(string), IList<CaptainHookDomainResultsFailure> failures = default(IList<CaptainHookDomainResultsFailure>))
        {
            Message = message;
            Failures = failures;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "failures")]
        public IList<CaptainHookDomainResultsFailure> Failures { get; set; }

    }
}
