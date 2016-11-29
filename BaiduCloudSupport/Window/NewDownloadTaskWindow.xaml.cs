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

        public NewDownloadTaskWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_LoadDownloadPath.DataContext = MainWindow.totalData;
            textBlock_Space.DataContext = this;
            MainWindow.totalData.DownloadDefaultFolderPath = Setting.DownloadPath;
            DirectoryInfo info = new DirectoryInfo(Setting.DownloadPath);
            Space = Other.Tools.GetHardDiskFreeSpace(info.Root.FullName);
        }
    }
}
