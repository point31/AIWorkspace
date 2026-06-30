using AIWorkspace.Messages;
using AIWorkspace.Models;
using AIWorkspace.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace AIWorkspace.ViewModels;

public partial class MessageListViewModel :
    ObservableObject,
    IRecipient<ChatSelectedMessage>,
    IRecipient<MessageAddedMessage>
{
    private readonly MessageRepository _repository;

    public ObservableCollection<MessageModel> Messages { get; } = [];

    [ObservableProperty]
    private ChatModel? currentChat;

    public MessageListViewModel(MessageRepository repository)
    {
        _repository = repository;
        WeakReferenceMessenger.Default.Register<ChatSelectedMessage>(this);
        WeakReferenceMessenger.Default.Register<MessageAddedMessage>(this);
    }
    public void Receive(MessageAddedMessage message)
    {
        Messages.Add(message.Value);
    }
    public async void Receive(ChatSelectedMessage message)
    {
        CurrentChat = message.Value;

        Messages.Clear();

        var list = await _repository.GetByChatAsync(CurrentChat.Id);

        foreach (var item in list)
        {
            Messages.Add(new MessageModel
            {
                Id = item.Id,
                Role = item.Role,
                Content = item.Content,
                CreatedAt = item.CreatedAt
            });
        }
    }
    
}