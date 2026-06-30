using AIWorkspace.Infrastructure;
using AIWorkspace.Services;
using AIWorkspace.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AIWorkspace;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DatabaseService.Initialize();

        ServiceLocator.Configure();

        var mainWindow = ServiceLocator.Services.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }
}