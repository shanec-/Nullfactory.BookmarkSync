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
using Microsoft.Phone.Tasks;

namespace Nullfactory.BookmarkSync.Views
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void GoWebsite_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri(@"http://www.nullfactory.net");
            task.Show();
        }

        private void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask sendFeedbackTask = new EmailComposeTask();
            sendFeedbackTask.Subject = "[Bookmark Sync] Feedback";
            sendFeedbackTask.To = "nullfactory@outlook.com";
            sendFeedbackTask.Show();
        }
    }
}
