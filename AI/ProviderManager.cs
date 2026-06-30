using AIWorkspace.AI.Interfaces;
using AIWorkspace.Repositories;

namespace AIWorkspace.AI;

public class ProviderManager
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly ProviderSettingsRepository _settings;

    public ProviderManager(
        IEnumerable<IAIProvider> providers,
        ProviderSettingsRepository settings)
    {
        _providers = providers;
        _settings = settings;
    }

    public IAIProvider GetProvider(ProviderType provider)
    {
        return _providers.First(x => x.Provider == provider);
    }

    /// <summary>
    /// Loads all saved API keys and model names from the database and
    /// applies them to each registered provider. Call this on app startup
    /// and whenever the user saves new provider settings.
    /// </summary>
    public async Task ApplySettingsAsync()
    {
        var allSettings = await _settings.GetAllAsync();

        foreach (var setting in allSettings)
        {
            var provider = _providers.FirstOrDefault(p => p.Provider == setting.Provider);
            provider?.Configure(setting.ApiKey, setting.DefaultModel);
        }
    }
}
