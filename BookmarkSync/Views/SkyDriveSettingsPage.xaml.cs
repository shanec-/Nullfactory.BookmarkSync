using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Nullfactory.BookmarkSync;
using Microsoft.Live.Controls;
using Microsoft.Live;

namespace Nullfactory.BookmarkSync.Views
{
    public partial class SkyDriveSettingsPage : PhoneApplicationPage
    {
        public SkyDriveSettingsPage()
        {
            InitializeComponent();

            DataContext = App.ViewModelLocator.SkyDriveSettingsViewModel;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {

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
