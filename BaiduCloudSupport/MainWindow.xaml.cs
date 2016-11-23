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
    }
}
