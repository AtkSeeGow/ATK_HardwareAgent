using HardwareAgent.Domain;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HardwareAgent.Services
{
    public class LanguageModelService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        public LanguageModelService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> AskAsync(string workspaceSlug, string question, CancellationToken cancellationToken = default)
        {
            var httpClient = httpClientFactory.CreateClient("AnythingLLM");

            var chatRequest = new ChatRequest { Message = question };

            var json = JsonSerializer.Serialize(chatRequest);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponseMessage = await httpClient.PostAsync($"workspace/{workspaceSlug}/chat", stringContent, cancellationToken);

            var responseText = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"AnythingLLM API error: {httpResponseMessage.StatusCode} - {responseText}");

            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseText, this.jsonSerializerOptions);

            var result = chatResponse?.TextResponse ?? "無回應";
            result = Regex.Replace(result, "<think>.*?</think>", "", RegexOptions.Singleline).Trim();
            return result;
        }
    }
}
