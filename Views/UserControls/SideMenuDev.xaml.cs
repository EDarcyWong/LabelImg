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

namespace LabelImg.Views.UserControls
{
    /// <summary>
    /// SideMenuDev.xaml 的交互逻辑
    /// </summary>
    public partial class SideMenuDev : UserControl
    {
        public event EventHandler<MenuItemClickedEventArgs> MenuItemClicked;
        public event EventHandler AddAnnotationClicked;
        public event EventHandler<bool> ShowAnnotationsChanged;
        public SideMenuDev()
        {
            InitializeComponent();
        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem clickedItem)
            {
                // Toggle the IsExpanded property of the clicked item
                bool shouldExpand = !clickedItem.IsExpanded;

                // Collapse all other top-level items
                foreach (TreeViewItem item in MenuTreeView.Items)
                {
                    if (item != clickedItem)
                    {
                        item.IsExpanded = false;
                    }
                }

                // Set the clicked item to the desired state (expanded/collapsed)
                clickedItem.IsExpanded = shouldExpand;

                // Prevent the event from propagating further
                e.Handled = true;
            }
        }

        private void SubItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem subItem)
            {
                // Handle subitem click without collapsing the parent menu
                // For example, you can show a message box with the subitem content
                //MessageBox.Show($"Selected: {subItem.Header}");

                // Prevent the event from propagating further
                e.Handled = true;


                if (MenuTreeView.SelectedItem is TreeViewItem selectedItem)
                {
                    string selectedContent = selectedItem.Header.ToString();
                    
                    MenuItemClicked?.Invoke(this, new MenuItemClickedEventArgs(selectedContent, true));
                    switch (selectedContent)
                    {
                        case "资源路径":
                            // Navigate to Resource Path
                            break;
                        case "标注工具":
                            // Navigate to Annotation Tool
                            break;
                        case "Profile":
                            // Navigate to Profile
                            break;
                        case "Logout":
                            // Perform Logout
                            break;
                        default:
                            // Handle subitems if needed
                            break;
                    }
                }
            }

        }

        private void ShowAnnotationsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Handle checkbox checked event
            //MessageBox.Show("显示标注图已启用");
            MenuItemClicked?.Invoke(this, new MenuItemClickedEventArgs(((CheckBox)sender).Content.ToString(), true));
        }

        private void ShowAnnotationsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Handle checkbox unchecked event
            //MessageBox.Show("显示标注图已禁用");
            MenuItemClicked?.Invoke(this, new MenuItemClickedEventArgs(((CheckBox)sender).Content.ToString(), false));
        }

        private void AddAnnotation_Click(object sender, RoutedEventArgs e)
        {
            AddAnnotationClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ShowAnnotationsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ShowAnnotationsChanged?.Invoke(this, true);
        }

        private void ShowAnnotationsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowAnnotationsChanged?.Invoke(this, false);
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class MenuItemClickedEventArgs : EventArgs
    {
        public string MenuItem { get; set; }
        public bool Checked { get; set; }

        public MenuItemClickedEventArgs(string menuItem, bool _checked)
        {
            MenuItem = menuItem;
            Checked = _checked;
        }
    }
}
