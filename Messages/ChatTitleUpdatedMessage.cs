using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIWorkspace.Messages;

public record ChatTitleUpdate(int ChatId, string NewTitle);

public class ChatTitleUpdatedMessage : ValueChangedMessage<ChatTitleUpdate>
{
    public ChatTitleUpdatedMessage(int chatId, string newTitle)
        : base(new ChatTitleUpdate(chatId, newTitle)) { }
}
