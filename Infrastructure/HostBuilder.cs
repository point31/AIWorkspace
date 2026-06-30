using AIWorkspace.Repositories;
using AIWorkspace.ViewModels;
using AIWorkspace.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIWorkspace.Infrastructure;

public static class HostBuilder
{
    public static IHost Build()
    {
        return Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
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

            })
            .Build();
    }
}