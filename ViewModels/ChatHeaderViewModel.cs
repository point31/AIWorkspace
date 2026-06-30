using AIWorkspace.AI;
using AIWorkspace.Messages;
using AIWorkspace.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace AIWorkspace.ViewModels;

public partial class ChatHeaderViewModel : ObservableObject,
    IRecipient<ChatSelectedMessage>
{
    private readonly IMessenger _messenger;
    private readonly ChatRepository _chats;
    private readonly ProviderSettingsRepository _settings;

    private int _currentChatId;

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private ProviderType selectedProvider;

    /// <summary>Only providers that have a saved API key are offered.</summary>
    public ObservableCollection<ProviderType> AvailableProviders { get; } = [];

    public ChatHeaderViewModel(
        IMessenger messenger,
        ChatRepository chats,
        ProviderSettingsRepository settings)
    {
        _messenger = messenger;
        _chats     = chats;
        _settings  = settings;

        _messenger.RegisterAll(this);

        _ = LoadProvidersAsync();
    }

    private async Task LoadProvidersAsync()
    {
        var all = await _settings.GetAllAsync();
        AvailableProviders.Clear();
        foreach (var s in all.Where(x => !string.IsNullOrEmpty(x.ApiKey)))
            AvailableProviders.Add(s.Provider);
    }

    public void Receive(ChatSelectedMessage message)
    {
        _currentChatId   = message.Value.Id;
        Title            = message.Value.Title;
        SelectedProvider = message.Value.Provider;
    }

    // When the user picks a different provider in the header, persist it.
    partial void OnSelectedProviderChanged(ProviderType value)
    {
        if (_currentChatId == 0) return;
        _ = _chats.UpdateProviderAsync(_currentChatId, value);
    }
}
