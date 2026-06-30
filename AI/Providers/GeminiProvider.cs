using AIWorkspace.AI.Interfaces;
using AIWorkspace.AI.Models;

namespace AIWorkspace.AI.Providers;

public class GeminiProvider : IAIProvider
{
    private string _apiKey = "";
    private string _model = "";

    public ProviderType Provider => ProviderType.Gemini;

    public void Configure(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public Task<AIResponse> SendAsync(
        IEnumerable<AIMessage> messages,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}