using AIWorkspace.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace AIWorkspace.Views;

public partial class ChatsView : UserControl
{
    public ChatsView()
    {
        InitializeComponent();

        DataContext = App.Host.Services.GetRequiredService<ChatsViewModel>();
    }
}