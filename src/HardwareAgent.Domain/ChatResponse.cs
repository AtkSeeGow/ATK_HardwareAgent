using System.Text.Json.Serialization;

namespace HardwareAgent.Domain
{
    public class ChatResponse
    {
        [JsonPropertyName("textResponse")]
        public string TextResponse { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
