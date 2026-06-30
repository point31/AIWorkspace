using CommunityToolkit.Mvvm.ComponentModel;

namespace AIWorkspace.Services;

public class NavigationService : ObservableObject
{
    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public void Navigate(object view)
    {
        CurrentView = view;
    }
}