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

namespace Nullfactory.PhoneClient.Helpers
{
    public partial class AppSettings
    {
        public static class Import
        {
            public const string FileUrl = "Nullfactory.BookmarkSync.Import.FileUrl";
            public const string RememberLastFile = "Nullfactory.BookmarkSync.Import.RememberLastFile";
            public const string OverwriteExisting = "Nullfactory.BookmarkSync.Import.OverwriteExisting";
            public const string SkipOnError = "Nullfactory.BookmarkSync.Import.SkipOnError";
        }

        public static class CloudSync
        {
            public const string SkyDriveSyncFilename = "Nullfactory.BookmarkSync.CloudSync.SkyDriveSyncFilename";
            public const string SkyDriveInternalSyncFilename = "Nullfactory.BookmarkSync.CloudSync.SkyDriveInternalSyncFilename";
            public const string SyncDirection = "Nullfactory.BookmarkSync.CloudSync.SyncDirection";
        }
    }
}
