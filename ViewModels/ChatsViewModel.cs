using CommunityToolkit.Mvvm.ComponentModel;

namespace AIWorkspace.ViewModels;

public partial class ChatsViewModel : ObservableObject
{
    public ChatSidebarViewModel Sidebar { get; }

    public MessageListViewModel MessageList { get; }

    public MessageInputViewModel MessageInput { get; }

    public ChatHeaderViewModel Header { get; }

    public ChatsViewModel(
    ChatSidebarViewModel sidebar,
    MessageListViewModel messageList,
    MessageInputViewModel messageInput,
    ChatHeaderViewModel header)
    {
        Sidebar = sidebar;
        MessageList = messageList;
        MessageInput = messageInput;
        Header = header;
    }
}