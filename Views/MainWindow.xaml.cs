using AIWorkspace.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AIWorkspace.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();

            DataContext = App.Host.Services.GetRequiredService<MainWindowViewModel>();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup Error");
            throw;
        }
    }
}