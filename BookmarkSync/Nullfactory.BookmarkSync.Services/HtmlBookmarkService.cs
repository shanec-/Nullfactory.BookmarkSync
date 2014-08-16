using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Live;
using Nullfactory.PhoneClient.Helpers;

namespace Nullfactory.BookmarkSync.Services
{
    public class HtmlBookmarkService : BookmarkServiceBase
    {
        private string GenerateNetscapBookmarkFile(string title, ICollection<Bookmark> bookmarks)
        {
            string docType = @"<!DOCTYPE NETSCAPE-Bookmark-file-1>";
            string metaHttp = @"<META HTTP-EQUIV=""Content-Type"" CONTENT=""text/html; charset=UTF-8"">";
            string titleText = string.Format("<TITLE>{0}</TITLE>", title);
            string headerTitleText = string.Format("<H1>{0}</H1>{1}<DL><p>", title, Environment.NewLine);
            
            string footerText = "</DL><p>";

            StringBuilder formattedDoc = new StringBuilder();
            formattedDoc.Append(docType); formattedDoc.Append(Environment.NewLine);
            formattedDoc.Append(metaHttp); formattedDoc.Append(Environment.NewLine);
            formattedDoc.Append(titleText); formattedDoc.Append(Environment.NewLine);
            formattedDoc.Append(headerTitleText); formattedDoc.Append(Environment.NewLine);

            foreach (Bookmark bookmark in bookmarks)
            {
                formattedDoc.Append(string.Format(@"<DT><A HREF=""{0}"">{1}</A>", bookmark.Url, bookmark.Title));
                formattedDoc.Append(Environment.NewLine);
            }

            formattedDoc.Append(footerText);

            return formattedDoc.ToString();
        }

        public override void ImportExternalBookmarks(Uri fileLocation, bool skipOnError, Action<List<Bookmark>, BookmarkserviceImportCompletedEventArgs> completedAction)
        {
            HashSet<Bookmark> externalBookmarks = new HashSet<Bookmark>(new UrlComparer<Bookmark>());
            var completedEventArgs = new BookmarkserviceImportCompletedEventArgs() { StatusCode = ImportStatusCode.Success };

            WebClient client = new WebClient();
            client.DownloadStringCompleted += delegate(object o, DownloadStringCompletedEventArgs args)
            {
                //Critical Failure
                if (args.Error != null)
                {
                    completedEventArgs.Exception = args.Error;
                    completedEventArgs.StatusCode = ImportStatusCode.FailAccessError;
                    completedAction(externalBookmarks.ToList(), completedEventArgs);
                    return;
                }

                string unParsedHtml = args.Result;
                ParseHtml(unParsedHtml, skipOnError, externalBookmarks, completedEventArgs);

                completedEventArgs.SuccessCount = externalBookmarks.Count;

                completedAction(externalBookmarks.ToList(), completedEventArgs);
            };

            client.DownloadStringAsync(fileLocation);
        }

        public void SyncWithSkyDrive(LiveConnectSession session, Action<BookmarkserviceImportCompletedEventArgs> completedAction)
        {
            string syncFilename = AppSettings.GetValueOrDefault(AppSettings.CloudSync.SkyDriveSyncFilename, string.Empty);
            string syncInternalFilename = AppSettings.GetValueOrDefault(AppSettings.CloudSync.SkyDriveInternalSyncFilename, string.Empty);
            SyncDirection syncDirection = AppSettings.GetValueOrDefault<SyncDirection>(AppSettings.CloudSync.SyncDirection, SyncDirection.DeviceToCloud);

            var completedEventArgs = new BookmarkserviceImportCompletedEventArgs();

            //if the internal id of the uploaded file is unknown, then check to see if it already exists
            if (string.IsNullOrEmpty(syncInternalFilename))
            {
                LiveConnectClient client = new LiveConnectClient(session);
                client.GetCompleted += delegate(object sender, LiveOperationCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        List<object> data = (List<object>)e.Result["data"];
                        object oSyncFile = data.Where(x => {
                            return syncFilename.Equals((x as IDictionary<string, object>)["name"]);
                        }).FirstOrDefault();

                        if (oSyncFile != null)
                        {
                            syncInternalFilename = (oSyncFile as IDictionary<string, object>)["id"].ToString();
                            AppSettings.AddOrUpdateValue(AppSettings.CloudSync.SkyDriveInternalSyncFilename, syncInternalFilename);
                        }

                        CoordinateSync(session, syncFilename, syncInternalFilename, completedAction, completedEventArgs);
                    }
                };
                client.GetAsync("/me/skydrive/files");
            }
            else
            {
                CoordinateSync(session, syncFilename, syncInternalFilename, completedAction, completedEventArgs);
            }
        }

