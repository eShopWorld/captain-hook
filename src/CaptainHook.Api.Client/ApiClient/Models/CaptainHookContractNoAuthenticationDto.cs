// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    [Newtonsoft.Json.JsonObject("CaptainHook.Contract.NoAuthenticationDto")]
    public partial class CaptainHookContractNoAuthenticationDto : CaptainHookContractAuthenticationDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractNoAuthenticationDto class.
        /// </summary>
        public CaptainHookContractNoAuthenticationDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractNoAuthenticationDto class.
        /// </summary>
        public CaptainHookContractNoAuthenticationDto(string type = default(string))
            : base(type)
        {
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

    }
}
