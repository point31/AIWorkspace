using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using AIWorkspace.Messages;
using AIWorkspace.Models;
using AIWorkspace.Views;

namespace AIWorkspace.ViewModels;

public partial class MainWindowViewModel :
    ObservableObject,
    IRecipient<NavigationChangedMessage>
{
    [ObservableProperty]
    private object? currentView;
    private readonly IMessenger _messenger;

    public MainWindowViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register(this);

        CurrentView = new HomeView();
       
    }

    public void Receive(NavigationChangedMessage message)
    {
        switch (message.Value)
        {
            case NavigationPage.Home:
                CurrentView = new HomeView();
                break;

            case NavigationPage.Chats:
                CurrentView = new ChatsView();
                break;

            case NavigationPage.PromptLibrary:
                CurrentView = new PromptLibraryView();
                break;

            case NavigationPage.AIProviders:
                CurrentView = new AIProvidersView();
                break;

            case NavigationPage.Files:
                CurrentView = new FilesView();
                break;

            case NavigationPage.Settings:
                CurrentView = new SettingsView();
                break;
        }
    }


}