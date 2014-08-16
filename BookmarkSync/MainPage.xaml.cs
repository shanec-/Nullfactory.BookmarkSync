using Microsoft.Live;
using Microsoft.Live.Controls;
using Microsoft.Phone.Controls;

namespace Nullfactory.BookmarkSync
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModelLocator.Main;
        }

        private void SignInButton_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Session != null && e.Status == LiveConnectSessionStatus.Connected)
            {
                App.LiveConnectSession = e.Session;
                App.ViewModelLocator.Main.HasSignedIntoCloudService = true;
            }
            else
            {
                App.ViewModelLocator.Main.HasSignedIntoCloudService = false;
            }
        }
    }
}