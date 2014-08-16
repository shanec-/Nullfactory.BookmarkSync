/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Nullfactory.BookmarkSync"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Nullfactory.BookmarkSync.Services;
using System;
using Nullfactory.PhoneClient.Helpers;

namespace Nullfactory.BookmarkSync.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<INavigationService>(() => { return new NavigationService(); });

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<CreateEditBookmarkViewModel>();
            SimpleIoc.Default.Register<ImportBookmarksViewModel>();
            SimpleIoc.Default.Register<SkyDriveSettingsViewModel>();
            
            SimpleIoc.Default.Register<BookmarkServiceBase>(() => { return new CsvBookmarkService(); }, BookmarkServiceFormat.ImportCsvFormat);
            SimpleIoc.Default.Register<BookmarkServiceBase>(() => { return new HtmlBookmarkService(); }, BookmarkServiceFormat.ImportHtmlFormat);
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public CreateEditBookmarkViewModel CreateEditBookmarkViewModel
        {
            get { return ServiceLocator.Current.GetInstance<CreateEditBookmarkViewModel>(); }
        }

        public ImportBookmarksViewModel ImportBookmarksViewModel
        {
            get { return ServiceLocator.Current.GetInstance<ImportBookmarksViewModel>(); }
        }

        public SkyDriveSettingsViewModel SkyDriveSettingsViewModel
        {
            get { return ServiceLocator.Current.GetInstance<SkyDriveSettingsViewModel>(); }
        }
        
        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<MainViewModel>();
            SimpleIoc.Default.Unregister<CreateEditBookmarkViewModel>();
            SimpleIoc.Default.Unregister<ImportBookmarksViewModel>();
        }

        public static void Cleanup<TClass>() where TClass : ViewModelBase
        {
            SimpleIoc.Default.Unregister<TClass>();
        }

        public class Pages
        {
            public static readonly Uri MainPage = new Uri("/MainPage.xaml", UriKind.Relative);
            public static readonly Uri CreateEditBookmarkPage = new Uri("/Views/CreateEditBookmarkPage.xaml", UriKind.Relative);
            public static readonly Uri ImportBookmarkPage = new Uri("/Views/ImportBookmarksPage.xaml", UriKind.Relative);
            public static readonly Uri AboutPage = new Uri("/Views/AboutPage.xaml", UriKind.Relative);
            public static readonly Uri UnhandledExceptionPage = new Uri("/Views/UnhandledExceptionPage.xaml", UriKind.Relative);
            public static readonly Uri SkyDriveSettingsPage = new Uri("/Views/SkyDriveSettingsPage.xaml", UriKind.Relative);
        }
    }
}