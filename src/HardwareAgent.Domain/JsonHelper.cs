using System.Text.Encodings.Web;
using System.Text.Json;

namespace HardwareAgent.Domain
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}
