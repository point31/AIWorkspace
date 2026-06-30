using CommunityToolkit.Mvvm.ComponentModel;
using AIWorkspace.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using AIWorkspace.Messages;

namespace AIWorkspace.ViewModels;

public partial class NavigationMenuViewModel : ObservableObject
{
    public ObservableCollection<NavigationItemModel> Items { get; } =
    [
        new()
        {
            Page = NavigationPage.Home,
            Title = "Home",
            Icon = "\uE80F",
        },

        new()
        {
            Page = NavigationPage.Chats,
            Title = "Chats",
            Icon = "\uE8BD"
        },

        new()
        {
            Page = NavigationPage.PromptLibrary,
            Title = "Prompt Library",
            Icon = "\uE7C3"
        },

        new()
        {
            Page = NavigationPage.AIProviders,
            Title = "AI Providers",
            Icon = "\uE9CA"
        },

        new()
        {
            Page = NavigationPage.Files,
            Title = "Files",
            Icon = "\uE8B7"
        },

        new()
        {
            Page = NavigationPage.Settings,
            Title = "Settings",
            Icon = "\uE713"
        }
    ];

    public NavigationMenuViewModel()
    {
        SelectedItem = Items[0];
    }


    [ObservableProperty]
    private NavigationItemModel? selectedItem;

    partial void OnSelectedItemChanged(
    NavigationItemModel? oldValue,
    NavigationItemModel? newValue)
    {
        if (oldValue != null)
            oldValue.IsSelected = false;

        if (newValue != null)
        {
            newValue.IsSelected = true;

            WeakReferenceMessenger.Default.Send(
                new NavigationChangedMessage(newValue.Page));
        }
    }
}