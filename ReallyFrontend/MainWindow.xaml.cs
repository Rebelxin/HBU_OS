using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Path = System.IO.Path;
using Backend;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace ReallyFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string path;

        private string origin_path;

        private bool isShearing;

        private Backend.Files.FileSystem fileSystem;
        public MainWindow()
        {
            InitializeComponent();

            InitializeFileSystem();

            Refresh();
        }

        private void Refresh()
        {
            LoadTreeViewItems();
            LoadUniformGrid();
        }

        private void InitializeFileSystem()
        {
            // 初始化磁盘管理器和文件系统
            fileSystem= new();

            SetTextContent("成功初始化文件系统\n"+Backend.Disk.DiskManager.DiskPath);

            Backend.Files.FileSystem.FileNode rootNode = fileSystem.TraverseFileTree();
        }

        private void LoadUniformGrid()
        {
            BitMap.Children.Clear();
            var diskBlocks = fileSystem.GetDiskBitMap();
            for (int i = 0; i < diskBlocks.Length; i++)
            {
                // 创建一个Rectangle
                Rectangle rect = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = diskBlocks[i] ? Brushes.Green : Brushes.Red,
                    Stroke = Brushes.Black, // 设置边界颜色
                    StrokeThickness = 1 // 设置边界宽度
                };

                // 将Rectangle添加到UniformGrid
                BitMap.Children.Add(rect);
            }
        }

        private void LoadTreeViewItems()
        {
            FileTree.Items.Clear();
            var rootNode = fileSystem.TraverseFileTree();
            // 创建根节点
            TreeViewItem rootItem = new TreeViewItem();

            // 从资源中获取ContextMenu
            ContextMenu contextMenu = (ContextMenu)this.Resources["ItemContextMenu"];

            // 将ContextMenu赋给TreeViewItem
            rootItem.ContextMenu = contextMenu;
            rootItem.Header = rootNode.Name;
            rootItem.Selected += TreeViewItem_Selected;
            rootItem.PreviewMouseRightButtonDown += TreeViewItem_PreviewMouseRightButtonDown;

            FileTree.Items.Add(rootItem);

            LoadTreeChildrenItems(rootItem,rootNode);
        }

        private void LoadTreeChildrenItems(TreeViewItem fileItem, Backend.Files.FileSystem.FileNode fileNode)
        {
            foreach (var childNode in fileNode.Children) 
            { 
                TreeViewItem childItem = new TreeViewItem() { Header = childNode.Name };

                // 从资源中获取ContextMenu
                ContextMenu contextMenu = (ContextMenu)this.Resources["ItemContextMenu"];

                childItem.ContextMenu = contextMenu;
                childItem.Selected += TreeViewItem_Selected;
                childItem.PreviewMouseRightButtonDown += TreeViewItem_PreviewMouseRightButtonDown;

                fileItem.Items.Add(childItem);
                if (childNode.IsDirectory) 
                {
                    LoadTreeChildrenItems(childItem,childNode);
                }
            }
        }

        // 事件处理程序
        private void CreateItem_Click(object sender, RoutedEventArgs e)
        {
            if (!fileSystem.IsDirectory(path))
            {
                MessageBox.Show("无法在此创建文件对象");
                return;
            }
            InputFileNameWindow inputNameWindow = new InputFileNameWindow();
            if (inputNameWindow.ShowDialog() == true)
            {
                string fileName = inputNameWindow.InputName;
                bool isDirectory = inputNameWindow.IsDirectory;
                path = Path.Combine(path,fileName);
                
                fileSystem.CreateFileObject(path,isDirectory);
            }
            Refresh();
        }

        private void ModifyItem_Click(object sender, RoutedEventArgs e)
        { 
            ModifyFileObjectWindow modifyFileObjectWindow = new ModifyFileObjectWindow();
            if (modifyFileObjectWindow.ShowDialog()==true)
            {
                string fileName = modifyFileObjectWindow.InputName;

                fileSystem.ModifyFileObject(path,fileName);
            }
            Refresh();
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (path == "\\")
            {
                MessageBox.Show("根目录不可删除");
                return;
            }
            fileSystem.DeleteFileObject(path);
            Refresh();
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        { 
            isShearing = false;
            origin_path = path;
        }

        private void ShearItem_Click(object sender, RoutedEventArgs e)
        {
            isShearing = true;
            origin_path = path;
        }

        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Path.GetFileName(origin_path);
            path = Path.Combine(path, fileName);

            if (isShearing)
            {
                fileSystem.MoveFileObject(origin_path,path);
            }
            else
            {
                fileSystem.CopyFileObject(origin_path, path);
            }
            Refresh();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            string text;
            TreeViewItem item = sender as TreeViewItem;
            string fileName = item.Header.ToString();
            if (item.Header.ToString() == "rot")
            {
                fileName = "\\";
            }
            if (item != null)
            {
                if (e.OriginalSource == item)
                {
                    path = fileName;
                }
                else
                {
                    path = Path.Combine(fileName, path);
                }
                text = "选中文件对象: " + path+"\n";
                if (item.Header.ToString() == "rot")
                {
                    text+=fileSystem.GetFileObjectInformation(path);


                    if (!fileSystem.IsDirectory(path))
                    {
                        text += "文件内容：\n" + fileSystem.ReadFile(path);
                    }
                    
                }
                

                SetTextContent(text);
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                // 设置 TreeViewItem 为选中状态
                item.IsSelected = true;

                e.Handled = false;
            }
        }

        private void WriteFile(object sender, RoutedEventArgs e)
        {
            string text;
            if (fileSystem.IsDirectory(path))
            {
                text = "无法写入目录";
            }
            else {
                fileSystem.WriteData2File(path, GetInputContent());
                text = "写入成功";
            }
            SetTextContent(text);
        }


        private void SetTextContent(string content)
        {
            FileContent.Text = content;
        }

        private string GetTextContent()
        { 
            return FileContent.Text;
        }

        private string GetInputContent()
        { 
            return InputText.Text;
        }
    }
}