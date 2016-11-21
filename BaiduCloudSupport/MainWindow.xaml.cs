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
            if ((bool)LW.ShowDialog())
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginSucceed"));
            }
            else
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("CommonMessage_Result"), GlobalLanguage.FindText("LoginWindow_LoginFailed"));
            }
        }
    }
}
