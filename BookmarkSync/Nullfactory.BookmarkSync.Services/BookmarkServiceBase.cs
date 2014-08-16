using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WP7RootToolsSDK;
using System.Diagnostics;
using Nullfactory.PhoneClient.Helpers;

namespace Nullfactory.BookmarkSync.Services
{
    public enum ImportStatusCode
    {
        FailAccessError,
        FailParsingError,
        Success
    }

    public enum SyncDirection
    {
        DeviceToCloud,
        CloudToDevice
    }

    public static class BookmarkServiceFormat
    {
        public const string ImportCsvFormat = "ImportCSVFormat";
        public const string ImportHtmlFormat = "ImportHtmlFormat";
    }

    public class BookmarkserviceImportCompletedEventArgs
    {
        public ImportStatusCode StatusCode { get; set; }
        public Exception Exception { get; set; }
        public int PotentialCount { get; set; }
        public int SuccessCount { get; set; }
        public int SkipCount { get; set; }
    }

    public abstract class BookmarkServiceBase
    {
        #region constants
        const string FAVORITES_PATH = @"\Windows\Favorites";
        #endregion 

        #region abstract methods

        public abstract void ImportExternalBookmarks(Uri fileLocation, bool skipOnError, Action<List<Bookmark>, BookmarkserviceImportCompletedEventArgs> completedAction);

        #endregion

        public List<Bookmark> RetrieveDeviceBookmarks()
        {
            List<Bookmark> internalBookmarks = new List<Bookmark>();

#if DEBUG
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
            {
                internalBookmarks = AppSettings.GetValueOrDefault<List<Bookmark>>("EmulatedBookmarks", internalBookmarks);
                return internalBookmarks;
            }
#endif

            Folder favourites = WP7RootToolsSDK.FileSystem.GetFolder(FAVORITES_PATH);
            List<FileSystemEntry> files = favourites.GetSubItems();
            foreach (FileSystemEntry file in files)
            {
                if (file.IsFile)
                {
                    byte [] buffer = FileSystem.ReadFile(file.Path);
                    string urlFileContent = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    string url = ExtractUrl(urlFileContent);

                    string bookmarkName = file.Name.Substring(0, file.Name.LastIndexOf(".url"));

                    internalBookmarks.Add(new Bookmark() { Title = bookmarkName, Url= url});
                }
            }

            return internalBookmarks;
        }

        public void CreateOrUpdateDeviceBookmark(string oldTitle, Bookmark bookmark)
        {
#if DEBUG
            //todo: replace with mock types
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
            {
                List<Bookmark> internalBookmarks = new List<Bookmark>();
                internalBookmarks = AppSettings.GetValueOrDefault<List<Bookmark>>("EmulatedBookmarks", internalBookmarks);
                internalBookmarks.Add(bookmark);

                AppSettings.AddOrUpdateValue("EmulatedBookmarks", internalBookmarks);

                return;
            }
#endif
            //If the filename has been renamed, delete the previous file
            if(!string.IsNullOrEmpty(oldTitle) && !oldTitle.Equals(bookmark.Title, StringComparison.InvariantCultureIgnoreCase))
                DeleteDeviceBookmark(oldTitle);

            //Ensure that the file name is a valid one
            string fileName = FileService.ConvertToValidFileName(string.Concat(bookmark.Title, ".url"));
            fileName = System.IO.Path.Combine(FAVORITES_PATH, fileName);

            string fileContent = string.Concat("[InternetShortcut]", System.Environment.NewLine, "URL=", bookmark.Url, System.Environment.NewLine);
            byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

            WP7RootToolsSDK.FileSystem.WriteFile(fileName, buffer);
        }

        public void DeleteDeviceBookmark(string title)
        {
#if DEBUG
            return;
#endif 
            string fileName = string.Concat(title, ".url");
            fileName = System.IO.Path.Combine(FAVORITES_PATH, fileName);

            if (WP7RootToolsSDK.FileSystem.FileExists(fileName))
                WP7RootToolsSDK.FileSystem.DeleteFile(fileName);
        }

        protected string ExtractUrl(string urlFileContent)
        {
            int urlStartPos = urlFileContent.LastIndexOf("URL=");
            string url = urlFileContent.Substring(urlStartPos + 4);
            url = url.Trim();
            return url;
        }
    }
}
