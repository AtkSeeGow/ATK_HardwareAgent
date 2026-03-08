using HardwareAgent.Domain;
using HardwareAgent.Domain.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace HardwareAgent.Services
{
    public class ChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ChatOptions _options;

        public ChatService(
            IHttpClientFactory httpClientFactory,
            IOptions<ChatOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<string> AskAsync(string workspaceSlug, string question)
        {
            var httpClient = _httpClientFactory.CreateClient("AnythingLLM");

            var endpoint = $"{_options.BaseUrl}/workspace/{workspaceSlug}/chat";

            var chatRequest = new ChatRequest { Message = question };

            var json = JsonSerializer.Serialize(chatRequest);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var httpResponseMessage = await httpClient.PostAsync(endpoint, stringContent);

            var responseText = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
                return $"API錯誤: {httpResponseMessage.StatusCode}";

            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return chatResponse?.TextResponse ?? "無回應";
        }
    }
}
