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

        DatabaseService.Initialize();

        Host = Infrastructure.HostBuilder.Build();

        await Host.StartAsync();

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