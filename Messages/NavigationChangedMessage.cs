using CommunityToolkit.Mvvm.Messaging.Messages;
using AIWorkspace.Models;

namespace AIWorkspace.Messages;

public class NavigationChangedMessage : ValueChangedMessage<NavigationPage>
{
    public NavigationChangedMessage(NavigationPage value)
        : base(value)
    {
    }
}