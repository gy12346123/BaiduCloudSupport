using BaiduCloudSupport.API;
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

namespace BaiduCloudSupport.Window
{
    /// <summary>
    /// FolderListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FolderListWindow : MetroWindow
    {
        private List<Node> nodes;
        private BitmapImage Icon_NormalFolder = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/NormalFolder.png"));
        private BitmapImage Icon_NormalFolderWithPlus = new BitmapImage(new Uri("pack://application:,,,/Images/Icon/NormalFolderWithPlus.png"));
        private int countID = 1;
        public string CenterTitle { get; set; }

        public string SelectedPath { get; set; }

        public FolderListWindow(string title)
        {
            CenterTitle = title;
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBlock_Title.DataContext = this;
            FlushList("/");
        }

        private async void FlushList(string newPath)
        {
            var folderList = await BDC.OnlyFolderInfo(newPath);
            nodes = new List<Node>();
            foreach (var folder in folderList)
            {
                string[] name = folder.path.Split('/');
                BitmapImage icon;
                if (folder.dir_empty == 0)
                {
                    // Have sub folder
                    icon = Icon_NormalFolderWithPlus;
                }
                else
                {
                    // end
                    icon = Icon_NormalFolder;
                }
                nodes.Add(new Node { ID = countID, Path = folder.path, Name = name[name.Count() - 1], Icon = icon, dir_empty = folder.dir_empty });
                countID++;
            }
            List<Node> outputList = Bind(nodes);
            treeView_Folder.ItemsSource = outputList;
        }

        List<Node> Bind(List<Node> nodes)
        {
            List<Node> outputList = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ParentID == -1)
                {
                    outputList.Add(nodes[i]);
                }
                else
                {
                    FindDownward(nodes, nodes[i].ParentID).Nodes.Add(nodes[i]);
                }
            }
            return outputList;
        }

        Node FindDownward(List<Node> nodes, int id)
        {
            if (nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ID == id)
                {
                    return nodes[i];
                }
                Node node = FindDownward(nodes[i].Nodes, id);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string path = ((TextBlock)sender).ToolTip.ToString();
            Node selectedNode = new Node { dir_empty = 1};
            Parallel.ForEach(nodes, (node, state)=> {
                if (node.Path.Equals(path))
                {
                    selectedNode = node;
                    state.Stop();
                }
            });
            if (selectedNode.dir_empty == 1)
            {
                return;
            }
            FlushList(path);
            ChangeNavigation(path);
            SelectedPath = path;
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

        private void TransferPage(string path)
        {
            FlushList(path);
            ChangeNavigation(path);
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            Node selectedNode = (Node)treeView_Folder.SelectedItem;
            if (selectedNode != null)
            {
                SelectedPath = selectedNode.Path;
                
            }
            DialogResult = true;
        }
    }

    public class Node
    {
        public Node()
        {
            this.Nodes = new List<Node>();
            this.ParentID = -1;
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public int dir_empty { get; set; }
        public string Path { get; set; }
        public int ParentID { get; set; }
        public BitmapImage Icon { get; set; }
        public List<Node> Nodes { get; set; }
    }
}
