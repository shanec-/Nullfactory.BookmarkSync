using System;
using System.Windows.Navigation;

namespace Nullfactory.PhoneClient.Helpers
{
    public interface INavigationService
    {
        event NavigatingCancelEventHandler Navigating;
        void NavigateTo(Uri uri);
        void GoBack();
        void RemoveBackEntry();
        void EmptyBackStack();
    }
}
