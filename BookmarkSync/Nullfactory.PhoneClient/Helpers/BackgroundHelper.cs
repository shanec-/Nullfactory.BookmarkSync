using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace Nullfactory.PhoneClient.Helpers
{
    public static class BackgroundHelper
    {
        public static void Execute(Action action)
        {
            ThreadPool.QueueUserWorkItem((o)=> {
                action.Invoke();
            });
        }

        public static void ExecuteUIThread(Action action)
        {
            //This means that we're executing in the UI thread
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        action.Invoke();
                    });
            }
        }
    }
}
