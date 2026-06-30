using AIWorkspace.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AIWorkspace.ViewModels;

public partial class StatusBarViewModel :
    ObservableObject,
    IRecipient<ChatCountChangedMessage>
{
    [ObservableProperty]
    private string status = "🟢 Ready";

    [ObservableProperty]
    private string provider = "🤖 OpenAI";

    [ObservableProperty]
    private string model = "No model";

    [ObservableProperty]
    private string version = "v1.0";

    [ObservableProperty]
    private string chatCount = "0 Chats";

    public StatusBarViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(ChatCountChangedMessage message)
    {
        ChatCount = $"{message.Value} Chats";
    }
}