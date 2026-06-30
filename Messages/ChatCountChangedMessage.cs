using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIWorkspace.Messages;

public class ChatCountChangedMessage : ValueChangedMessage<int>
{
    public ChatCountChangedMessage(int value)
        : base(value)
    {
    }
}