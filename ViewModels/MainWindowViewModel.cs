using CommunityToolkit.Mvvm.ComponentModel;
using AIWorkspace.Models;
using System.Collections.ObjectModel;

namespace AIWorkspace.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<NavigationItemModel> NavigationItems { get; } =
[
    new()
    {
        Title="Home",
        Icon="\uE80F"
    },

    new()
    {
        Title="Chats",
        Icon="\uE8BD"
    },

    new()
    {
        Title="Prompt Library",
        Icon="\uE7C3"
    },

    new()
    {
        Title="AI Providers",
        Icon="\uE9CA"
    },

    new()
    {
        Title="Files",
        Icon="\uE8B7"
    },

    new()
    {
        Title="Settings",
        Icon="\uE713"
    }
    ];
}