using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Probe controller
    /// </summary>
    [Route("[controller]")]
    [AllowAnonymous]
    public class ProbeController : Controller
    {
        /// <summary>
        /// Returns a probe result
        /// </summary>
        /// <returns>Returns status code 200</returns>
        [HttpGet]
        [HttpHead]
        public IActionResult GetProbe()
        {
            return Ok("Healthy");
        }
    }
}