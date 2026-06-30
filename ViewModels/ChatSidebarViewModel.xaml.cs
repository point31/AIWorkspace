using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIWorkspace.Models;
using AIWorkspace.Repositories;
using System.Collections.ObjectModel;
using AIWorkspace.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace AIWorkspace.ViewModels;

public partial class ChatSidebarViewModel : ObservableObject
{
    private readonly ChatRepository _repository;

    public ObservableCollection<ChatModel> Chats { get; } = [];

    [ObservableProperty]
    private ChatModel? selectedChat;

    [ObservableProperty]
    private string searchText = "";

    public ChatSidebarViewModel(ChatRepository repository)
    {
        _repository = repository;
        LoadChats();
    }

    private async void LoadChats()
    {
        Chats.Clear();

        var chats = await _repository.GetAllAsync();

        foreach (var chat in chats)
        {
            Chats.Add(new ChatModel
            {
                Id = chat.Id,
                Title = chat.Title,
                UpdatedAt = chat.UpdatedAt,
                Provider = chat.Provider
            });
        }

        SelectedChat = Chats.FirstOrDefault();
    }


    partial void OnSelectedChatChanged(ChatModel? oldValue, ChatModel? newValue)
    {
        if (newValue == null)
            return;

        WeakReferenceMessenger.Default.Send(
    new ChatSelectedMessage(newValue));
    }

    [RelayCommand]
    private async Task NewChat()
    {
        var chat = await _repository.CreateAsync("New Chat");

        Chats.Insert(0, new ChatModel
        {
            Id = chat.Id,
            Title = chat.Title,
            UpdatedAt = chat.UpdatedAt,
            Provider = chat.Provider
        });

        SelectedChat = Chats.First();
    }
}