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
using GalaSoft.MvvmLight;
using Nullfactory.PhoneClient.Helpers;
using Microsoft.Practices.ServiceLocation;
using Nullfactory.BookmarkSync.Services;
using GalaSoft.MvvmLight.Command;

namespace Nullfactory.BookmarkSync.ViewModel
{
    public class ImportBookmarksViewModel : ViewModelBase
    {
        #region Services

        INavigationService NavigationService;
        BookmarkServiceBase BookmarkService;

        #endregion 

        public ImportBookmarksViewModel()
        {
            NavigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            BookmarkService = ServiceLocator.Current.GetInstance<BookmarkServiceBase>(BookmarkServiceFormat.ImportHtmlFormat);
        }


        #region Properties

        public const string IsImportInProgressPropertyName = "IsImportInProgress";
        private bool _IsImportInProgress = false;
        public bool IsImportInProgress
        {
            get
            {
                return _IsImportInProgress;
            }

            set
            {
                if (_IsImportInProgress == value)
                {
                    return;
                }

                var oldValue = _IsImportInProgress;
                _IsImportInProgress = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(IsImportInProgressPropertyName);
            }
        }

        public const string ImportFileUrlPropertyName = "ImportFileUrl";
        private string _ImportFileUrl = string.Empty;
        public string ImportFileUrl
        {
            get
            {
                return _ImportFileUrl;
            }

            set
            {
                if (_ImportFileUrl == value)
                {
                    return;
                }

                var oldValue = _ImportFileUrl;
                _ImportFileUrl = value;

                IsImportValidState = App.ViewModelLocator.Main.HasNetworkConnectivity && !string.IsNullOrEmpty(ImportFileUrl);

                RaisePropertyChanged(ImportFileUrlPropertyName);
            }
        }

        public const string IsRememberLastImportFileUrlPropertyName = "IsRememberLastImportFileUrl";
        private bool _IsRememberLastImportFileUrl = false;
        public bool IsRememberLastImportFileUrl
        {
            get
            {
                return _IsRememberLastImportFileUrl;
            }

            set
            {
                if (_IsRememberLastImportFileUrl == value)
                {
                    return;
                }

                var oldValue = _IsRememberLastImportFileUrl;
                _IsRememberLastImportFileUrl = value;

                RaisePropertyChanged(IsRememberLastImportFileUrlPropertyName);
            }
        }

        public const string IsOverwriteExistingPropertyName = "IsOverwriteExisting";
        private bool _IsOverwriteExisting = false;
        public bool IsOverwriteExisting
        {
            get
            {
                return _IsOverwriteExisting;
            }

            set
            {
                if (_IsOverwriteExisting == value)
                {
                    return;
                }

                var oldValue = _IsOverwriteExisting;
                _IsOverwriteExisting = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(IsOverwriteExistingPropertyName);
            }
        }

        public const string IsSkipOnErrorPropertyName = "IsSkipOnError";
        private bool _IsSkipOnError = false;
        public bool IsSkipOnError
        {
            get
            {
                return _IsSkipOnError;
            }

            set
            {
                if (_IsSkipOnError == value)
                {
                    return;
                }

                var oldValue = _IsSkipOnError;
                _IsSkipOnError = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(IsSkipOnErrorPropertyName);
            }
        }

        public const string IsImportValidStatePropertyName = "IsImportValidState";
        private bool _IsImportValidState = false;
        public bool IsImportValidState
        {
            get
            {
                return _IsImportValidState;
            }
            set
            {
                if (_IsImportValidState == value)
                {
                    return;
                }

                var oldValue = _IsImportValidState;
                _IsImportValidState = value;

                RaisePropertyChanged(IsImportValidStatePropertyName);
            }
        }

        #endregion

        #region Methods

