using AIWorkspace.Infrastructure;
using AIWorkspace.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AIWorkspace.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Host.Services.GetRequiredService<MainWindowViewModel>();
    }
}