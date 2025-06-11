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

namespace LabelImg.Views
{
    /// <summary>
    /// AddItemDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddItemDialog : Window
    {
        public DataItem DataItem { get; private set; }

        public AddItemDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(typeIndexTextBox.Text, out int typeIndex) && !string.IsNullOrEmpty(objectTextBox.Text))
            {
                DataItem = new DataItem { TypeIndex = typeIndex, Object = objectTextBox.Text };
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("请输入有效的类型索引和物体名称。", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
