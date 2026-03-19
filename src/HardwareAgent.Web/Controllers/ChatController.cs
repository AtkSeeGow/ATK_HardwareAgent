using HardwareAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace HardwareAgent.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("Api/[controller]/[action]")]
    public class ChatController : ControllerBase
    {
        private readonly LanguageModelService _chatService;

        public ChatController(LanguageModelService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<string> Ask(string workspaceSlug, string question)
        {
            return await _chatService.AskAsync(workspaceSlug, question);
        }
    }
}