using AIWorkspace.AI;
using AIWorkspace.AI.Models;
using AIWorkspace.Messages;
using AIWorkspace.Models;
using AIWorkspace.Repositories;
using CommunityToolkit.Mvvm.Messaging;

namespace AIWorkspace.Services.AI;

public class AIChatService
{
    private readonly ProviderManager _providerManager;
    private readonly MessageRepository _messages;
    private readonly ChatRepository _chats;
    private readonly IMessenger _messenger;

    public AIChatService(
        ProviderManager providerManager,
        MessageRepository messages,
        ChatRepository chats,
        IMessenger messenger)
    {
        _providerManager = providerManager;
        _messages = messages;
        _chats = chats;
        _messenger = messenger;
    }

    public async Task<string> AskAsync(
        int chatId,
        CancellationToken cancellationToken = default)
    {
        var history = await _messages.GetByChatAsync(chatId);
        var chat    = await _chats.GetAsync(chatId);

        if (chat == null)
            throw new InvalidOperationException("Chat not found.");

        var provider = _providerManager.GetProvider(chat.Provider);

        var aiMessages = history.Select(x => new AIMessage
        {
            Role    = x.Role,
            Content = x.Content
        });

        var response = await provider.SendAsync(aiMessages, cancellationToken);

        if (!response.Success)
            throw new InvalidOperationException(response.Error);

        var entity = await _messages.AddAsync(chatId, "assistant", response.Content);

        _messenger.Send(new MessageAddedMessage(new MessageModel
        {
            Id        = entity.Id,
            Role      = entity.Role,
            Content   = entity.Content,
            CreatedAt = entity.CreatedAt
        }));

        // Auto-title the chat from the first user message if it still has the default title.
        if (chat.Title == "New Chat")
        {
            var firstUserMessage = history.FirstOrDefault(x => x.Role == "user");
            if (firstUserMessage != null)
            {
                var title = MakeTitle(firstUserMessage.Content);
                await _chats.UpdateTitleAsync(chatId, title);
                _messenger.Send(new ChatTitleUpdatedMessage(chatId, title));
            }
        }

        return response.Content;
    }

    private static string MakeTitle(string content)
    {
        var trimmed = content.Trim();
        return trimmed.Length <= 45 ? trimmed : trimmed[..42] + "…";
    }
}
