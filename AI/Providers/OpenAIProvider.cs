using AIWorkspace.AI.Interfaces;
using AIWorkspace.AI.Models;
using OpenAI.Chat;

namespace AIWorkspace.AI.Providers;

public class OpenAIProvider : IAIProvider
{
    private string _apiKey = "";
    private string _model = "gpt-4o-mini";
    private ChatClient? _client;

    public ProviderType Provider => ProviderType.OpenAI;

    public void Configure(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;

        // Rebuild the client whenever configuration changes.
        _client = string.IsNullOrWhiteSpace(_apiKey)
            ? null
            : new ChatClient(_model, _apiKey);
    }

    public async Task<AIResponse> SendAsync(
        IEnumerable<AIMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            return new AIResponse
            {
                Success = false,
                Error = "OpenAI is not configured. Please add an API key in AI Providers settings."
            };
        }

        try
        {
            var chatMessages = messages
                .Select<AIMessage, ChatMessage>(m => m.Role switch
                {
                    "system"    => ChatMessage.CreateSystemMessage(m.Content),
                    "assistant" => ChatMessage.CreateAssistantMessage(m.Content),
                    _           => ChatMessage.CreateUserMessage(m.Content)
                })
                .ToList();

            var response = await _client.CompleteChatAsync(
                chatMessages,
                cancellationToken: cancellationToken);

            var content = response.Value.Content
                .Select(p => p.Text)
                .Aggregate(string.Concat);

            return new AIResponse
            {
                Success = true,
                Content = content
            };
        }
        catch (Exception ex)
        {
            return new AIResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
