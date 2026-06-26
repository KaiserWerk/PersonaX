using System.Text.Json.Serialization;

namespace PersonaX.UI.Services
{
    public interface ILLMService
    {
        Task<string> QueryAsync(string prompt, LlmOptions? options = null, CancellationToken cancellationToken = default);
    }

    public sealed class LlmOptions
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4o-mini";
        public bool AllowRawPii { get; set; }
        public bool RedactPii { get; set; } = true;
        public double Temperature { get; set; } = 0.2;
        public string SystemPrompt { get; set; } = "You are a careful assistant. Do not expose sensitive information.";
    }

    internal sealed class OpenAiChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("messages")]
        public List<OpenAiMessage> Messages { get; set; } = [];
    }

    internal sealed class OpenAiMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    internal sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice> Choices { get; set; } = [];
    }

    internal sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage? Message { get; set; }
    }
}
