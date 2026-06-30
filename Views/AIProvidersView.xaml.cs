using AIWorkspace.Models;
using AIWorkspace.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AIWorkspace.Views;

public partial class AIProvidersView : UserControl
{
    public AIProvidersView()
    {
        InitializeComponent();
        DataContext = App.Host.Services.GetRequiredService<AIProvidersViewModel>();
    }

    private AIProvidersViewModel Vm => (AIProvidersViewModel)DataContext;

    // Wires the "Add provider" PasswordBox to the ViewModel's NewApiKey property.
    private void NewKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
            Vm.NewApiKey = box.Password;
    }

    // Wires the per-card "Edit Key" PasswordBox to the model's ApiKey property.
    private void EditKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box && box.Tag is ProviderSettingsModel model)
            model.ApiKey = box.Password;
    }
}
