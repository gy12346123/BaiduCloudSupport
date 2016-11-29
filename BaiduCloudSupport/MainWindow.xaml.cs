using BaiduCloudSupport.API;
using BaiduCloudSupport.Other;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaiduCloudSupport
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// All UI binding and necessary data
        /// </summary>
        public static TotalData totalData;

        /// <summary>
        /// Theme styles for change window style
        /// </summary>
        public enum ThemeStyle { BaseLight, BaseDark }

        /// <summary>
        /// Theme accents for change window color
        /// </summary>
        public enum ThemeAccent { Red, Green, Blue, Purple, Orange, Lime, Emerald, Teal, Cyan, Cobalt, Indigo, Violet, Pink, Magenta, Crimson, Amber, Yellow, Brown, Olive, Steel, Mauve, Taupe, Sienna }

        private BitmapImage Icon_NormalFolder = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/NormalFolder.png"));

        private BitmapImage Icon_BT = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/BT.png"));

        private BitmapImage Icon_Exe = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Exe.png"));

        private BitmapImage Icon_Image = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Image.png"));

        private BitmapImage Icon_Iso = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Iso.png"));

        private BitmapImage Icon_Music = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Music.png"));

        private BitmapImage Icon_Txt = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Txt.png"));

        private BitmapImage Icon_Unknown = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Unknown.png"));

        private BitmapImage Icon_Video = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Video.png"));

        private BitmapImage Icon_Zip = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/Zip.png"));

        private static object _DownloadListChangeLock = new object();

        public MainWindow()
        {
            // Set default Language
            GlobalLanguage.SetLanguage(Setting.MainLanguage);
            InitializeComponent();
            totalData = new TotalData();
        }

        private System.Windows.Input.ICommand openFirstFlyoutCommand;

        public System.Windows.Input.ICommand OpenFirstFlyoutCommand
        {
            get
            {
                return this.openFirstFlyoutCommand ?? (this.openFirstFlyoutCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
                    ExecuteDelegate = x => this.ToggleFlyout(0)
                });
            }
        }
        /// <summary>
        /// Show the flyout
        /// </summary>
        /// <param name="index">the number of Flyout Item</param>
        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        private async void button_Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login.LoginWindow LW = new Login.LoginWindow();
                LW.Owner = this;
                LW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if ((bool)LW.ShowDialog())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginSucceed"));
                    totalData.ProgressRing_IsActive = true;
                    var result = await ReloadSimpleUserInfo();
                    if (!result)
                    {
                        await Task.Factory.StartNew(() => {
                            LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                        });
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_AutoLoginFailed"));
                    }
                    var floderResult = await LoadFloder("");
                    if (floderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = floderResult;
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginFailed"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_Login_Click", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private void button_About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow AB = new AboutWindow();
            AB.Owner = this;
            AB.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            AB.ShowDialog();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            grid_Main.DataContext = totalData;
            flyoutsControl.DataContext = totalData;

            try
            {
                if (Setting.Baidu_Access_Token != null && !Setting.Baidu_Access_Token.Equals(""))
                {
                    totalData.ProgressRing_IsActive = true;
                    if (Setting.Baidu_uname.Equals("") || Setting.UserPortraitFilePath.Equals(""))
                    {
                        var result = await ReloadSimpleUserInfo();
                        if (!result)
                        {
                            await Task.Factory.StartNew(() => {
                                LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                            });
                            await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_AutoLoginFailed"));
                        }
                    }
                    else
                    {
                        totalData.uname = Setting.Baidu_uname;
                        if (!Setting.Baidu_Quota_Total.Equals(""))
                        {
                            totalData.Quota_Total = Convert.ToUInt64(Setting.Baidu_Quota_Total);
                        }
                        if (!Setting.Baidu_Quota_Used.Equals(""))
                        {
                            totalData.Quota_Used = Convert.ToUInt64(Setting.Baidu_Quota_Used);
                        }
                        await Task.Factory.StartNew(() => {
                            LoadUserPortraitFromFile(Setting.UserPortraitFilePath);
                        });
                    }
                    var floderResult = await LoadFloder("");
                    if (floderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = floderResult;
                }
                else
                {
                    await Task.Factory.StartNew(() => {
                        LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                    });
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MetroWindow_Loaded", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        public Task<bool> ReloadSimpleUserInfo()
        {
            return Task<bool>.Factory.StartNew(() => {
                try
                {
                    ulong[] quota = PCS.Quota();
                    if (quota != null && quota.Count() == 2)
                    {
                        totalData.Quota_Total = quota[0];
                        totalData.Quota_Used = quota[1];
                    }
                    else
                    {
                        return false;
                    }
                    SimpleUserInfoStruct userInfo = PCS.SimpleUser();
                    if (userInfo.uid != 0)
                    {
                        totalData.uid = userInfo.uid;
                        totalData.uname = userInfo.uname;
                        if (!Setting.Baidu_portrait.Equals(userInfo.portrait))
                        {
                            string imagePath = Setting.BasePath + @"Images\UserInfo\";
                            string imageFile = imagePath + userInfo.uid + ".jpg";
                            if (!Directory.Exists(imagePath))
                            {
                                Directory.CreateDirectory(imagePath);
                            }
                            WebClient web = new WebClient();
                            web.DownloadFile(new Uri(PCS.UserSmallPortrait(userInfo.portrait)), imageFile);
                            totalData.portrait = userInfo.portrait;
                            Setting.WriteAppSetting("UserPortraitFilePath", imageFile, true);
                            LoadUserPortraitFromFile(imageFile);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("MainWindow.ReloadSimpleUserInfo", ex);
                    return false;
                }
            });
        }

        public Task<List<FileListDataItem>> LoadFloder(string path)
        {
            return Task.Factory.StartNew(()=> {
                FileListStruct[] fileListStruct = PCS.SingleFloder(path);
                if (fileListStruct == null)
                {
                    return null;
                }
                //List<FileListDataItem> fileList = new List<FileListDataItem>();
                //foreach(FileListStruct FS in fileListStruct)
                //{
                //    string[] files = FS.path.Split('/');
                //    string convertedSize = "-";
                //    if (FS.isdir == 0)
                //    {
                //        convertedSize = ConvertFileSize(FS.size);
                //    }

                //    fileList.Add(new FileListDataItem() {
                //        fs_id = FS.fs_id,
                //        path = FS.path,
                //        file = files[files.Count() - 1],
                //        mtime = Tools.TimeStamp2DateTime(FS.mtime.ToString()),
                //        md5 = FS.md5,
                //        size = convertedSize,
                //        isdir = FS.isdir,
                //        isSelected = false,
                //        Icon = GetFileIcon(files[files.Count() - 1], Convert.ToInt32(FS.isdir))
                //    });
                //}

                ConcurrentBag<FileListDataItem> bag = new ConcurrentBag<FileListDataItem>();
                Parallel.ForEach(fileListStruct, (FS)=> {
                    string[] files = FS.path.Split('/');
                    string convertedSize = "-";
                    if (FS.isdir == 0)
                    {
                        convertedSize = ConvertFileSize(FS.size);
                    }

                    bag.Add(new FileListDataItem()
                    {
                        fs_id = FS.fs_id,
                        path = FS.path,
                        file = files[files.Count() - 1],
                        mtime = Tools.TimeStamp2DateTime(FS.mtime.ToString()),
                        md5 = FS.md5,
                        size = convertedSize,
                        isdir = FS.isdir,
                        isSelected = false,
                        Icon = GetFileIcon(files[files.Count() - 1], Convert.ToInt32(FS.isdir))
                    });
                });
                var q = bag.OrderByDescending(x => x.isdir);
                return q.ToList();
            });
        }

        private BitmapImage GetFileIcon(string fileName, int isdir)
        {
            if (isdir == 0)
            {
                // File
                string[] extension = fileName.Split('.');
                if (extension.Count() > 1)
                {
                    switch (extension[extension.Count() - 1])
                    {
                        case "torrent":
                            return Icon_BT;
                        case "exe":
                            return Icon_Exe;
                        case "jpg":
                        case "png":
                        case "bmp":
                        case "gif":
                        case "tiff":
                        case "exif":
                        case "svg":
                        case "psd":
                        case "eps":
                            return Icon_Image;
                        case "iso":
                            return Icon_Iso;
                        case "mp3":
                        case "cue":
                        case "flac":
                        case "mac":
                        case "ape":
                        case "m4a":
                        case "aac":
                        case "wav":
                        case "wma":
                            return Icon_Music;
                        case "txt":
                            return Icon_Txt;
                        case "mp4":
                        case "rm":
                        case "rmvb":
                        case "wmv":
                        case "avi":
                        case "3gp":
                        case "mkv":
                        case "flv":
                            return Icon_Video;
                        case "zip":
                        case "7z":
                        case "rar":
                            return Icon_Zip;
                        default:
                            return Icon_Unknown;
                    }
                }
                else
                {
                    return Icon_Unknown;
                }
            }
            else
            {
                // Folder
                return Icon_NormalFolder;
            }
        }

        private string ConvertFileSize(ulong size)
        {
            if (size < 1024ul)
            {
                return string.Format("{0}B", size);
            }else if (size >= 1024ul && size < 1024ul * 1024ul)
            {
                return string.Format("{0}K", Math.Round(Convert.ToDouble(size / 1024ul), 1));
            }else if (size >= 1024ul * 1024ul && size < 1024ul * 1024ul * 1024ul)
            {
                return string.Format("{0}M", Math.Round(Convert.ToDouble(size / 1024ul / 1024ul), 1));
            }else if (size >= 1024ul * 1024ul * 1024ul && size < 1024ul * 1024ul * 1024ul * 1024ul)
            {
                return string.Format("{0}G", Math.Round(Convert.ToDouble(size / 1024ul / 1024ul / 1024ul), 2));
            }else
            {
                return string.Format("{0}T", Math.Round(Convert.ToDouble(size / 1024ul / 1024ul / 1024ul / 1024ul), 3));
            }
        }

        private void LoadUserPortraitFromFile(string filePath)
        {
            try
            {
                System.Drawing.Bitmap image = System.Drawing.Bitmap.FromFile(filePath) as System.Drawing.Bitmap;
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromBitmap(ref image);
                totalData.UserPortrait = bitmap;
                image.Dispose();
                image = null;
                bitmap = null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.LoadUserPortraitFromFile", ex);
            }
        }

        /// <summary>
        /// Change app accent
        /// </summary>
        /// <param name="accent">New accent</param>
        /// <param name="theme">Theme</param>
        public void ChangeAppStyle(ThemeAccent accent, MahApps.Metro.AppTheme theme)
        {
            // set the accent and theme to the window
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current,
                                        MahApps.Metro.ThemeManager.GetAccent(accent.ToString()),
                                        MahApps.Metro.ThemeManager.GetAppTheme(theme.Name));
        }

        /// <summary>
        /// Change app theme
        /// </summary>
        /// <param name="accent">Accent</param>
        /// <param name="style">New theme</param>
        public void ChangeAppStyle(MahApps.Metro.Accent accent, ThemeStyle style)
        {
            // set the accent and theme to the window
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current,
                                        MahApps.Metro.ThemeManager.GetAccent(accent.Name),
                                        MahApps.Metro.ThemeManager.GetAppTheme(style.ToString()));
        }

        private async void splitButton_Setting_Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (splitButton_Setting_Language.SelectedItem != null && !splitButton_Setting_Language.SelectedItem.ToString().Equals(Setting.MainLanguage))
            {
                if (Setting.WriteAppSetting("MainLanguage", splitButton_Setting_Language.SelectedItem.ToString()))
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Done"), GlobalLanguage.FindText("Message_Done_LanguageChanged"));
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("Message_Fail_SaceChange"));
                }
            }
        }

        private void button_Setting_Click(object sender, RoutedEventArgs e)
        {
            // Accent color init
            button_Setting_Color_Blue.Background = new SolidColorBrush(Color.FromRgb(112, 197, 233));
            button_Setting_Color_Purple.Background = new SolidColorBrush(Color.FromRgb(131, 122, 229));
            button_Setting_Color_Green.Background = new SolidColorBrush(Color.FromRgb(128, 186, 69));
            button_Setting_Color_Pink.Background = new SolidColorBrush(Color.FromRgb(246, 142, 217));
            button_Setting_Color_Teal.Background = new SolidColorBrush(Color.FromRgb(51, 188, 186));
            button_Setting_Color_Orange.Background = new SolidColorBrush(Color.FromRgb(251, 134, 51));
            button_Setting_Color_Cobalt.Background = new SolidColorBrush(Color.FromRgb(51, 115, 242));
            button_Setting_Color_Red.Background = new SolidColorBrush(Color.FromRgb(234, 67, 51));
            button_Setting_Color_Crimson.Background = new SolidColorBrush(Color.FromRgb(181, 51, 81));
            button_Setting_Color_Mauve.Background = new SolidColorBrush(Color.FromRgb(145, 128, 161));
            button_Setting_Color_Olive.Background = new SolidColorBrush(Color.FromRgb(138, 159, 131));
            button_Setting_Color_Steel.Background = new SolidColorBrush(Color.FromRgb(131, 145, 159));

            // Language list init
            List<GlobalLanguage.LanguageList> langList = new List<GlobalLanguage.LanguageList>();
            langList.Add(GlobalLanguage.LanguageList.en);
            langList.Add(GlobalLanguage.LanguageList.zh);
            splitButton_Setting_Language.ItemsSource = langList;
            totalData.DownloadDefaultFolderPath = Setting.DownloadPath;
            // read setting and show default language selected.
            switch (Setting.MainLanguage)
            {
                case "en":
                    splitButton_Setting_Language.SelectedItem = GlobalLanguage.LanguageList.en;
                    break;
                case "zh":
                    splitButton_Setting_Language.SelectedItem = GlobalLanguage.LanguageList.zh;
                    break;
                default:
                    splitButton_Setting_Language.SelectedItem = GlobalLanguage.LanguageList.en;
                    break;
            }

            ToggleFlyout(0);
        }

        private void button_Setting_Theme_Light_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(appStyle.Item2, ThemeStyle.BaseLight);
        }

        private void button_Setting_Theme_Dark_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(appStyle.Item2, ThemeStyle.BaseDark);
        }

        private void button_Setting_Color_Blue_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Blue, appStyle.Item1);
        }

        private void button_Setting_Color_Purple_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Purple, appStyle.Item1);
        }

        private void button_Setting_Color_Green_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Green, appStyle.Item1);
        }

        private void button_Setting_Color_Pink_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Pink, appStyle.Item1);
        }

        private void button_Setting_Color_Teal_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Teal, appStyle.Item1);
        }

        private void button_Setting_Color_Orange_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Orange, appStyle.Item1);
        }

        private void button_Setting_Color_Cobalt_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Cobalt, appStyle.Item1);
        }

        private void button_Setting_Color_Red_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Red, appStyle.Item1);
        }

        private void button_Setting_Color_Crimson_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Crimson, appStyle.Item1);
        }

        private void button_Setting_Color_Mauve_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Mauve, appStyle.Item1);
        }

        private void button_Setting_Color_Olive_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Olive, appStyle.Item1);
        }

        private void button_Setting_Color_Steel_Click(object sender, RoutedEventArgs e)
        {
            Tuple<MahApps.Metro.AppTheme, MahApps.Metro.Accent> appStyle = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            ChangeAppStyle(ThemeAccent.Steel, appStyle.Item1);
        }

        private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
            if (item != null)
            {
                if (item.isdir == 0)
                {
                    // File
                    DownloadFile(item);
                }else
                {
                    // Floder
                    TransferPage(item.path);
                }
            }else
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
            }
        }

        private void ChangeNavigation(string path)
        {
            wrapPanel_Navigation.Children.Clear();
            if (path.Replace(PCS.BasePath, "").Equals("/"))
            {
                return;
            }
            string[] eachFolder = path.Replace(PCS.BasePath,"").Split('/');
            string basePath = PCS.BasePath;
            if (eachFolder == null)
            {
                return;
            }
            foreach (string folder in eachFolder)
            {
                basePath += basePath.EndsWith("/") ? folder : string.Format("/{0}", folder);
                Button button = new Button()
                {
                    Content = folder == "" ? "root> " : folder + "> ",
                    ToolTip = basePath,
                    Foreground = Brushes.Blue,
                    FontSize = 15d,
                };
                button.Click += Button_Navigation_Template_Click;
                wrapPanel_Navigation.Children.Add(button);
            }
        }

        private void Button_Navigation_Template_Click(object sender, RoutedEventArgs e)
        {
            TransferPage(((Button)sender).ToolTip.ToString());
        }

        public async void DownloadFile(FileListDataItem item)
        {
            try
            {
                if (item.size.Contains("G") || item.size.Contains("T"))
                {
                    // File too big
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_FileTooBig"));
                    string url = await PCS.DownloadURL(Setting.Baidu_Access_Token, item.path);
                    Clipboard.SetText(url);
                }
                else
                {
                    // Download
                    DownloadListDataItem dataItem = new DownloadListDataItem
                    {
                        fs_id = item.fs_id,
                        file = item.file,
                        size = 0L,
                        received = 0L,
                        percentage = 0d,
                        rate = 0d,
                        startTime = DateTime.Now,
                        isSelected = false
                    };

                    if (totalData.DownloadListDataItems != null && totalData.DownloadListDataItems.Count() != 0)
                    {
                        DownloadListAddItem(totalData.DownloadListDataItems, dataItem);
                    }
                    else
                    {
                        List<DownloadListDataItem> list = new List<DownloadListDataItem>();
                        list.Add(dataItem);
                        totalData.DownloadListDataItems = list;
                    }
                    CheckDownloadFolder();
                    //PCS.DownloadFile(Setting.Baidu_Access_Token, item.fs_id, item.path, Setting.DownloadPath + item.file);
                    PCS.DownloadFileSegment(Setting.Baidu_Access_Token, item.fs_id, item.path, Setting.DownloadPath + item.file);
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.DownloadFile", ex);
            }
        }

        public static void CheckDownloadFolder()
        {
            if (Setting.DownloadPath == null || Setting.DownloadPath.Equals(""))
            {
                string defaultFolder = Setting.BasePath + @"Download\";
                if (!Directory.Exists(defaultFolder))
                {
                    Directory.CreateDirectory(defaultFolder);
                }
                totalData.DownloadDefaultFolderPath = defaultFolder;
            }else
            {
                if (!Setting.DownloadPath.EndsWith(@"\"))
                {
                    totalData.DownloadDefaultFolderPath = string.Format("{0}\\", Setting.DownloadPath);
                }
            }
        }

        private async void TransferPage(string path)
        {
            try
            {
                totalData.ProgressRing_IsActive = true;
                var floderResult = await LoadFloder(path);
                if (floderResult == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                }
                totalData.FileListDataItems = floderResult;
                ChangeNavigation(path);
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.TransferPage", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        public static void DownloadListAddItem(List<DownloadListDataItem> list, DownloadListDataItem dataItem)
        {
            List<DownloadListDataItem> newList = new List<DownloadListDataItem>();
            foreach (DownloadListDataItem file in list)
            {
                newList.Add(new DownloadListDataItem {
                    fs_id = file.fs_id,
                    file = file.file,
                    size = file.size,
                    received = file.received,
                    percentage = file.percentage,
                    rate = file.rate,
                    startTime = file.startTime,
                    isSelected = file.isSelected
                });
            }
            newList.Add(dataItem);
            totalData.DownloadListDataItems = newList;
        }

        public static void DownloadListChangeItems(ulong fs_id, long size, long received, double percentage, double rate)
        {
            lock (_DownloadListChangeLock)
            {
                List<DownloadListDataItem> newList = new List<DownloadListDataItem>();
                foreach (DownloadListDataItem file in totalData.DownloadListDataItems)
                {
                    if (file.fs_id == fs_id)
                    {
                        newList.Add(new DownloadListDataItem {
                            fs_id = file.fs_id,
                            file = file.file,
                            size = size,
                            received = received,
                            percentage = percentage,
                            rate = rate,
                            startTime = file.startTime,
                            isSelected = file.isSelected
                        });
                    }else
                    {
                        newList.Add(file);
                    }
                }
                totalData.DownloadListDataItems = newList;
            }
        }

        private void button_LoadDownloadPath_Click(object sender, RoutedEventArgs e)
        {
            string result = Tools.GetFolder(GlobalLanguage.FindText("MainWindow_LoadDownloadPath_Title"), true);
            if (result != string.Empty)
            {
                totalData.DownloadDefaultFolderPath = result;
            }
        }

        private async void MenuItem_GetURL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
                if (item == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                if (item.isdir == 0)
                {
                    totalData.ProgressRing_IsActive = true;
                    string result = await PCS.DownloadURL(Setting.Baidu_Access_Token, item.path);
                    Clipboard.SetText(result);
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Done"), GlobalLanguage.FindText("MainWindow_MenuItem_GetURL_Done"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_GetURL_Click", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
                if (item != null)
                {
                    if (item.isdir == 0)
                    {
                        // File
                        DownloadFile(item);
                    }
                    else
                    {
                        // Folder
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_MenuItem_Download_NotSupportFolderNow"));
                    }
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_GetURL_Click", ex);
            }
        }

        private async void button_GetDownloadURLSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (totalData.FileListDataItems == null)
                {
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                int count = 0;
                //List<string> downloadURLList = new List<string>();
                //foreach (var file in totalData.FileListDataItems)
                //{
                //    if (file.isSelected && file.isdir == 0)
                //    {
                //        string URL = await PCS.DownloadURL(Setting.Baidu_Access_Token, file.path);
                //        downloadURLList.Add(URL);
                //        count++;
                //    }
                //}

                ConcurrentBag<string> bag = new ConcurrentBag<string>();
                await Task.Factory.StartNew(()=> {
                    Parallel.ForEach(totalData.FileListDataItems, new ParallelOptions { MaxDegreeOfParallelism = 3 }, (file) => {
                        if (file.isSelected && file.isdir == 0)
                        {
                            string URL = PCS.DownloadURL(file.path);
                            bag.Add(URL);
                            count++;
                        }
                    });
                });

                if (count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_Button_GetDownloadURLSelected_NoSelect"));
                    return;
                }
                CheckDownloadFolder();
                string filePath = string.Format("{0}DownloadURL_{1}.txt", Setting.DownloadPath, Tools.GetTimeStamp());
                using (StreamWriter SW = new StreamWriter(new FileStream(filePath, FileMode.Create)))
                {
                    foreach (string url in bag)
                    {
                        await SW.WriteLineAsync(url);
                    }
                    await SW.FlushAsync();
                }
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Done"), string.Format(GlobalLanguage.FindText("MainWindow_Button_GetDownloadURLSelected_Done"), filePath));
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_GetDownloadURLSelected_Click", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }

        }

        private void DataGridCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;

            Parallel.ForEach(totalData.FileListDataItems, (file, state) =>
            {
                if (file.fs_id == item.fs_id)
                {
                    file.isSelected = true;
                    state.Stop();
                }
            });
        }

        private void DataGridUncheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;

            Parallel.ForEach(totalData.FileListDataItems, (file, state) => {
                if (file.fs_id == item.fs_id)
                {
                    file.isSelected = false;
                    state.Stop();
                }
            });
        }

        private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dataGrid_FileList.Height = e.NewSize.Height - 200d;
            dataGrid_FileDownloadList.Height = e.NewSize.Height - 200d;
        }

        private void MenuItem_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool isSelected = totalData.FileListDataItems[0].isSelected;
            Parallel.ForEach(totalData.FileListDataItems, (file) =>
            {
                file.isSelected = !isSelected;
            });
        }

        private async void button_ClearAccountSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Setting.WriteAppSetting("Baidu_Access_Token", "", false);
                Setting.WriteAppSetting("Baidu_Expires_In", "", false);
                Setting.WriteAppSetting("Baidu_Session_Secret", "", false);
                Setting.WriteAppSetting("Baidu_Session_Key", "", false);
                Setting.WriteAppSetting("Baidu_Scope", "", false);
                Setting.WriteAppSetting("Baidu_uid", "", false);
                Setting.WriteAppSetting("Baidu_uname", "", false);
                Setting.WriteAppSetting("Baidu_portrait", "", false);
                Setting.WriteAppSetting("UserPortraitFilePath", "", false);
                Setting.WriteAppSetting("Baidu_Quota_Total", "", false);
                Setting.WriteAppSetting("Baidu_Quota_Used", "", true);
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("Message_Done"));
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_ClearAccountSetting_Click", ex);
            }
        }
    }
}
