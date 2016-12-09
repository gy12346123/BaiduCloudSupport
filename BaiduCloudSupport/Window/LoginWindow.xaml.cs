using BaiduCloudSupport.Login;
using CefSharp;
using CefSharp.Wpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public string Cookies { get; set; }

        public List<string> CookieList;

        private string URL;

        public string CookieLocalPath { get; set; }

        public LoginWindow(string address)
        {
            URL = address;
            CookieList = new List<string>();
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (CookieLocalPath != null && !CookieLocalPath.Equals(""))
            {
                LoadCookies(CookieLocalPath);
            }

            // Load baidu oauth address
            //webBrowser.Address = BaiduLogin.LoginWeb(Setting.Baidu_client_id, Setting.Baidu_redirect_uri);
            webBrowser.Address = URL;
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
                    else if (address.Contains("/disk/home"))
                    {
                        var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
                        CookieVisitor visitor = new CookieVisitor();
                        visitor.SendCookie += visitor_SendCookie;
                        cookieManager.VisitAllCookies(visitor);
                        Match match_Uid = Regex.Match(result, "(?<=\"uk\":)[0-9]{1,}(?=,)");
                        if (match_Uid.Success)
                        {
                            Setting.WriteAppSetting("Baidu_uid", match_Uid.Value, true);
                        }
                        this.DialogResult = true;
                    }
                    else if (address.Contains("error_description"))
                    {
                        // Login error
                        this.DialogResult = false;
                    }
                });
            }
        }

        public static Task SaveCookies(List<string> cookieList, FileInfo file)
        {
            return Task.Factory.StartNew(()=> {
                if (!Directory.Exists(file.DirectoryName))
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }
                using (StreamWriter SW = new StreamWriter(new FileStream(file.FullName, FileMode.Create)))
                {
                    foreach (string cookie in cookieList)
                    {
                        SW.WriteLine(cookie);
                    }
                }
            });
        }

        private async void LoadCookies(string file)
        {
            CookieList.Clear();
            using (StreamReader SR = new StreamReader(new FileStream(file, FileMode.Open)))
            {
                while (!SR.EndOfStream)
                {
                    CookieList.Add(SR.ReadLine());
                }
            }
            var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
            foreach (string cookie in CookieList)
            {
                string[] param = cookie.Split('$');
                await cookieManager.SetCookieAsync("http://yun.baidu.com", new CefSharp.Cookie
                {
                    Domain = param[0],
                    Name = param[1],
                    Value = param[2],
                    Expires = DateTime.MaxValue
                });
            }
        }

        private void visitor_SendCookie(CefSharp.Cookie obj)
        {
            CookieList.Add(string.Format("{0}${1}${2}${3}", obj.Domain, obj.Name, obj.Value, obj.Expires));
        }
    }

    public class CookieVisitor : CefSharp.ICookieVisitor
    {
        public event Action<CefSharp.Cookie> SendCookie;

        public void Dispose()
        {
        }

        public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;
            if (SendCookie != null)
            {
                SendCookie(cookie);
            }

            return true;
        }
    }
}
