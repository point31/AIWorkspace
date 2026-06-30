using AIWorkspace.AI.Models;

namespace AIWorkspace.AI.Interfaces;

public interface IAIProvider
{
    ProviderType Provider { get; }

    /// <summary>
    /// Applies the API key and model name loaded from ProviderSettings.
    /// Must be called before SendAsync.
    /// </summary>
    void Configure(string apiKey, string model);

    Task<AIResponse> SendAsync(
        IEnumerable<AIMessage> messages,
        CancellationToken cancellationToken = default);
}