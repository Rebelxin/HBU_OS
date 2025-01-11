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
    /// ModifyFileObjectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyFileObjectWindow : Window
    {
        public string InputName { get; private set; }
        public ModifyFileObjectWindow()
        {
            InitializeComponent();
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            InputName = NameTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
