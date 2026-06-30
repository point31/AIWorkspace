using AIWorkspace.AI;
using AIWorkspace.Security;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIWorkspace.Models;

public partial class ProviderSettingsModel : ObservableObject
{
    public ProviderType Provider { get; set; }

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private string apiKey = "";

    [ObservableProperty]
    private string defaultModel = "";

    /// <summary>True when the user is actively editing the API key field.</summary>
    [ObservableProperty]
    private bool isEditingKey;

    /// <summary>Human-readable name shown in the UI.</summary>
    public string ProviderName => Provider switch
    {
        ProviderType.OpenAI => "OpenAI",
        ProviderType.Claude => "Claude (Anthropic)",
        ProviderType.Gemini => "Gemini (Google)",
        _ => Provider.ToString()
    };

    /// <summary>Icon/emoji for the provider.</summary>
    public string ProviderIcon => Provider switch
    {
        ProviderType.OpenAI => "🤖",
        ProviderType.Claude => "🟣",
        ProviderType.Gemini => "✨",
        _ => "🔑"
    };

    /// <summary>Masked key for display when not editing.</summary>
    public string MaskedKey => SecureStorageService.Mask(ApiKey);
}