        private void CoordinateSync(LiveConnectSession session, string syncFilename, string syncInternalFilename, Action<BookmarkserviceImportCompletedEventArgs> completedAction, BookmarkserviceImportCompletedEventArgs completedEventArgs)
        {
            //if device is sync priority, then not necessary to retrieve existing bookmarks
            //this would act as backup
            SyncDirection syncDirection = AppSettings.GetValueOrDefault<SyncDirection>(AppSettings.CloudSync.SyncDirection, SyncDirection.DeviceToCloud);

            if (syncDirection == SyncDirection.DeviceToCloud)
                PerformDeviceToCloudSync(session, syncFilename, completedAction, completedEventArgs);
            else
                PerformCloudToDeviceSync(session, syncFilename, syncInternalFilename, completedAction, completedEventArgs);

        }

        private void PerformCloudToDeviceSync(LiveConnectSession session, string syncFilename, string syncInternalFilename, Action<BookmarkserviceImportCompletedEventArgs> completedAction, BookmarkserviceImportCompletedEventArgs completedEventArgs)
        {
            //get cloud bookmarks
            LiveConnectClient downloadClient = new LiveConnectClient(session);
            downloadClient.DownloadCompleted += delegate(object sender, LiveDownloadCompletedEventArgs d)
            {
                HashSet<Bookmark> externalBookmarks = new HashSet<Bookmark>();
                
                if (d.Error == null)
                {
                    string fileContents;
                    //parse downloaded file
                    using (StreamReader reader = new StreamReader(d.Result))
                        fileContents = reader.ReadToEnd();

                    ParseHtml(fileContents, true, externalBookmarks, completedEventArgs);
                }

                List<Bookmark> deviceBookmarks = RetrieveDeviceBookmarks();
                IEnumerable<Bookmark> deviceBookmarksRemoved = externalBookmarks.Except(externalBookmarks, new UrlComparer<Bookmark>());
            
                //remove items that have been removed since
                foreach (Bookmark bookmark in deviceBookmarksRemoved)
                    DeleteDeviceBookmark(bookmark.Title);
                
                //add the newly discovered bookmarks to the device
                foreach (Bookmark bookmark in externalBookmarks)
                    CreateOrUpdateDeviceBookmark(bookmark.Title, bookmark);

                completedEventArgs.StatusCode = ImportStatusCode.Success;
                completedAction(completedEventArgs);
            };

            downloadClient.DownloadAsync(string.Concat(syncInternalFilename, "/content"));
        }

        private void PerformDeviceToCloudSync(LiveConnectSession session, string syncFilename, Action<BookmarkserviceImportCompletedEventArgs> completedAction, BookmarkserviceImportCompletedEventArgs completedEventArgs)
        {
            //get device bookmarks
            List<Bookmark> deviceBookmarks = RetrieveDeviceBookmarks();
        
            //upload
            string syncFileContents = GenerateNetscapBookmarkFile(syncFilename, deviceBookmarks);

            LiveConnectClient uploadClient = new LiveConnectClient(session);
            uploadClient.UploadCompleted += delegate(object uploadSender, LiveOperationCompletedEventArgs e)
            {
                #warning update the internal file name if non existant
                completedEventArgs.StatusCode = ImportStatusCode.Success;
                completedAction(completedEventArgs);
            };

            Stream uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(syncFileContents));
            uploadClient.UploadAsync("me/skydrive", syncFilename, true, uploadStream, null);
        }

        private void ParseHtml(string unParsedHtml, bool skipOnError, HashSet<Bookmark> externalBookmarks, BookmarkserviceImportCompletedEventArgs completedEventArgs)
        {
            try
            {
                MatchCollection matches = Regex.Matches(unParsedHtml, "href=\"(?<link>[^\"]*)\".*>(?<title>.*)</a>", RegexOptions.IgnoreCase);

                completedEventArgs.PotentialCount = matches.Count;

                foreach (Match m in matches)
                {
                    Bookmark bookmark = new Bookmark();

                    if (m.Groups["title"] != null)
                        bookmark.Title = FileService.ConvertToValidFileName(m.Groups["title"].Value);

                    if (m.Groups["link"] != null)
                        bookmark.Url = m.Groups["link"].Value;

                    if (!string.IsNullOrEmpty(bookmark.Title) && !string.IsNullOrEmpty(bookmark.Url))
                    {
                        externalBookmarks.Add(bookmark);
                    }
                    else
                    {
                        if (skipOnError)
                        {
                            completedEventArgs.SkipCount++;
                            continue;
                        }
                        else
                        {
                            completedEventArgs.StatusCode = ImportStatusCode.FailParsingError;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                completedEventArgs.Exception = ex;
                completedEventArgs.StatusCode = ImportStatusCode.FailParsingError;
            }
        }
    }
}
