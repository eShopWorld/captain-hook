// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class CaptainHookContractClientSecretDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractClientSecretDto class.
        /// </summary>
        public CaptainHookContractClientSecretDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractClientSecretDto class.
        /// </summary>
        public CaptainHookContractClientSecretDto(string vault = default(string), string name = default(string))
        {
            Vault = vault;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "vault")]
        public string Vault { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

    }
}
