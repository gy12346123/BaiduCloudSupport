using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BaiduCloudSupport.Window
{
    /// <summary>
    /// NewDownloadTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewDownloadTaskWindow : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private long _Space;

        public long Space
        {
            get { return _Space; }
            set
            {
                if (_Space != value)
                {
                    _Space = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Space"));
                    }
                }
            }
        }

        private string _DownloadURL;

        public string DownloadURL
        {
            get { return _DownloadURL; }
            set
            {
                if (_DownloadURL != value)
                {
                    _DownloadURL = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DownloadURL"));
                    }
                }
            }
        }



        public NewDownloadTaskWindow(string url)
        {
            InitializeComponent();
            DownloadURL = url;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_LoadDownloadPath.DataContext = MainWindow.totalData;
            textBlock_Space.DataContext = this;
            textBox_URL.DataContext = this;
            if (Setting.DownloadPath != null && !Setting.DownloadPath.Equals(""))
            {
                MainWindow.totalData.DownloadDefaultFolderPath = Setting.DownloadPath;
                DirectoryInfo info = new DirectoryInfo(Setting.DownloadPath);
                Space = Other.Tools.GetHardDiskFreeSpace(info.Root.FullName);
            }

        }

        private void button_LoadDownloadPath_Click(object sender, RoutedEventArgs e)
        {
            string path = Other.Tools.GetFolder(GlobalLanguage.FindText("MainWindow_LoadDownloadPath_Title"), true);
            if (path != string.Empty)
            {
                MainWindow.totalData.DownloadDefaultFolderPath = path;
            }
        }

        private void button_Download_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_LoadDownloadPath.Text == null || textBox_LoadDownloadPath.Text.Equals(""))
            {
                textBox_LoadDownloadPath.Background = Brushes.Red;
                return;
            }
            DialogResult = true;
        }
    }
}
