using AIWorkspace.AI;
using AIWorkspace.Infrastructure;
using AIWorkspace.Services;
using AIWorkspace.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace AIWorkspace;

public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Host = Infrastructure.HostBuilder.Build();

        await Host.StartAsync();

        // Apply EF Core migrations before anything touches the database.
        var dbService = Host.Services.GetRequiredService<DatabaseService>();
        dbService.Initialize();

        // Load saved API keys into provider instances.
        var providerManager = Host.Services.GetRequiredService<ProviderManager>();
        await providerManager.ApplySettingsAsync();

        var window = Host.Services.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await Host.StopAsync();
        Host.Dispose();
        base.OnExit(e);
    }
}