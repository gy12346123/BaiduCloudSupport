using BaiduCloudSupport.API;
using BaiduCloudSupport.Other;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
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
            Login.LoginWindow LW = new Login.LoginWindow();
            LW.Owner = this;
            LW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if ((bool)LW.ShowDialog())
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginSucceed"));
                totalData.ProgressRing_IsActive = true;
                var result = await ReloadSimpleUserInfo();
                totalData.ProgressRing_IsActive = false;
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
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginFailed"));
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
            if (Setting.Baidu_Access_Token != null && !Setting.Baidu_Access_Token.Equals(""))
            {
                if (Setting.Baidu_uname.Equals("") || Setting.UserPortraitFilePath.Equals(""))
                {
                    totalData.ProgressRing_IsActive = true;
                    var result = await ReloadSimpleUserInfo();
                    totalData.ProgressRing_IsActive = false;
                    if (!result)
                    {
                        await Task.Factory.StartNew(() => {
                            LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                        });
                        await this.ShowMessageAsync(GlobalLanguage.FindText("CommonTitle_Notice"), GlobalLanguage.FindText("MainWindow_AutoLoginFailed"));
                    }
                }else
                {
                    totalData.ProgressRing_IsActive = true;
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
                    totalData.ProgressRing_IsActive = false;
                }
            }else
            {
                await Task.Factory.StartNew(() => {
                    LoadUserPortraitFromFile(Setting.BasePath + @"Images\UserInfo\UserPortraitDefault.png");
                });
            }
            //LoadUserPortraitFromFile(Setting.BasePath + "8.jpg");
            //totalData.uname = "gy12346123";
            //totalData.Quota_Total = 123;
            //totalData.Quota_Used = 3;
        }

        public Task<bool> ReloadSimpleUserInfo()
        {
            return Task<bool>.Factory.StartNew(() => {
                ulong[] quota = PCS.Quota();
                if (quota != null && quota.Count() == 2)
                {
                    totalData.Quota_Total = quota[0];
                    totalData.Quota_Used = quota[1];
                }else
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
            });
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
    }
}
