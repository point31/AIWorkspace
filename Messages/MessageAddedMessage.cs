using AIWorkspace.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIWorkspace.Messages;

public class MessageAddedMessage : ValueChangedMessage<MessageModel>
{
    public MessageAddedMessage(MessageModel value)
        : base(value)
    {
    }
}