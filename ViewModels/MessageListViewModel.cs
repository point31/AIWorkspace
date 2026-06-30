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
    private readonly IMessenger _messenger;

    public ObservableCollection<MessageModel> Messages { get; } = [];

    [ObservableProperty]
    private ChatModel? currentChat;

    public MessageListViewModel(MessageRepository repository, IMessenger messenger)
    {
        _repository = repository;
        _messenger = messenger;

        _messenger.Register<MessageListViewModel, ChatSelectedMessage>(
    this,
    static (recipient, message) => recipient.Receive(message));

        _messenger.Register<MessageListViewModel, MessageAddedMessage>(
            this,
            static (recipient, message) => recipient.Receive(message));
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