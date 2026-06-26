using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PersonaX.UI.Services
{
    public class OpenAiLlmService : ILLMService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;

        public OpenAiLlmService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> QueryAsync(string prompt, LlmOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new LlmOptions();

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("LLM BaseUrl ist nicht konfiguriert.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("LLM ApiKey ist nicht konfiguriert.");
            }

            var sanitizedPrompt = SanitizePrompt(prompt, options);
            var request = new OpenAiChatRequest
            {
                Model = options.Model,
                Temperature = options.Temperature,
                Messages =
                [
                    new OpenAiMessage { Role = "system", Content = options.SystemPrompt },
                    new OpenAiMessage { Role = "user", Content = sanitizedPrompt }
                ]
            };

            using var message = new HttpRequestMessage(HttpMethod.Post, BuildEndpoint(options.BaseUrl));
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            message.Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(payload, JsonOptions);
            var content = parsed?.Choices.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Die LLM-Antwort war leer.");
            }

            return content;
        }

        private static string BuildEndpoint(string baseUrl)
        {
            var trimmed = baseUrl.TrimEnd('/');
            return trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"{trimmed}/chat/completions";
        }

        private static string SanitizePrompt(string prompt, LlmOptions options)
        {
            if (options.AllowRawPii || !options.RedactPii)
            {
                return prompt;
            }

            var redacted = Regex.Replace(prompt, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", "[REDACTED-EMAIL]", RegexOptions.IgnoreCase);
            redacted = Regex.Replace(redacted, @"\+?[0-9][0-9\-\s()]{6,}[0-9]", "[REDACTED-PHONE]");
            redacted = Regex.Replace(redacted, @"\b\d{5,}\b", "[REDACTED-NUMBER]");
            return redacted;
        }
    }
}
