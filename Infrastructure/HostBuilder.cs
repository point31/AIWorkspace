using AIWorkspace.AI;
using AIWorkspace.AI.Interfaces;
using AIWorkspace.AI.Providers;
using AIWorkspace.Data;
using AIWorkspace.Repositories;
using AIWorkspace.Services;
using AIWorkspace.Services.AI;
using AIWorkspace.ViewModels;
using AIWorkspace.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace AIWorkspace.Infrastructure;

public static class HostBuilder
{
    public static IHost Build()
    {
        return Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                // Repositories
                services.AddSingleton<ChatRepository>();
                services.AddSingleton<MessageRepository>();
                services.AddSingleton<ProviderSettingsRepository>();

                // Services
                services.AddSingleton<DatabaseService>();
                services.AddSingleton<AIChatService>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<ChatsViewModel>();
                services.AddTransient<ChatSidebarViewModel>();
                services.AddTransient<MessageListViewModel>();
                services.AddTransient<MessageInputViewModel>();
                services.AddTransient<ChatHeaderViewModel>();
                services.AddTransient<StatusBarViewModel>();
                services.AddTransient<NavigationMenuViewModel>();
                services.AddTransient<AIProvidersViewModel>();

                // AI Providers
                services.AddSingleton<IAIProvider, OpenAIProvider>();
                services.AddSingleton<IAIProvider, ClaudeProvider>();
                services.AddSingleton<IAIProvider, GeminiProvider>();

                services.AddSingleton<ProviderManager>();

                // Windows
                services.AddSingleton<MainWindow>();

                // Database
                var dbPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "AIWorkspace.db");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
            })
            .Build();
    }
}