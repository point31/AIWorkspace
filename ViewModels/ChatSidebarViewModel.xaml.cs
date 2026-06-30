using AIWorkspace.AI;
using AIWorkspace.Messages;
using AIWorkspace.Models;
using AIWorkspace.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace AIWorkspace.ViewModels;

public partial class ChatSidebarViewModel : ObservableObject,
    IRecipient<ChatTitleUpdatedMessage>
{
    private readonly ChatRepository _repository;
    private readonly ProviderSettingsRepository _providerSettings;
    private readonly IMessenger _messenger;

    // All loaded chats (unfiltered source of truth)
    private readonly List<ChatModel> _allChats = [];

    /// <summary>Filtered chats shown in the sidebar list.</summary>
    public ObservableCollection<ChatModel> Chats { get; } = [];

    /// <summary>Providers that have a configured API key — shown in the filter bar.</summary>
    public ObservableCollection<ProviderType> ActiveProviders { get; } = [];

    [ObservableProperty]
    private ChatModel? selectedChat;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private ProviderType? selectedProvider;

    public ChatSidebarViewModel(
        ChatRepository repository,
        ProviderSettingsRepository providerSettings,
        IMessenger messenger)
    {
        _repository      = repository;
        _providerSettings = providerSettings;
        _messenger       = messenger;

        _messenger.Register<ChatSidebarViewModel, ChatTitleUpdatedMessage>(
            this, static (r, m) => r.Receive(m));

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        // Load configured providers for the filter bar
        var settings = await _providerSettings.GetAllAsync();
        var configured = settings
            .Where(x => !string.IsNullOrEmpty(x.ApiKey))
            .Select(x => x.Provider)
            .ToList();

        ActiveProviders.Clear();
        foreach (var p in configured)
            ActiveProviders.Add(p);

        // Default to first configured provider
        SelectedProvider = ActiveProviders.FirstOrDefault();

        await RefreshChatsAsync();
    }

    private async Task RefreshChatsAsync()
    {
        _allChats.Clear();

        var chats = await _repository.GetAllAsync();

        foreach (var chat in chats)
        {
            _allChats.Add(new ChatModel
            {
                Id        = chat.Id,
                Title     = chat.Title,
                UpdatedAt = chat.UpdatedAt,
                Provider  = chat.Provider
            });
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = SearchText?.Trim().ToLowerInvariant() ?? "";

        var filtered = _allChats
            .Where(c => SelectedProvider == null || c.Provider == SelectedProvider)
            .Where(c => string.IsNullOrEmpty(search) || c.Title.ToLowerInvariant().Contains(search))
            .ToList();

        Chats.Clear();
        foreach (var c in filtered)
            Chats.Add(c);

        if (SelectedChat == null || !Chats.Contains(SelectedChat))
            SelectedChat = Chats.FirstOrDefault();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedProviderChanged(ProviderType? value) => ApplyFilter();

    partial void OnSelectedChatChanged(ChatModel? oldValue, ChatModel? newValue)
    {
        if (newValue == null) return;
        _messenger.Send(new ChatSelectedMessage(newValue));
    }

    public void Receive(ChatTitleUpdatedMessage message)
    {
        var chat = _allChats.FirstOrDefault(c => c.Id == message.Value.ChatId);
        if (chat != null)
            chat.Title = message.Value.NewTitle;
    }

    [RelayCommand]
    private void SelectProvider(ProviderType provider)
    {
        SelectedProvider = provider;
    }

    [RelayCommand]
    private async Task NewChat()
    {
        var provider = SelectedProvider ?? ProviderType.OpenAI;
        var entity   = await _repository.CreateAsync("New Chat", provider);

        var model = new ChatModel
        {
            Id        = entity.Id,
            Title     = entity.Title,
            UpdatedAt = entity.UpdatedAt,
            Provider  = entity.Provider
        };

        _allChats.Insert(0, model);
        ApplyFilter();

        SelectedChat = Chats.FirstOrDefault(c => c.Id == model.Id);
    }

    [RelayCommand]
    private async Task DeleteChat(ChatModel? chat)
    {
        if (chat == null) return;

        await _repository.DeleteAsync(chat.Id);

        _allChats.Remove(chat);
        ApplyFilter();
    }
}
