using System.ComponentModel;
using System;
using Nullfactory.BookmarkSync.Services;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using Nullfactory.PhoneClient.Helpers;
using System.Windows.Input;

namespace Nullfactory.BookmarkSync.ViewModel
{

    public class CreateEditBookmarkViewModel : ViewModelBase
    {
        #region Services

        BookmarkServiceBase BookmarkService {get; set;}
        INavigationService NavigationService { get; set; }
        private string oldTitle = string.Empty;

        #endregion

        /// <summary>
        /// Initializes a new instance of the AddBookmarkViewModel class.
        /// </summary>
        public CreateEditBookmarkViewModel()
        {
            BookmarkService = ServiceLocator.Current.GetInstance<BookmarkServiceBase>(BookmarkServiceFormat.ImportHtmlFormat);
            NavigationService = ServiceLocator.Current.GetInstance<INavigationService>();
        }


        #region Properties

        private string _Title = string.Empty;
        public string Title
        { 
            get 
            { 
                return _Title; 
            }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    IsValidEntry = !string.IsNullOrEmpty(_Title) && !string.IsNullOrEmpty(_Url);
                    RaisePropertyChanged("Title");
                }
            }
        }

        private string _Url = string.Empty;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                if (value != _Url)
                {
                    _Url = value;
                    IsValidEntry = !string.IsNullOrEmpty(_Title) && !string.IsNullOrEmpty(_Url);
                    RaisePropertyChanged("Url");
                }
            }
        }

        private bool _IsValidEntry = false;
        public bool IsValidEntry
        {
            get
            {
                return _IsValidEntry;
            }
            set
            {
                if (value != IsValidEntry)
                {
                    _IsValidEntry = value;

                    (DoneCommand as RelayCommand).RaiseCanExecuteChanged();
                    RaisePropertyChanged("IsValidEntry");
                }
            }
        }

        #endregion


        #region Methods

        public void CreateOrUpdateBookmark()
        {
            BookmarkService.CreateOrUpdateDeviceBookmark(oldTitle, new Bookmark() { Title = _Title, Url = _Url });
        }

        #endregion


        #region Events

        private ICommand _DoneCommand;
        private ICommand _CancelCommand;
        private ICommand _PerformIntializationCommand;

        public ICommand DoneCommand
        {
            get
            {
                return _DoneCommand ?? (_DoneCommand = new RelayCommand(() =>
                {
                    this.CreateOrUpdateBookmark();
                    NavigationService.GoBack();
                    App.ViewModelLocator.Main.RefreshDeviceBookmarks();
                },
                () => { return IsValidEntry; }));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _CancelCommand ?? (_CancelCommand =  new RelayCommand(() =>
                {
                    NavigationService.GoBack();
                }));
            }
        }

        public ICommand PerformInitializationCommand
        {
            get
            {
                return _PerformIntializationCommand ?? (_PerformIntializationCommand = new RelayCommand(() =>
                    {
                        oldTitle = _Title;
                    }));
            }
        }

        #endregion
    }
}