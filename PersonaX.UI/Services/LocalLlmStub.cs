namespace PersonaX.UI.Services
{
    public class LocalLlmStub : ILLMService
    {
        public Task<string> QueryAsync(string prompt, LlmOptions? options = null, CancellationToken cancellationToken = default)
        {
            const string message = "Lokales Modell ist noch nicht integriert. Verwende diese Klasse als Erweiterungspunkt für eine On-Device-Laufzeit wie ONNX oder gguf-basierte Runtimes.";
            return Task.FromResult(message);
        }
    }
}
