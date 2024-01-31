using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.Base
{
    public class CommonFunc
    {
        private static string _executionFolderPath = null;

        public static string AppFolderPath
        {
            get
            {
                if (_executionFolderPath is null)
                {
                    string executionFilePath = Assembly.GetExecutingAssembly().Location;
                    FileInfo fInf = new FileInfo(executionFilePath);
                    _executionFolderPath = fInf.DirectoryName;
                }
                return _executionFolderPath;
            }
        }

        /// <summary>
        /// Get Xaml Resource refer to a given File Path included extended folder. This folder must extended base on Application folder.
        /// </summary>
        /// <param name="extendedFilePath">like resource file "ViewPage/Date/rosDateEnglish.xaml"</param>
        /// <returns></returns>
        public static ResourceDictionary GetXamlResource(string extendedFilePath)
        {
            string urlFilePath = $@"file:///{AppFolderPath}\{extendedFilePath}";
            ResourceDictionary retSource = new ResourceDictionary() { Source = new Uri(urlFilePath, UriKind.Absolute) };
            return retSource;
        }
    }
}