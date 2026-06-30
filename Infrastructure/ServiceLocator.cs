using AIWorkspace.Repositories;
using AIWorkspace.ViewModels;
using AIWorkspace.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AIWorkspace.Infrastructure;

public static class ServiceLocator
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Configure()
    {
        var services = new ServiceCollection();

        // Repositories
        services.AddSingleton<ChatRepository>();
        services.AddSingleton<MessageRepository>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ChatsViewModel>();
        services.AddTransient<ChatSidebarViewModel>();
        services.AddTransient<MessageListViewModel>();
        services.AddTransient<MessageInputViewModel>();
        services.AddTransient<ChatHeaderViewModel>();
        services.AddTransient<StatusBarViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();
    }
}