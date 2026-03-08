using Microsoft.AspNetCore.Mvc;

namespace HardwareAgent.Web.Controllers
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    public class AgentController : ControllerBase
    {
        private readonly ILogger<AgentController> _logger;

        public AgentController(ILogger<AgentController> logger)
        {
            _logger = logger;
        }
    }
}
