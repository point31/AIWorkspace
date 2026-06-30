using AIWorkspace.AI;
using AIWorkspace.Entities;
using AIWorkspace.Models;
using AIWorkspace.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AIWorkspace.ViewModels;

public partial class AIProvidersViewModel : ObservableObject
{
    private readonly ProviderSettingsRepository _repository;
    private readonly ProviderManager _providerManager;

    /// <summary>Providers that have been configured (have an API key).</summary>
    public ObservableCollection<ProviderSettingsModel> ActiveProviders { get; } = [];

    /// <summary>Providers not yet configured — shown in the "Add" dropdown.</summary>
    public ObservableCollection<ProviderType> AvailableProviders { get; } = [];

    [ObservableProperty]
    private ProviderType? selectedProviderToAdd;

    [ObservableProperty]
    private string newApiKey = "";

    [ObservableProperty]
    private bool isAddPanelOpen;

    public AIProvidersViewModel(
        ProviderSettingsRepository repository,
        ProviderManager providerManager)
    {
        _repository = repository;
        _providerManager = providerManager;

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        ActiveProviders.Clear();
        AvailableProviders.Clear();

        var all = await _repository.GetAllAsync();

        var configured = all.Where(x => !string.IsNullOrEmpty(x.ApiKey)).ToList();
        var notConfigured = all.Where(x => string.IsNullOrEmpty(x.ApiKey)).ToList();

        foreach (var p in configured)
        {
            ActiveProviders.Add(new ProviderSettingsModel
            {
                Provider     = p.Provider,
                ApiKey       = p.ApiKey,
                DefaultModel = p.DefaultModel,
                IsEnabled    = p.IsEnabled
            });
        }

        foreach (var p in notConfigured)
            AvailableProviders.Add(p.Provider);
    }

    [RelayCommand]
    private void OpenAddPanel()
    {
        SelectedProviderToAdd = AvailableProviders.FirstOrDefault();
        NewApiKey = "";
        IsAddPanelOpen = true;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddPanelOpen = false;
        NewApiKey = "";
    }

    [RelayCommand]
    private async Task ConfirmAdd()
    {
        if (SelectedProviderToAdd is null || string.IsNullOrWhiteSpace(NewApiKey))
            return;

        var providerType = SelectedProviderToAdd.Value;

        // Persist
        await _repository.SaveAsync(new ProviderSettingsEntity
        {
            Provider     = providerType,
            IsEnabled    = true,
            ApiKey       = NewApiKey,
            DefaultModel = DefaultModelFor(providerType)
        });

        // Move from available → active
        AvailableProviders.Remove(providerType);

        ActiveProviders.Add(new ProviderSettingsModel
        {
            Provider     = providerType,
            ApiKey       = NewApiKey,
            DefaultModel = DefaultModelFor(providerType),
            IsEnabled    = true
        });

        IsAddPanelOpen = false;
        NewApiKey = "";

        await _providerManager.ApplySettingsAsync();
    }

    [RelayCommand]
    private async Task Save(ProviderSettingsModel model)
    {
        await _repository.SaveAsync(new ProviderSettingsEntity
        {
            Provider     = model.Provider,
            IsEnabled    = model.IsEnabled,
            ApiKey       = model.ApiKey,
            DefaultModel = model.DefaultModel
        });

        await _providerManager.ApplySettingsAsync();
    }

    [RelayCommand]
    private async Task Remove(ProviderSettingsModel model)
    {
        await _repository.DeleteAsync(model.Provider);

        ActiveProviders.Remove(model);
        AvailableProviders.Add(model.Provider);

        await _providerManager.ApplySettingsAsync();
    }

    [RelayCommand]
    private void ToggleEditKey(ProviderSettingsModel model)
    {
        model.IsEditingKey = !model.IsEditingKey;
    }

    private static string DefaultModelFor(ProviderType provider) => provider switch
    {
        ProviderType.OpenAI => "gpt-4o-mini",
        ProviderType.Claude => "claude-3-5-sonnet-20241022",
        ProviderType.Gemini => "gemini-2.0-flash",
        _                   => ""
    };
}
