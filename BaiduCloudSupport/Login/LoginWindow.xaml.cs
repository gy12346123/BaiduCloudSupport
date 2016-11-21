using CefSharp;
using CefSharp.Wpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
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

namespace BaiduCloudSupport.Login
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load baidu oauth address
            webBrowser.Address = BaiduLogin.LoginWeb(Setting.Baidu_client_id, Setting.Baidu_redirect_uri);
            // Show browser
            webBrowser.Visibility = Visibility.Visible;
            // Browser frame load end event
            webBrowser.FrameLoadEnd += WebBrowser_FrameLoadEnd;
        }

        private async void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            // Get html source
            var result = await webBrowser.GetSourceAsync();
            if (result != null)
            {
                this.Dispatcher.Invoke(() => {
                    // Get the address which baidu redirect
                    string address = webBrowser.Address;
                    if (address.Contains("/login_success"))
                    {
                        // Login succeed, split url and parameters
                        string[] parm = address.Split('#')[1].Split('&');
                        foreach (string p in parm)
                        {
                            string[] sub = p.Split('=');
                            // Get each parameter
                            switch (sub[0])
                            {
                                case "access_token":
                                    MainWindow.totalData.Access_Token = sub[1];
                                    break;
                                case "expires_in":
                                    MainWindow.totalData.Expires_In = sub[1];
                                    break;
                                case "session_secret":
                                    MainWindow.totalData.Session_Secret = sub[1];
                                    break;
                                case "session_key":
                                    MainWindow.totalData.Session_Key = sub[1];
                                    break;
                                case "scope":
                                    MainWindow.totalData.Scope = sub[1];
                                    break;
                            }
                        }
                        this.DialogResult = true;
                    }
                    else if (address.Contains("error"))
                    {
                        // Login error
                        this.DialogResult = false;
                    }
                });
            }
        }
    }
}
