using System.Windows;
using System.Windows.Controls;

namespace AIWorkspace.Controls;

public partial class PrimaryButton : UserControl
{
    public PrimaryButton()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PrimaryButton),
            new PropertyMetadata("Button"));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}