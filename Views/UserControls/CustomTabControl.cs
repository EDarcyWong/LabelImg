using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LabelImg.Views.UserControls
{
    public class CustomTabControl: TabControl
    {
        public ICommand RemoveTabCommand { get; }

        public CustomTabControl()
        {
            RemoveTabCommand = new RelayCommand<TabItem>(RemoveTab);
        }

        public void RemoveTab(TabItem tabItem)
        {
            if (tabItem != null && this.Items.Contains(tabItem))
            {
                this.Items.Remove(tabItem);
            }
        }
    }

}
