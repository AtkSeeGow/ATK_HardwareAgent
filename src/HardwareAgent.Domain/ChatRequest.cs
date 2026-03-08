using System.Text.Json.Serialization;

namespace HardwareAgent.Domain
{
    public class ChatRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "chat";
    }
}
