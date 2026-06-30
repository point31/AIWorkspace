using AIWorkspace.Messages;
using AIWorkspace.Repositories;
using AIWorkspace.Services.AI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AIWorkspace.ViewModels;

public partial class MessageInputViewModel : ObservableObject,
    IRecipient<ChatSelectedMessage>
{
    private readonly MessageRepository _repository;
    private readonly IMessenger _messenger;
    private readonly AIChatService _chatService;

    private int _currentChatId;

    [ObservableProperty]
    private string messageText = "";

    public MessageInputViewModel(
        MessageRepository repository,
        IMessenger messenger,
        AIChatService chatService)
    {
        _repository = repository;
        _messenger = messenger;
        _chatService = chatService;

        _messenger.RegisterAll(this);
    }

    public void Receive(ChatSelectedMessage message)
    {
        _currentChatId = message.Value.Id;
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        if (_currentChatId == 0)
            return;

        var entity = await _repository.AddAsync(
            _currentChatId,
            "user",
            MessageText);

        _messenger.Send(new MessageAddedMessage(
            new Models.MessageModel
            {
                Id = entity.Id,
                Role = entity.Role,
                Content = entity.Content,
                CreatedAt = entity.CreatedAt
            }));

        MessageText = "";

        await _chatService.AskAsync(_currentChatId);
    }
}
