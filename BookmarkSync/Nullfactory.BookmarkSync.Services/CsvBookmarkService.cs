using System;
using System.Collections.Generic;
using Nullfactory.PhoneClient.Helpers;
using System.Net;

namespace Nullfactory.BookmarkSync.Services
{
    public class CsvBookmarkService : BookmarkServiceBase
    {
        public override void ImportExternalBookmarks(Uri fileLocation, bool skipOnError, Action<List<Bookmark>, BookmarkserviceImportCompletedEventArgs> completedAction)
        {
            List<Bookmark> externalBookmarks = new List<Bookmark>();
            var completedEventArgs = new BookmarkserviceImportCompletedEventArgs() { StatusCode = ImportStatusCode.Success };

            WebClient client = new WebClient();
            client.DownloadStringCompleted += delegate(object o, DownloadStringCompletedEventArgs args)
            {
                //Critical Failure
                if (args.Error != null)
                {
                    completedEventArgs.Exception = args.Error;
                    completedEventArgs.StatusCode = ImportStatusCode.FailAccessError;
                    completedAction(externalBookmarks, completedEventArgs);
                    return;
                }

                string unparsedCsv = args.Result;
                string[] unParsedRows = unparsedCsv.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                bool titleRow = true;

                try
                {
                    //-1 since we're skipping the title
                    completedEventArgs.PotentialCount = unParsedRows.Length - 1;

                    foreach (string line in unParsedRows)
                    {
                        //Assumed that the first row is the title
                        if (titleRow) { titleRow = false; continue; }

                        string[] row = line.Split(',');
                        if (row.Length < 2)
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

                        string title = FileService.ConvertToValidFileName(row[0]);
                        string url = CleanCsvValue(row[1]);

                        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(url))
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
                        
                        externalBookmarks.Add(new Bookmark() { Title = title, Url = url });
                        completedEventArgs.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    completedEventArgs.Exception = ex;
                    completedEventArgs.StatusCode = ImportStatusCode.FailParsingError;
                }

                completedAction(externalBookmarks, completedEventArgs);
            };

            client.DownloadStringAsync(fileLocation);
        }

        protected string CleanCsvValue(string value)
        {
            string cleanedCsv = string.Empty;

            cleanedCsv = value.Replace("\'",string.Empty); 
            cleanedCsv = cleanedCsv.Replace("\"", string.Empty);

            return cleanedCsv;
        }
    }
}
