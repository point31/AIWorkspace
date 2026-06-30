using CommunityToolkit.Mvvm.ComponentModel;

namespace AIWorkspace.Models;

public partial class MessageModel : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string role = "";

    [ObservableProperty]
    private string content = "";

    [ObservableProperty]
    private DateTime createdAt;
}