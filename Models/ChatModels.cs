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
    private string provider = "";

    public string ProviderIcon =>
        Provider switch
        {
            "OpenAI" => "🤖",
            "Anthropic" => "🧠",
            "Google" => "✨",
            "OpenRouter" => "🟣",
            "Ollama" => "🦙",
            "LM Studio" => "💻",
            _ => "💬"
        };
}