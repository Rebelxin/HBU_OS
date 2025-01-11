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

namespace ReallyFrontend
{
    /// <summary>
    /// InputFileNameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputFileNameWindow : Window
    {
        public string InputName { get; private set; }

        public bool IsDirectory { get; private set; }
        public InputFileNameWindow()
        {
            InitializeComponent();
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            InputName = NameTextBox.Text;
            this.DialogResult = true;
            this.IsDirectory = false;
            this.Close();
        }

        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            InputName = NameTextBox.Text;
            this.DialogResult = true;
            this.IsDirectory = true;
            this.Close();
        }
    }
}
