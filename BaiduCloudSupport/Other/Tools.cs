using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.Other
{
    public static class Tools
    {
        /// <summary>
        /// Return TimeStamp as string
        /// </summary>
        /// <returns>TimeStamp</returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Return TimeStamp as long use DateTime
        /// </summary>
        /// <param name="dateTime">DataTime</param>
        /// <returns>TimeStamp</returns>
        public static long GetTimeStamp(DateTime dateTime)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            return Convert.ToInt64((dateTime - start).TotalSeconds);
        }

        /// <summary>
        /// Convert TimeStamp ot DateTime
        /// </summary>
        /// <param name="target">DateTime</param>
        /// <param name="timestamp">TimeStamp</param>
        /// <returns>DateTime</returns>
        public static DateTime TimeStamp2DateTime(this DateTime target, long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            return start.AddSeconds(timestamp);
        }

        public static DateTime TimeStamp2DateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        /// <summary>
        /// Get Path and File name which user selectd.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">Example:Output.log|*.log|Output.txt|*.txt</param>
        /// <param name="initDir">Default path</param>
        /// <param name="multiSelect">Multi file</param>
        /// <param name="checkPath">Check path if not exist will show the error message</param>
        /// <param name="checkFile">Check file if not exist will show the error message</param>
        /// <returns>File name with full path,if select one file,return string[0]</returns>
        public static string[] GetPath(string title, string filter, string initDir, bool multiSelect = false, bool checkPath = false, bool checkFile = false)
        {
            System.Windows.Forms.OpenFileDialog OFD = new System.Windows.Forms.OpenFileDialog();
            OFD.Title = title;
            OFD.Filter = filter;
            OFD.InitialDirectory = initDir;
            OFD.Multiselect = multiSelect;
            OFD.CheckPathExists = checkPath;
            OFD.CheckFileExists = checkFile;
            if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (multiSelect)
                {
                    return OFD.FileNames;
                }
                else
                {
                    return new string[1] { OFD.FileName };
                }

            }
            else
            {
                return new string[1] { String.Empty };
            }
        }

        /// <summary>
        /// Get folder which user selected or created
        /// </summary>
        /// <param name="description">Dialog title</param>
        /// <param name="showNewFolderButton">Show create folder button</param>
        /// <param name="rootFolder">Start up path</param>
        /// <returns>SelectedPath</returns>
        public static string GetFolder(string description, bool showNewFolderButton, Environment.SpecialFolder rootFolder = Environment.SpecialFolder.Desktop)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = description;
            dialog.ShowNewFolderButton = showNewFolderButton;
            dialog.RootFolder = rootFolder;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return string.Empty;
        }

        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024 * 1024);
                }
            }
            return freeSpace;
        }
    }
}
