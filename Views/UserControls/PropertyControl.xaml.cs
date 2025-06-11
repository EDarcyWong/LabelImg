using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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
    /// PropertyControl.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyControl : UserControl
    {
        public ObservableCollection<PropertyItem> AllProperties { get; set; } = new();
        public ObservableCollection<PropertyItem> FilteredProperties { get; set; } = new();

        public PropertyControl()
        {
            InitializeComponent();
            DataContext = this;

            // 示例数据
            AllProperties.Add(new PropertyItem { Key = "Name", Value = "ChatGPT", IsEditable = true });
            AllProperties.Add(new PropertyItem { Key = "Version", Value = "4.0", IsEditable = false });
            AllProperties.Add(new PropertyItem { Key = "Status", Value = "Active", IsEditable = true });

            UpdateFilter();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            var sorted = AllProperties.OrderBy(p => p.Key).ToList();
            AllProperties.Clear();
            foreach (var item in sorted) AllProperties.Add(item);
            UpdateFilter();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            string filter = FilterTextBox.Text?.Trim().ToLower();
            FilteredProperties.Clear();
            foreach (var item in AllProperties)
            {
                if (string.IsNullOrEmpty(filter) || item.Key.ToLower().Contains(filter))
                {
                    FilteredProperties.Add(item);
                }
            }
        }
    }

    public class PropertyItem : INotifyPropertyChanged
    {
        private string _value;

        public string Key { get; set; }
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public bool IsEditable { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
