using BaiduCloudSupport.API;
using BaiduCloudSupport.Other;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MyDownloader.Core;
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

        private System.Windows.Forms.NotifyIcon notifyIcon;

        public static MyDownloader.Extension.PersistedList.PersistedListExtension persistedListExtension;

        public MainWindow()
        {
            // Set default Language
            GlobalLanguage.SetLanguage(Setting.MainLanguage);
            InitializeComponent();
            totalData = new TotalData();
            removeIcon();
            startIcon();
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
                Window.LoginWindow LW = new Window.LoginWindow(Login.BaiduLogin.LoginWeb(Setting.Baidu_client_id, Setting.Baidu_redirect_uri));
                LW.Owner = this;
                LW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if ((bool)LW.ShowDialog())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginSucceed"));
                    //totalData.ProgressRing_IsActive = true;
                    //var result = await ReloadSimpleUserInfo();
                    //if (!result)
                    //{
                    //    await Task.Factory.StartNew(() => {
                    //        LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                    //    });
                    //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_AutoLoginFailed"));
                    //}

                    //var floderResult = await LoadFolder("");
                    //if (floderResult == null)
                    //{
                    //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    //}
                    //totalData.FileListDataItems = floderResult;
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

        private async void button_AdvanceLogin_Click(object sender, RoutedEventArgs e)
        {
            FileInfo file = new FileInfo(Setting.Baidu_CookiePath);
            if (file.Exists)
            {
                // Cookie existed
                var result = await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_Button_AdvanceLogin_CookieExisted"), MessageDialogStyle.AffirmativeAndNegative);
                if (result == MessageDialogResult.Negative)
                {
                    return;
                }
            }
            Window.LoginWindow LW = new Window.LoginWindow("http://yun.baidu.com");
            LW.Owner = this;
            LW.Width = 800d;
            LW.Height = 600d;
            LW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //LW.CookieLocalPath = Setting.BasePath + @"Cookie\BaiduCloud.txt";
            if ((bool)LW.ShowDialog())
            {
                List<string> cookieList = LW.CookieList;
                if (cookieList != null && cookieList.Count() > 0)
                {
                    // Save cookies
                    await Window.LoginWindow.SaveCookies(cookieList, file);
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_Button_AdvanceLogin_CookieSaveSucceed"));
                    LoadMainWindowContent();
                    return;
                }
            }
            await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginFailed"));
        }

        private void button_About_Click(object sender, RoutedEventArgs e)
        {
            Window.AboutWindow AB = new Window.AboutWindow();
            AB.Owner = this;
            AB.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            AB.ShowDialog();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            grid_Main.DataContext = totalData;
            flyoutsControl.DataContext = totalData;
            LoadMainWindowContent();
            PCS.DownloadManagerEvent();
            persistedListExtension = new MyDownloader.Extension.PersistedList.PersistedListExtension();
            InitDownloadList();
        }

        private async void LoadMainWindowContent()
        {
            try
            {
                totalData.ProgressRing_IsActive = true;
                if (BDC.IsCookieFileExist(Setting.Baidu_CookiePath))
                {
                    await BDC.LoadCookieAsync(Setting.Baidu_CookiePath);
                    //Setting.APIMode = Setting.APIMODE.BDC;
                    await ReloadSimpleUserInfo();
                    var folderResult = await LoadFolder("");
                    if (folderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = folderResult;
                }


                //if (Setting.Baidu_Access_Token != null && !Setting.Baidu_Access_Token.Equals(""))
                //{
                //    totalData.ProgressRing_IsActive = true;
                //    if (Setting.Baidu_uname.Equals("") || Setting.UserPortraitFilePath.Equals(""))
                //    {
                //        var result = await ReloadSimpleUserInfo();
                //        if (!result)
                //        {
                //            await Task.Factory.StartNew(() => {
                //                LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                //            });
                //            await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_AutoLoginFailed"));
                //        }
                //    }
                //    else
                //    {
                //        totalData.uname = Setting.Baidu_uname;
                //        if (!Setting.Baidu_Quota_Total.Equals(""))
                //        {
                //            totalData.Quota_Total = Convert.ToUInt64(Setting.Baidu_Quota_Total);
                //        }
                //        if (!Setting.Baidu_Quota_Used.Equals(""))
                //        {
                //            totalData.Quota_Used = Convert.ToUInt64(Setting.Baidu_Quota_Used);
                //        }
                //        await Task.Factory.StartNew(() => {
                //            LoadUserPortraitFromFile(Setting.UserPortraitFilePath);
                //        });
                //    }

                //    // Check Cookie and set API mode
                //    await Task.Factory.StartNew(()=> {
                //        if (BDC.IsCookieFileExist(Setting.Baidu_CookiePath))
                //        {
                //            BDC.LoadCookie(Setting.Baidu_CookiePath);
                //            Setting.APIMode = Setting.APIMODE.BDC;
                //        }
                //    });

                //    // Load folder
                //    var folderResult = await LoadFolder("");
                //    if (folderResult == null)
                //    {
                //        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                //    }
                //    totalData.FileListDataItems = folderResult;
                //}
                //else
                //{
                //    await Task.Factory.StartNew(() => {
                //        LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                //    });
                //}
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.LoadMainWindowContent", ex);
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            removeIcon();
            persistedListExtension.Dispose();
        }

        private void startIcon()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Text = GlobalLanguage.FindText("MainWindow_Title");
            this.notifyIcon.Icon = new System.Drawing.Icon(@"Images\Icon\16.ico");
            this.notifyIcon.Visible = true;
            //Handle mouse double click event
            this.notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            //Context menu
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem menuItem_Close = new System.Windows.Forms.MenuItem();
            menuItem_Close.Text = GlobalLanguage.FindText("NotifyIcon_Close");
            menuItem_Close.Click += new EventHandler(delegate {
                this.Close();
            });
            System.Windows.Forms.MenuItem menuItem_StartAll = new System.Windows.Forms.MenuItem();
            menuItem_StartAll.Text = GlobalLanguage.FindText("NotifyIcon_StartAll");
            menuItem_StartAll.Click += (object sender, EventArgs e) => {
                foreach (var downloader in DownloadManager.Instance.Downloads)
                {
                    downloader.Start();
                }
            };
            System.Windows.Forms.MenuItem menuItem_StopAll = new System.Windows.Forms.MenuItem();
            menuItem_StopAll.Text = GlobalLanguage.FindText("NotifyIcon_StopAll");
            menuItem_StopAll.Click += (object sender, EventArgs e) => {
                Task.Factory.StartNew(()=> {
                    DownloadManager.Instance.PauseAll();
                });
            };
            menu.MenuItems.Add(menuItem_StartAll);
            menu.MenuItems.Add(menuItem_StopAll);
            menu.MenuItems.Add(menuItem_Close);
            this.notifyIcon.ContextMenu = menu;
        }

        private void removeIcon()
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Visible = false;
                this.notifyIcon.Dispose();
                this.notifyIcon = null;
            }
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                //Show window
                this.Show();
                WindowState = WindowState.Normal;
            }
            else if (WindowState == WindowState.Normal)
            {
                //Hide window
                this.Hide();
                WindowState = WindowState.Minimized;
            }
        }

        public Task<bool> ReloadSimpleUserInfo()
        {
            return Task<bool>.Factory.StartNew(() => {
                try
                {
                    ulong[] quota = BDC.Quota();
                    if (quota != null && quota.Count() == 2)
                    {
                        totalData.Quota_Total = quota[0];
                        totalData.Quota_Used = quota[1];
                    }
                    if (Setting.Baidu_uid != null && !Setting.Baidu_uid.Equals(""))
                    {
                        var useInfo = BDC.SimpleUser(Convert.ToUInt64(Setting.Baidu_uid));
                        totalData.uname = useInfo.uname;
                        totalData.uid = Convert.ToUInt64(useInfo.uid);
                        if (!useInfo.portrait.Equals(Setting.Baidu_portrait))
                        {
                            string imagePath = Setting.BasePath + @"Images\UserInfo\";
                            string imageFile = imagePath + useInfo.uid + ".jpg";
                            if (!Directory.Exists(imagePath))
                            {
                                Directory.CreateDirectory(imagePath);
                            }
                            WebClient web = new WebClient();
                            web.DownloadFile(new Uri(useInfo.portrait), imageFile);
                            totalData.portrait = useInfo.portrait;
                            Setting.WriteAppSetting("UserPortraitFilePath", imageFile, true);
                            LoadUserPortraitFromFile(imageFile);
                            return true;
                        }
                    }


                    //if (Setting.Baidu_Access_Token != null && !Setting.Baidu_Access_Token.Equals(""))
                    //{
                    //    SimpleUserInfoStruct userInfo = PCS.SimpleUser(Setting.Baidu_Access_Token);
                    //    if (userInfo.uid != 0)
                    //    {
                    //        totalData.uid = userInfo.uid;
                    //        totalData.uname = userInfo.uname;
                    //        if (!Setting.Baidu_portrait.Equals(userInfo.portrait))
                    //        {
                    //            string imagePath = Setting.BasePath + @"Images\UserInfo\";
                    //            string imageFile = imagePath + userInfo.uid + ".jpg";
                    //            if (!Directory.Exists(imagePath))
                    //            {
                    //                Directory.CreateDirectory(imagePath);
                    //            }
                    //            WebClient web = new WebClient();
                    //            web.DownloadFile(new Uri(PCS.UserSmallPortrait(userInfo.portrait)), imageFile);
                    //            totalData.portrait = userInfo.portrait;
                    //            Setting.WriteAppSetting("UserPortraitFilePath", imageFile, true);
                    //            LoadUserPortraitFromFile(imageFile);
                    //            return true;
                    //        }
                    //    }
                    //}
                    LoadUserPortraitFromFile(Setting.UserPortraitFilePath == "" ? Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png" : Setting.UserPortraitFilePath);
                    return false;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("MainWindow.ReloadSimpleUserInfo", ex);
                    return false;
                }
            });
        }

        public Task<List<FileListDataItem>> LoadFolder(string path, int page = 1)
        {
            return Task.Factory.StartNew(()=> {
                if (path == null || path.Equals(""))
                {
                    path = "/";
                }
                FileListStruct[] fileListStruct = BDC.SingleFolder(path, page);
                if (!path.Equals(totalData.PageNowLoaded))
                {
                    totalData.PageCounter = 1;
                }
                if (fileListStruct.Count() >= 100)
                {
                    this.Dispatcher.Invoke(() => {
                        if (button_FileList_NextPage.Visibility != Visibility.Visible)
                        {
                            button_FileList_NextPage.Visibility = Visibility.Visible;
                        }
                    });
                }else
                {
                    this.Dispatcher.Invoke(() => {
                        if (button_FileList_NextPage.Visibility == Visibility.Visible)
                        {
                            button_FileList_NextPage.Visibility = Visibility.Hidden;
                        }
                    });
                }
                if (page > 1)
                {
                    this.Dispatcher.Invoke(() => {
                        if (button_FileList_PrePage.Visibility != Visibility.Visible)
                        {
                            button_FileList_PrePage.Visibility = Visibility.Visible;
                        }
                    });
                }else
                {
                    this.Dispatcher.Invoke(() => {
                        if (button_FileList_PrePage.Visibility == Visibility.Visible)
                        {
                            button_FileList_PrePage.Visibility = Visibility.Hidden;
                        }
                    });
                }
                return FileListStruct2FileListDataItem(ref fileListStruct);
            });
        }

        private List<FileListDataItem> FileListStruct2FileListDataItem(ref FileListStruct[] fileListStruct)
        {
            if (fileListStruct == null)
            {
                return null;
            }
            ConcurrentBag<FileListDataItem> bag = new ConcurrentBag<FileListDataItem>();
            Parallel.ForEach(fileListStruct, (FS) => {
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

            //switch(Setting.APIMode)
            //{
            //    case Setting.APIMODE.PCS:
            //        radioButton_PCS.IsChecked = true;
            //        radioButton_BDC.IsChecked = false;
            //        break;
            //    case Setting.APIMODE.BDC:
            //        radioButton_PCS.IsChecked = false;
            //        radioButton_BDC.IsChecked = true;
            //        break;
            //}

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
            if (!BDC.CheckCookie())
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                return;
            }
            FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
            if (item != null)
            {
                if (item.isdir == 0)
                {
                    // File
                    //if (!item.path.StartsWith(PCS.BasePath)) return;
                    //if (!await Setting.CheckApiModeAsync(Setting.APIMODE.PCS))
                    //{
                    //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_ChangeApiMode_FailMessage"));
                    //    return;
                    //}
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
            string basePath;
            string[] eachFolder;
            if (path.Equals("/"))
            {
                return;
            }
            basePath = "/";
            eachFolder = path.Split('/');
            //switch (Setting.APIMode)
            //{
            //    case Setting.APIMODE.PCS:
            //        if (path.Replace(PCS.BasePath, "").Equals("/"))
            //        {
            //            return;
            //        }
            //        basePath = PCS.BasePath;
            //        eachFolder = path.Replace(PCS.BasePath, "").Split('/');
            //        break;
            //    case Setting.APIMODE.BDC:
            //        if (path.Equals("/"))
            //        {
            //            return;
            //        }
            //        basePath = "/";
            //        eachFolder = path.Split('/');
            //        break;
            //    default:
            //        basePath = PCS.BasePath;
            //        eachFolder = path.Replace(PCS.BasePath, "").Split('/');
            //        break;
            //}
            //string basePath = PCS.BasePath;
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

        private async void AddDownloadTask(FileListDataItem item)
        {
            Window.NewDownloadTaskWindow taskWindow = new Window.NewDownloadTaskWindow(await BDC.DownloadURLAsync(item.path));
            taskWindow.Owner = this;
            if ((bool)taskWindow.ShowDialog())
            {
                DownloadListDataItem dataItem = new DownloadListDataItem
                {
                    fs_id = item.fs_id,
                    file = item.file,
                    size = 0L,
                    received = 0L,
                    percentage = 0d,
                    rate = 0d,
                    startTime = DateTime.Now,
                    isSelected = false,
                    state = DownloaderStateChinese.需解析,
                    left = TimeSpan.FromMilliseconds(0d),
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
                PCS.DownloadFileSegment(item.fs_id, taskWindow.DownloadURL, Setting.DownloadPath + item.file, item.path);
            }
            taskWindow = null;
        }

        public async void DownloadFile(FileListDataItem item)
        {
            try
            {
                totalData.ProgressRing_IsActive = true;
                if (item.size.Contains("T"))
                {
                    // File too big
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_FileTooBig"));
                    string url = await BDC.DownloadURLAsync(item.path);
                    Clipboard.SetText(url);
                }
                else
                {
                    // Download
                    AddDownloadTask(item);
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.DownloadFile", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
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
                var floderResult = await LoadFolder(path);
                if (floderResult == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                }
                totalData.FileListDataItems = floderResult;
                totalData.PageNowLoaded = path;
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
                    isSelected = file.isSelected,
                    left = file.left,
                    state = file.state
                });
            }
            if (dataItem != null)
            {
                newList.Add(dataItem);
            }
            totalData.DownloadListDataItems = newList;
        }

        public static void DownloadListChangeItems(ulong fs_id, long size, long received, double percentage, double rate, TimeSpan left, DownloaderState state)
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
                            isSelected = file.isSelected,
                            left = left,
                            state = Other.Tools.ConvertDownloaderState(state)
                        });
                    }else
                    {
                        newList.Add(file);
                    }
                }
                totalData.DownloadListDataItems = newList;
            }
        }

        private void InitDownloadList()
        {
            if (DownloadManager.Instance.Downloads.Count() > 0)
            {
                PCS.Init();
                List<DownloadListDataItem> newList = new List<DownloadListDataItem>();
                foreach (var downloader in DownloadManager.Instance.Downloads)
                {
                    newList.Add(new DownloadListDataItem {
                        fs_id = downloader.fs_id,
                        file = System.IO.Path.GetFileName(downloader.LocalFile),
                        size = downloader.FileSize / 1048576L,
                        received = downloader.Transfered / 1048576L,
                        percentage = Math.Round(downloader.Progress, 1),
                        rate = 0d,
                        startTime = downloader.CreatedDateTime,
                        isSelected = false,
                        left = downloader.Left,
                        state = Other.Tools.ConvertDownloaderState(downloader.State)
                    });
                }
                var list = DownloadManager.Instance.Downloads.ToArray();
                for (int i = 0; i < list.Count(); i++)
                {
                    PCS.DownloaderEvent(ref list[i]);
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
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
                if (item == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                if (item.isdir == 0)
                {
                    totalData.ProgressRing_IsActive = true;
                    //if (!item.path.StartsWith(PCS.BasePath)) return;
                    //if (!await Setting.CheckApiModeAsync(Setting.APIMODE.PCS))
                    //{
                    //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_ChangeApiMode_FailMessage"));
                    //    return;
                    //}
                    string result = await BDC.DownloadURLAsync(item.path);
                    if (await this.ShowMessageAsync(GlobalLanguage.FindText("MainWindow_MenuItem_GetURL_ShortUrl_Title"), GlobalLanguage.FindText("MainWindow_MenuItem_GetURL_ShortUrl_Message"), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                    {
                        Clipboard.SetText(await ShortURL.ShortenAsync(result));
                    }else
                    {
                        Clipboard.SetText(result);
                    }
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
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
                if (item != null)
                {
                    if (item.isdir == 0)
                    {
                        // File
                        //if (!await Setting.CheckApiModeAsync(Setting.APIMODE.PCS))
                        //{
                        //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_ChangeApiMode_FailMessage"));
                        //    return;
                        //}
                        //if (item.path.StartsWith(PCS.BasePath))
                        //{
                        //    DownloadFile(item);
                        //}
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
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                if (totalData.FileListDataItems == null)
                {
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                //if (!await Setting.CheckApiModeAsync(Setting.APIMODE.PCS))
                //{
                //    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_ChangeApiMode_FailMessage"));
                //    return;
                //}
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
                            string URL = BDC.DownloadURL(file.path);
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
            try
            {
                if (totalData.FileListDataItems == null || totalData.FileListDataItems.Count == 0)
                {
                    return;
                }
                bool isSelected = totalData.FileListDataItems[0].isSelected;
                Parallel.ForEach(totalData.FileListDataItems, (file) =>
                {
                    file.isSelected = !isSelected;
                });
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_SelectAll_Click", ex);
            }
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

        private async void button_SearchFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                string keyword = await this.ShowInputAsync(GlobalLanguage.FindText("MainWindow_Button_SearchFile_Title"), GlobalLanguage.FindText("MainWindow_Button_SearchFile_Message"));
                if (keyword == null || keyword.Equals(""))
                {
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                FileListStruct[] fileList = await BDC.Search(keyword);

                var floderResult = FileListStruct2FileListDataItem(ref fileList);
                if (floderResult == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    return;
                }
                if (floderResult.Count() == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Done"), GlobalLanguage.FindText("MainWindow_Button_SearchFile_NoResult"));
                    return;
                }
                totalData.FileListDataItems = floderResult;
                string[] pathParam = fileList[0].path.Split('/');
                ChangeNavigation(fileList[0].path.Replace("/" + pathParam[pathParam.Count() - 1], ""));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_SearchFile_Click", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadListDataItem item = (DownloadListDataItem)dataGrid_FileDownloadList.SelectedItem;
                if (item == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                await Task.Factory.StartNew(() => {
                    foreach (var downloader in DownloadManager.Instance.Downloads)
                    {
                        if (downloader.fs_id == item.fs_id)
                        {
                            downloader.Pause();
                            break;
                        }
                    }
                });
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_Stop_Click", ex);
            }
        }

        private async void MenuItem_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadListDataItem item = (DownloadListDataItem)dataGrid_FileDownloadList.SelectedItem;
                if (item == null)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                await Task.Factory.StartNew(() => {
                    foreach (var downloader in DownloadManager.Instance.Downloads)
                    {
                        if (downloader.fs_id == item.fs_id)
                        {
                            downloader.Start();
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_Start_Click", ex);
            }
        }

        private async void radioButton_PCS_Checked(object sender, RoutedEventArgs e)
        {
            if (Setting.APIMode != Setting.APIMODE.PCS)
            {
                Setting.APIMode = Setting.APIMODE.PCS;
                if (Setting.Baidu_Access_Token != null && !Setting.Baidu_Access_Token.Equals(""))
                {
                    try
                    {
                        totalData.ProgressRing_IsActive = true;
                        var folderResult = await LoadFolder("");
                        if (folderResult == null)
                        {
                            await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                        }
                        totalData.FileListDataItems = folderResult;
                    }catch (Exception ex)
                    {
                        LogHelper.WriteLog("MainWindow.radioButton_PCS_Checked", ex);
                    }finally
                    {
                        totalData.ProgressRing_IsActive = false;
                    }
                }
            }
        }

        private async void radioButton_BDC_Checked(object sender, RoutedEventArgs e)
        {
            if (Setting.APIMode != Setting.APIMODE.BDC)
            {
                Setting.APIMode = Setting.APIMODE.BDC;
                try
                {
                    totalData.ProgressRing_IsActive = true;
                    await Task.Factory.StartNew(() => {
                        if (BDC.IsCookieFileExist(Setting.Baidu_CookiePath))
                        {
                            BDC.LoadCookie(Setting.Baidu_CookiePath);
                            Setting.APIMode = Setting.APIMODE.BDC;
                        }
                    });

                    var folderResult = await LoadFolder("");
                    if (folderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = folderResult;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("MainWindow.radioButton_BDC_Checked", ex);
                }finally
                {
                    totalData.ProgressRing_IsActive = false;
                }
            }
        }

        private async void MenuItem_CopyTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                if (dataGrid_FileList.SelectedItems.Count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                Window.FolderListWindow FLW = new Window.FolderListWindow(GlobalLanguage.FindText("DataGrid_FileList_ContextMenu_CopyTo"));
                FLW.Owner = this;
                string dest = "";
                if ((bool)FLW.ShowDialog())
                {
                    dest = FLW.SelectedPath;
                }
                else
                {
                    return;
                }

                if (!dest.Equals(""))
                {
                    List<DBCCopyStruct> copyList = new List<DBCCopyStruct>();
                    foreach (FileListDataItem file in dataGrid_FileList.SelectedItems)
                    {
                        copyList.Add(new DBCCopyStruct { path = file.path, dest = dest, newname = file.file });
                    }
                    if (await BDC.CopyTo(copyList))
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_MenuItem_CopyTo_Succeed"));
                    }
                    else
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("MainWindow_MenuItem_CopyTo_Error"));
                    }
                }else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_MenuItem_CopyTo_DestError"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_CopyTo_Click", ex);
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                if (dataGrid_FileList.SelectedItems.Count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                List<string> pathList = new List<string>();
                foreach (FileListDataItem file in dataGrid_FileList.SelectedItems)
                {
                    pathList.Add(file.path);
                }
                await BDC.DeleteAsync(pathList.ToArray());
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_MenuItem_Delete_Succeed"));
                TransferPage(totalData.PageNowLoaded);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_Delete_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_MenuItem_Delete_Fail"));
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_MoveTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                if (dataGrid_FileList.SelectedItems.Count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                Window.FolderListWindow FLW = new Window.FolderListWindow(GlobalLanguage.FindText("DataGrid_FileList_ContextMenu_MoveTo"));
                FLW.Owner = this;
                string dest = "";
                if ((bool)FLW.ShowDialog())
                {
                    dest = FLW.SelectedPath;
                }
                else
                {
                    return;
                }

                if (!dest.Equals(""))
                {
                    List<DBCCopyStruct> copyList = new List<DBCCopyStruct>();
                    foreach (FileListDataItem file in dataGrid_FileList.SelectedItems)
                    {
                        copyList.Add(new DBCCopyStruct { path = file.path, dest = dest, newname = file.file });
                    }
                    if (await BDC.MoveTo(copyList))
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_MenuItem_MoveTo_Succeed"));
                        var folderResult = await LoadFolder(totalData.PageNowLoaded);
                        if (folderResult == null)
                        {
                            await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                        }
                        totalData.FileListDataItems = folderResult;
                    }
                    else
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("MainWindow_MenuItem_MoveTo_Error"));
                    }
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_MenuItem_MoveTo_DestError"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_MoveTo_Click", ex);
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_Share_Click(object sender, RoutedEventArgs e)
        {
            if (!BDC.CheckCookie())
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                return;
            }
            Window.GenerateShareLinkWindow GSLW = new Window.GenerateShareLinkWindow(((FileListDataItem)dataGrid_FileList.SelectedItem).fs_id);
            GSLW.Owner = this;
            GSLW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            GSLW.ShowDialog();
        }

        private async void button_TransforLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                var shareLink = await this.ShowInputAsync(GlobalLanguage.FindText("MainWindow_Button_TransforLink_Title"), GlobalLanguage.FindText("MainWindow_Button_TransforLink_Message"));
                if (shareLink == null || shareLink.Equals(""))
                {
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                if (!shareLink.StartsWith("http://") && !shareLink.StartsWith("https://"))
                {
                    shareLink = "http://" + shareLink;
                }
                string toFolder = "/Temp";
                try
                {
                    var testPath = await BDC.OnlyFolderInfo(toFolder);
                }catch (ErrorCodeException)
                {
                    await BDC.CreateFolderAsync(toFolder);
                }
                
                string path = await BDC.Transfer(shareLink, toFolder);
                //var fileMeta = await PCS.SingleFileMeta(path);
                string[] param = path.Split('/');
                var fileMeta = await BDC.SingleFolderAsync(path.Replace("/" + param[param.Count() - 1], ""), 1);
                ulong fs_id = 0UL;
                UInt32 mtime = 0U;
                UInt32 isdir = 0U;
                foreach(var file in fileMeta)
                {
                    if (file.path.Equals(path))
                    {
                        fs_id = file.fs_id;
                        mtime = file.mtime;
                        isdir = file.isdir;
                        break;
                    }
                }
                string[] fileName = path.Split('/');
                AddDownloadTask(new FileListDataItem
                {
                    file = fileName[fileName.Count() - 1],
                    path = path,
                    fs_id = fs_id,
                    mtime = Tools.TimeStamp2DateTime(mtime.ToString()),
                    isdir = isdir,
                    isSelected = false
                });

            }catch (ErrorCodeException)
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("MainWindow_Button_TransforLink_ErrorCodeMessage"));
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_TransforLink_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void MenuItem_CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                var folderPath = await this.ShowInputAsync(GlobalLanguage.FindText("MainWindow_MenuItem_CreateFolder_Title"), GlobalLanguage.FindText("MainWindow_MenuItem_CreateFolder_Message"));
                if (folderPath == null || folderPath.Equals(""))
                {
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                if (await BDC.CreateFolderAsync(string.Format("{0}/{1}", totalData.PageNowLoaded, folderPath)))
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_MenuItem_CreateFolder_Succeed"));
                    var folderResult = await LoadFolder(totalData.PageNowLoaded);
                    if (folderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = folderResult;
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("MainWindow_Button_TransforLink_ErrorCodeMessage"));
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_CreateFolder_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
            
        }

        private async void MenuItem_Rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BDC.CheckCookie())
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("CommonMessage_NeedLogin"));
                    return;
                }
                if (dataGrid_FileList.SelectedItems.Count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                if (dataGrid_FileList.SelectedItems.Count > 1)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_MenuItem_Rename_SupportOne"));
                    return;
                }
                totalData.ProgressRing_IsActive = true;
                FileListDataItem item = (FileListDataItem)dataGrid_FileList.SelectedItem;
                var newName = await this.ShowInputAsync(GlobalLanguage.FindText("MainWindow_MenuItem_Rename_NewName_Title"), GlobalLanguage.FindText("MainWindow_MenuItem_Rename_NewName_Message"));
                if (newName == null || newName.Equals(""))
                {
                    return;
                }
                if (await BDC.RenameAsync(item.path, newName))
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_MenuItem_Rename_NewName_Succeed"));
                    var folderResult = await LoadFolder(totalData.PageNowLoaded);
                    if (folderResult == null)
                    {
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_LoadFolderInfoFailed"));
                    }
                    totalData.FileListDataItems = folderResult;
                }
                else
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("MainWindow_MenuItem_Rename_NewName_Failed"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_Rename_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void button_ClearCookies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(Setting.Baidu_CookiePath))
                {
                    File.Delete(Setting.Baidu_CookiePath);
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("MainWindow_Button_ClearCookies_Succeed"));
                }
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_ClearCookies_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }
        }

        private async void MenuItem_DownloadList_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGrid_FileDownloadList.SelectedItems.Count == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_DataGrid_SelectedNull"));
                    return;
                }
                var downloader = DownloadManager.Instance.Downloads;
                var needDeleteSource = await this.ShowMessageAsync(GlobalLanguage.FindText("MainWindow_MenuItem_DownloadList_Delete_NeedDeleteSource_Title"), GlobalLanguage.FindText("MainWindow_MenuItem_DownloadList_Delete_NeedDeleteSource_Message"), MessageDialogStyle.AffirmativeAndNegative);

                foreach (DownloadListDataItem file in dataGrid_FileDownloadList.SelectedItems)
                {
                    if (DownloadManager.Instance.Downloads.Count() > 0)
                    {
                        foreach (var d in downloader)
                        {
                            if (file.fs_id == d.fs_id)
                            {
                                DownloadManager.Instance.RemoveDownload(d);
                                break;
                            }
                        }
                    }

                    totalData.DownloadListDataItems.Remove(file);
                    if (needDeleteSource == MessageDialogResult.Affirmative)
                    {
                        string path = System.IO.Path.Combine(Setting.DownloadPath, file.file);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                DownloadListAddItem(totalData.DownloadListDataItems, null);
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.MenuItem_DownloadList_Delete_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }
        }

        private async void button_FileList_NextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                totalData.ProgressRing_IsActive = true;
                var fileList = await LoadFolder(totalData.PageNowLoaded, ++totalData.PageCounter);
                if (fileList == null || fileList.Count() == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("MainWindow_Button_FileList_NextPage_LastPage_Title"), GlobalLanguage.FindText("MainWindow_Button_FileList_NextPage_LastPage_Message"));
                    return;
                }
                totalData.FileListDataItems = fileList;
            }catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_FileList_NextPage_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }

        private async void button_FileList_PrePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                totalData.ProgressRing_IsActive = true;
                var fileList = await LoadFolder(totalData.PageNowLoaded, --totalData.PageCounter);
                if (fileList == null || fileList.Count() == 0)
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("MainWindow_Button_FileList_NextPage_LastPage_Title"), GlobalLanguage.FindText("MainWindow_Button_FileList_NextPage_LastPage_Message"));
                    return;
                }
                totalData.FileListDataItems = fileList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("MainWindow.button_FileList_NextPage_Click", ex);
                await this.ShowMessageAsync(GlobalLanguage.FindText("Message_Fail"), GlobalLanguage.FindText("CommonMessage_Exception"));
            }
            finally
            {
                totalData.ProgressRing_IsActive = false;
            }
        }
    }
}
