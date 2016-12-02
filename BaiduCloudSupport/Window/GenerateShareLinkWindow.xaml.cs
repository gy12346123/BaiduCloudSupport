using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// GenerateShareLinkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GenerateShareLinkWindow : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ulong fs_id;

        private string _ShareLink;
        public string ShareLink
        {
            get { return _ShareLink; }
            set
            {
                if (_ShareLink != value)
                {
                    _ShareLink = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ShareLink"));
                    }
                }
            }
        }

        private string _Password = "";
        public string Password
        {
            get { return _Password; }
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Password"));
                    }
                }
            }
        }

        public GenerateShareLinkWindow(ulong fs_id)
        {
            this.fs_id = fs_id;
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            grid_Main.DataContext = this;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string text = ((TextBox)sender).Text;
        }

        private async void Button_GeneratePublicLink_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            var result = await API.BDC.Share(fs_id);
            stackPanel_Generate.Visibility = Visibility.Hidden;
            stackPanel_Result.Visibility = Visibility.Visible;
            ShareLink = result.shorturl;
            progressBar.Visibility = Visibility.Hidden;
        }

        private async void Button_GeneratePrivateLink_Click(object sender, RoutedEventArgs e)
        {
            string text = Password;
            if (text.Count() != 4 && text.Count() != 0)
            {
                textBox_EnterPassword.Background = Brushes.Red;
                return;
            }
            progressBar.Visibility = Visibility.Visible;
            switch (text.Count())
            {
                case 4:
                    Regex regex = new Regex("^[a-zA-Z0-9]*$");
                    Match match = regex.Match(text);
                    if (!match.Success)
                    {
                        textBox_EnterPassword.Background = Brushes.Red;
                        return;
                    }
                    break;
                case 0:
                    Password = Other.Tools.GenerateStr(4);
                    break;
            }

            var result = await API.BDC.Share(fs_id, Password);
            stackPanel_Generate.Visibility = Visibility.Hidden;
            stackPanel_Result.Visibility = Visibility.Visible;
            textBox_ShowPassword.Visibility = Visibility.Visible;
            ShareLink = result.shorturl;
            progressBar.Visibility = Visibility.Hidden;
        }

        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            switch (textBox_ShowPassword.Visibility)
            {
                case Visibility.Visible:
                    Clipboard.SetText(string.Format(GlobalLanguage.FindText("GenerateShareLinkWindow_Copy_Format"), ShareLink, Password));
                    break;
                case Visibility.Hidden:
                    Clipboard.SetText(ShareLink);
                    break;
            }
            DialogResult = true;
        }
    }
}
