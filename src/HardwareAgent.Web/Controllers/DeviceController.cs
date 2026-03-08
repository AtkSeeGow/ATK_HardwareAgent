using Microsoft.AspNetCore.Mvc;

namespace HardwareAgent.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("Api/[controller]/[action]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(ILogger<DeviceController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> Heartbeat()
        {
            return new { command = "HelloWorld" };
        }
    }
}
