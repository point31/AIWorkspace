using AIWorkspace.AI;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIWorkspace.Models;

public partial class ChatModel : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private DateTime updatedAt;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private ProviderType provider;

    public string ProviderDisplay =>
        Provider switch
        {
            ProviderType.OpenAI => "🤖 OpenAI",
            ProviderType.Claude => "🟣 Claude",
            ProviderType.Gemini => "✨ Gemini",
            _ => "Unknown"
        };

    public string ProviderIcon =>
        Provider switch
        {
            ProviderType.OpenAI => "🤖",
            ProviderType.Claude => "🟣",
            ProviderType.Gemini => "✨",
            _ => "❓"
        };
}