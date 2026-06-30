using AIWorkspace.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIWorkspace.Messages;

public class ChatSelectedMessage : ValueChangedMessage<ChatModel>
{
    public ChatSelectedMessage(ChatModel value)
        : base(value)
    {
    }
}