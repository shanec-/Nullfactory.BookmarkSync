using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Microsoft.Practices.ServiceLocation;
using Nullfactory.BookmarkSync.Services;
using System.Collections.Generic;
using Nullfactory.PhoneClient.Helpers;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using System;
using System.Windows.Input;
using System.Windows;
using System.Threading;

namespace Nullfactory.BookmarkSync.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Services

        private BookmarkServiceBase BookmarkService { get; set; }
        private INavigationService NavigationService { get; set; }

        private bool? _HasRootAccess = null;
        private bool? _HasNetworkConnectivity = null;

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            BookmarkService = ServiceLocator.Current.GetInstance<BookmarkServiceBase>(BookmarkServiceFormat.ImportHtmlFormat);
            NavigationService = ServiceLocator.Current.GetInstance<INavigationService>();

            this.DeviceBookmarks = new ObservableCollection<ItemViewModel>();
        }


        #region Properties

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> DeviceBookmarks { get; private set; }

        public const string IsPageEnabledPropertyName = "IsPageEnabled";
        private bool _IsPageEnabled = false;
        public bool IsPageEnabled
        {
            get
            {
                return _IsPageEnabled;
            }
            set
            {
                if (_IsPageEnabled == value)
                {
                    return;
                }

                var oldValue = _IsPageEnabled;
                _IsPageEnabled = value;

                RaisePropertyChanged(IsPageEnabledPropertyName);
            }
        }

        public bool HasRootAccess
        {
            get
            {
                return _HasRootAccess ?? false;
            }
        }

        public bool HasNetworkConnectivity
        {
            get
            {
                return _HasNetworkConnectivity ?? false;
            }
        }

        public const string HasSignedIntoCloudServicePropertyName = "HasSignedIntoCloudService";
        private bool _HasSignedIntoCloudService = false;
        public bool HasSignedIntoCloudService
        {
            get
            {
                return _HasSignedIntoCloudService;
            }

            set
            {
                if (_HasSignedIntoCloudService == value)
                {
                    return;
                }

                var oldValue = _HasSignedIntoCloudService;
                _HasSignedIntoCloudService = value;

                RaisePropertyChanged(HasSignedIntoCloudServicePropertyName);

                //BackgroundHelper.ExecuteUIThread(() =>
                //{
                    (CloudSyncCommand as RelayCommand).RaiseCanExecuteChanged();
                //});
            }
        }

        public const string IsSyncInProgressPropertyName = "IsSyncInProgress";
        private bool _IsSyncInProgress = false;
        public bool IsSyncInProgress
        {
            get
            {
                return _IsSyncInProgress;
            }

            set
            {
                if (_IsSyncInProgress == value)
                {
                    return;
                }

                var oldValue = _IsSyncInProgress;
                _IsSyncInProgress = value;

                RaisePropertyChanged(IsSyncInProgressPropertyName);
            }
        }

        #endregion

        #region Commands

        private ICommand _AboutPageCommand;
        private ICommand _AddDeviceBookmarkCommand;
        private ICommand _RefreshDeviceBookmarksCommand;
        private ICommand _DonateCommand;
        private ICommand _ImportBookmarksCommand;
        private ICommand _EditDeviceBookmarkCommand;
        private ICommand _DeleteDeviceBookmarkCommand;
        private ICommand _EmailLinkCommand;
        private ICommand _PerformInitializationCommand;
        private ICommand _OpenDeviceBookmarkCommand;
        private ICommand _CloudSyncCommand;
        private ICommand _SkyDriveSettingsCommmand;

        public ICommand AddDeviceBookmarkCommand
        {
            get
            {
                return _AddDeviceBookmarkCommand ?? (_AddDeviceBookmarkCommand = new RelayCommand(() =>
                {
                    App.ViewModelLocator.CreateEditBookmarkViewModel.Title = string.Empty;
                    App.ViewModelLocator.CreateEditBookmarkViewModel.Url = string.Empty;

                    NavigationService.NavigateTo(ViewModelLocator.Pages.CreateEditBookmarkPage);
                },
                () => { return IsPageEnabled; }));
            }
        }

        public ICommand AboutPageCommand
        {
            get
            {
                return _AboutPageCommand ?? (_AboutPageCommand = new RelayCommand(() =>
                    {
                        NavigationService.NavigateTo(ViewModelLocator.Pages.AboutPage);
                    }));
            }
        }

        public ICommand RefreshDeviceBookmarksCommand
        {
            get
            {
                return _RefreshDeviceBookmarksCommand ?? (_RefreshDeviceBookmarksCommand = new RelayCommand(() =>
                {
                    this.RefreshDeviceBookmarks();
                },
                () => { return IsPageEnabled; }));
            }
        }

        public ICommand ImportBookmarksCommand
        {
            get
            {
                return _ImportBookmarksCommand ?? (_ImportBookmarksCommand = new RelayCommand(() =>
                {
                    NavigationService.NavigateTo(ViewModelLocator.Pages.ImportBookmarkPage);
                },
                () => { return IsPageEnabled && (_HasRootAccess ?? false); }));
            }
        }

        public ICommand DonateCommand
        {
            get
            {
                return _DonateCommand ?? (_DonateCommand = new RelayCommand(() =>
                {
                    WebBrowserTask task = new WebBrowserTask();
                    task.Uri = new Uri(@"http://nullfactory.net");
                    task.Show();

                }));
            }
        }

        public ICommand EditDeviceBookmarkCommand
        {
            get
            {
                return _EditDeviceBookmarkCommand ?? (_EditDeviceBookmarkCommand = new RelayCommand<ItemViewModel>((selectedBookmark) =>
                {
                    App.ViewModelLocator.CreateEditBookmarkViewModel.Title = selectedBookmark.Title;
                    App.ViewModelLocator.CreateEditBookmarkViewModel.Url = selectedBookmark.Url;

                    NavigationService.NavigateTo(ViewModelLocator.Pages.CreateEditBookmarkPage);
                },
                (selectedBookmark) => { return IsPageEnabled; }));
            }
        }

        public ICommand DeleteDeviceBookmarkCommand
        {
            get
            {
                return _DeleteDeviceBookmarkCommand ?? (_DeleteDeviceBookmarkCommand = new RelayCommand<ItemViewModel>((selectedBookmark) =>
                {
                    BackgroundHelper.Execute(() => {
                        BookmarkService.DeleteDeviceBookmark(selectedBookmark.Title);

                        BackgroundHelper.ExecuteUIThread(() =>
                        {
                            MessageBox.Show(string.Concat(selectedBookmark.Title, " successfully deleted."), "Successfully Deleted!", MessageBoxButton.OK);
                        });

                        RefreshDeviceBookmarks();
                    });
                }));
            }
        }

        public ICommand EmailLinkCommand
        {
            get
            {
                return _EmailLinkCommand ?? (_EmailLinkCommand = new RelayCommand<ItemViewModel>((selectedBookmark) =>
                {
                    var emailLinkTask = new EmailComposeTask();
                    emailLinkTask.Body = string.Concat(selectedBookmark.Title,
                                                        Environment.NewLine,
                                                        selectedBookmark.Url);
                    emailLinkTask.Show();
                }));
            }
        }

        public ICommand PerformInitializationCommand
        {
            get
            {
                return _PerformInitializationCommand ?? (_PerformInitializationCommand = new RelayCommand(() =>
                {
                    if (_HasRootAccess == null || _HasNetworkConnectivity == null)
                    {
                        VerifyApplicationPreRequisites();
                    }
                }));
            }
        }

        public ICommand OpenDeviceBookmarkCommand
        {
            get
            {
                return _OpenDeviceBookmarkCommand ?? (_OpenDeviceBookmarkCommand = new RelayCommand<ItemViewModel>((selectedBookmark) =>
                    {
                        WebBrowserTask task = new WebBrowserTask();
                        task.Uri = new Uri(selectedBookmark.Url);
                        task.Show();
                    }));
            }
        }

        public ICommand CloudSyncCommand
        {
            get
            {
                return _CloudSyncCommand ?? (_CloudSyncCommand = new AsyncRelayCommand(() =>
                {
                    BackgroundHelper.ExecuteUIThread(() => { IsSyncInProgress = true; });
                    (BookmarkService as HtmlBookmarkService).SyncWithSkyDrive(App.LiveConnectSession, (syncCompletedArgs) =>
                    {
                        BackgroundHelper.ExecuteUIThread(() =>
                        { 
                            switch (syncCompletedArgs.StatusCode)
                            {
                                case (ImportStatusCode.FailParsingError):
                                    {
                                        MessageBox.Show("Error while syncing.", "Failure", MessageBoxButton.OK);
                                        break;
                                    }
                                case (ImportStatusCode.FailAccessError):
                                    {
                                        MessageBox.Show("Unable to connect to remote server or file was not found. Please ensure that the url provided is valid.", "Failure", MessageBoxButton.OK);
                                        break;
                                    }
                                case (ImportStatusCode.Success):
                                    {
                                        MessageBox.Show("Sync Completed Sucessfully", "Success", MessageBoxButton.OK);
                                        break;
                                    }
                            }
                            IsSyncInProgress = false; 
                        });
                    });
                    
                },
                () => { return HasSignedIntoCloudService && IsPageEnabled && (_HasRootAccess ?? false); }));
            }
        }

        public ICommand SkyDriveSettingsCommmand
        {
            get
            {
                return _SkyDriveSettingsCommmand ?? (_SkyDriveSettingsCommmand = new RelayCommand(() =>
                    {
                        NavigationService.NavigateTo(ViewModelLocator.Pages.SkyDriveSettingsPage);
                    }));
            }
        }

        #endregion

        #region Methods

        public void RefreshDeviceBookmarks()
        {
            if (IsPageEnabled)
            {
                List<Bookmark> serviceBookmarks  = null;
                BackgroundHelper.Execute(() =>
                {
                    serviceBookmarks = BookmarkService.RetrieveDeviceBookmarks();
                    BackgroundHelper.ExecuteUIThread(() =>
                    {
                        DeviceBookmarks.Clear();
                        foreach (Bookmark bookmark in serviceBookmarks)
                            DeviceBookmarks.Add(new ItemViewModel() { Title = bookmark.Title, Url = bookmark.Url });    
                    });
                });
            }
        }

        public void VerifyApplicationPreRequisites()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                //verify root status
                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                    _HasRootAccess = true;
                else
                    _HasRootAccess = WP7RootToolsSDK.Environment.HasRootAccess();

                _HasNetworkConnectivity = Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsNetworkAvailable;

                // Running on the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    IsPageEnabled = _HasRootAccess.Value;

                    //Refresh the Commands Bounds
                    (AddDeviceBookmarkCommand as RelayCommand).RaiseCanExecuteChanged();
                    (RefreshDeviceBookmarksCommand as RelayCommand).RaiseCanExecuteChanged();
                    (ImportBookmarksCommand as RelayCommand).RaiseCanExecuteChanged();

                    if (!_HasRootAccess.Value)
                    {
                        MessageBox.Show("No root access detected. Please ensure that the device is interop-unlocked and Bookmark Organizer is set to trusted using WP7 Root Tools. Please restart the application for changes to take effect.", "No Root Access!", MessageBoxButton.OK);
                    }

                    if (!_HasNetworkConnectivity.Value)
                    {
                        MessageBox.Show("No internet connectivity detected. Some functionality will be unavailable. Please verify network connections and restrart the application. ", "No connectivity!", MessageBoxButton.OK);
                    }

                    RefreshDeviceBookmarks();
                });
            });
        }

        #endregion
    }
}