        private void LoadSettingsFromIsolatedStorage()
        {
            ImportFileUrl = AppSettings.GetValueOrDefault<string>(AppSettings.Import.FileUrl, string.Empty);
            IsOverwriteExisting = AppSettings.GetValueOrDefault<bool>(AppSettings.Import.OverwriteExisting, false);
            IsRememberLastImportFileUrl = AppSettings.GetValueOrDefault<bool>(AppSettings.Import.RememberLastFile, false);
            IsSkipOnError = AppSettings.GetValueOrDefault<bool>(AppSettings.Import.SkipOnError, false);
        }

        private void SaveSettingsToIsolatedStorage()
        {
            if (_IsRememberLastImportFileUrl) 
                AppSettings.AddOrUpdateValue(AppSettings.Import.FileUrl, _ImportFileUrl);
            else
                AppSettings.AddOrUpdateValue(AppSettings.Import.FileUrl, string.Empty);

            AppSettings.AddOrUpdateValue(AppSettings.Import.OverwriteExisting, _IsOverwriteExisting);
            AppSettings.AddOrUpdateValue(AppSettings.Import.RememberLastFile, _IsRememberLastImportFileUrl);
            AppSettings.AddOrUpdateValue(AppSettings.Import.SkipOnError, _IsSkipOnError);
        }

        private void ReInitializeBookmarkServiceOnFileUrl()
        {
            if (string.IsNullOrEmpty(_ImportFileUrl))
                return;

            if (_ImportFileUrl.EndsWith(".csv"))
                BookmarkService = ServiceLocator.Current.GetInstance<BookmarkServiceBase>(BookmarkServiceFormat.ImportCsvFormat);
            else
                BookmarkService = ServiceLocator.Current.GetInstance<BookmarkServiceBase>(BookmarkServiceFormat.ImportHtmlFormat);
        }

        #endregion

        #region Events

        private ICommand _ImportBookmarks;
        public ICommand ImportBookmarks
        {
            get
            {
                return _ImportBookmarks ?? (_ImportBookmarks = new RelayCommand(() => {

                    IsImportInProgress = true;

                    SaveSettingsToIsolatedStorage();

                    Uri fileLocation = new Uri(ImportFileUrl);
                    ReInitializeBookmarkServiceOnFileUrl();

                    BookmarkService.ImportExternalBookmarks(fileLocation, IsSkipOnError, (externalBookmarks, importStatus) =>
                        {
                            switch (importStatus.StatusCode)
                            {

                                case (ImportStatusCode.FailParsingError):
                                    {
                                        MessageBox.Show("Error while parsing file.", "Failure", MessageBoxButton.OK);
                                        break;
                                    }
                                case (ImportStatusCode.FailAccessError):
                                    {
                                        MessageBox.Show("Unable to connect to remote server or file was not found. Please ensure that the url provided is valid.", "Failure", MessageBoxButton.OK);
                                        break;
                                    }
                                case (ImportStatusCode.Success):
                                {
                                    foreach (Bookmark bookmark in externalBookmarks)
                                    {
                                        try
                                        {
                                            BookmarkService.CreateOrUpdateDeviceBookmark(string.Empty, bookmark);
                                        }
                                        catch
                                        {
                                            if (IsSkipOnError)
                                                importStatus.SkipCount++;
                                            else
                                                break;
                                        }
                                    }

                                    MessageBox.Show(string.Format("{0} of {1} Bookmarks Successfully Imported! {2} Skipped. ", importStatus.SuccessCount, importStatus.PotentialCount, importStatus.SkipCount), "Success", MessageBoxButton.OK);
                                    break;
                                }
                            }

                            IsImportInProgress = false;
                        });
                }));
            }
        }

        private ICommand _PerformIntializationCommand;
        public ICommand PerformInitializationCommand
        {
            get
            {
                return _PerformIntializationCommand ?? (_PerformIntializationCommand  = new RelayCommand(() =>
                    {
                        LoadSettingsFromIsolatedStorage();
                    }
                ));
            }
        }

        #endregion

    }
}
