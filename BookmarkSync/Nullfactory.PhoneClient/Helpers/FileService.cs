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
    public class FileService
    {
        public static string ConvertToValidFileName(string fileName)
        {
            //The following chars are identical to the ones produced by System.IO.Path.GetInvalidFileNameChars
            int[] invalidFileNameChars = new int[] { 34,60,62,124,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,
                                     16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,58,42,63,92,47 };

            string validFilename = fileName;

            foreach (int i in invalidFileNameChars)
            {
                //perform correction if necessary
                char c = (char)i;
                validFilename = validFilename.Replace(c, '-');
            }

            char[] invalidPathCharacters = System.IO.Path.GetInvalidPathChars();
            foreach (char c in invalidPathCharacters)
                validFilename = validFilename.Replace(c, '-');

            return validFilename;
        }
    }
}
