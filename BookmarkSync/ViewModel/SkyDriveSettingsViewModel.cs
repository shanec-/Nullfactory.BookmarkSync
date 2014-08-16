using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Nullfactory.PhoneClient.Helpers;
using Microsoft.Phone.Info;
using Microsoft.Live;
using System.Collections.ObjectModel;
using Services = Nullfactory.BookmarkSync.Services;
using System.Linq;
using System;
using Microsoft.Phone.Tasks;

namespace Nullfactory.BookmarkSync.ViewModel
{
    public class SkyDriveSettingsViewModel : ViewModelBase
    {
        private bool _HasSyncFileNameChanged = false;

        public SkyDriveSettingsViewModel()
        {
            this.SyncDirectionOptions = new ObservableCollection<DropDownViewModel>();
            
            this.SyncDirectionOptions.Add(new DropDownViewModel() { DisplayValue = "Device to SkyDrive", Value = Services.SyncDirection.DeviceToCloud.ToString() });
            this.SyncDirectionOptions.Add(new DropDownViewModel() { DisplayValue = "SkyDrive to Device", Value = Services.SyncDirection.CloudToDevice.ToString() });
        }

        #region Properties

        public ObservableCollection<DropDownViewModel> SyncDirectionOptions{ get; private set; }

        public const string SyncFilenamePropertyName = "SyncFilename";
        private string _SyncFileName = string.Empty;
        public string SyncFilename
        {
            get
            {
                return _SyncFileName;
            }

            set
            {
                if (_SyncFileName == value)
                {
                    return;
                }

                var oldValue = _SyncFileName;
                _SyncFileName = value;
                _HasSyncFileNameChanged = true;
                
                RaisePropertyChanged(SyncFilenamePropertyName);
            }
        }


        public const string SelectedSyncDirectionPropertyName = "SelectedSyncDirection";
        private DropDownViewModel _SelectedSyncDirection = null;
        public DropDownViewModel SelectedSyncDirection
        {
            get
            {
                return _SelectedSyncDirection ?? (_SelectedSyncDirection = SyncDirectionOptions[0]);
            }

            set
            {
                if (_SelectedSyncDirection == value)
                {
                    return;
                }

                var oldValue = _SelectedSyncDirection;
                _SelectedSyncDirection = value;

                RaisePropertyChanged(SelectedSyncDirectionPropertyName);
            }
        }

        #endregion

        #region Events

        private ICommand _PerformInitializationCommand;
        public ICommand PerformInitializationCommand
        {
            get
            {
                return _PerformInitializationCommand ?? (_PerformInitializationCommand = new AsyncRelayCommand(() =>
                {
                    _SyncFileName = AppSettings.GetValueOrDefault<string>(AppSettings.CloudSync.SkyDriveSyncFilename, string.Empty);
                    if (string.IsNullOrEmpty(_SyncFileName))
                    {
                        string defaultSyncFileName = FileService.ConvertToValidFileName(string.Concat(DeviceStatus.DeviceName, "-bookmarks.txt"));
                        _SyncFileName = defaultSyncFileName;
                        _HasSyncFileNameChanged = true;
                    }

                    //Load up the Saved Sync Direction
                    Services.SyncDirection savedDirection = AppSettings.GetValueOrDefault<Services.SyncDirection>(AppSettings.CloudSync.SyncDirection, Services.SyncDirection.DeviceToCloud);
                    _SelectedSyncDirection = SyncDirectionOptions.Where(x => x.Value == savedDirection.ToString()).FirstOrDefault();

                    BackgroundHelper.ExecuteUIThread(() => { 
                        RaisePropertyChanged(SyncFilenamePropertyName);
                        RaisePropertyChanged(SelectedSyncDirectionPropertyName);
                    });
                }));
            }
        }

        private ICommand _PerformFinalizationCommand;
        public ICommand PerformFinalizationCommand
        {
            get
            {
                return _PerformFinalizationCommand ?? (_PerformFinalizationCommand = new AsyncRelayCommand(() => {
                    if (_HasSyncFileNameChanged)
                    {
                        AppSettings.AddOrUpdateValue(AppSettings.CloudSync.SkyDriveSyncFilename, _SyncFileName);

                        //if the filename has been changed from the previous one, 
                        //then a new skydrive internal reference must be established
                        AppSettings.AddOrUpdateValue(AppSettings.CloudSync.SkyDriveInternalSyncFilename, string.Empty);
                    }

                    //Save the Sync Direction Mode
                    Services.SyncDirection selectedEnum = (Services.SyncDirection)Enum.Parse(typeof(Services.SyncDirection), SelectedSyncDirection.Value, true);
                    AppSettings.AddOrUpdateValue(AppSettings.CloudSync.SyncDirection, selectedEnum);
                }));
            }
        }

        private ICommand _DisplaySyncLimitationInformation;
        public ICommand DisplaySyncLimitationInformation
        {
            get
            {
                return _DisplaySyncLimitationInformation ?? (_DisplaySyncLimitationInformation = new RelayCommand(()=> {
                    WebBrowserTask task = new WebBrowserTask();
                    task.Uri = new Uri(@"http://msdn.microsoft.com/en-us/library/live/hh826545.aspx#fileformats");
                    task.Show();
                }));
            }
        }

        #endregion
    }
}