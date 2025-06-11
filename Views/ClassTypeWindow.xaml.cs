using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// ClassTypeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClassTypeWindow : Window
    {
        private ObservableCollection<DataItem> dataItems;
        private string originalValue;
        public DataItem SelectedItem { get; private set; }

        public ClassTypeWindow()
        {
            InitializeComponent();
            FillDataGrid();
        }

        private void FillDataGrid()
        {
            // 创建数据项集合
            dataItems = new ObservableCollection<DataItem>();

            // 将数据添加到集合
            foreach (var item in ObjectData.Data)
            {
                dataItems.Add(new DataItem { TypeIndex = item.Key, Object = item.Value });
            }

            // 将数据集合绑定到 DataGrid
            dataGrid.ItemsSource = dataItems;
            UpdateTotalCount();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 过滤 DataGrid 中的项
            var filter = searchTextBox.Text.ToLower();
            var filteredItems = dataItems.Where(item => item.Object.ToLower().Contains(filter));
            dataGrid.ItemsSource = new ObservableCollection<DataItem>(filteredItems);
            UpdateTotalCount();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            // 清空查询框内容
            searchTextBox.Text = string.Empty;
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            // 弹出输入框
            var dialog = new AddItemDialog();
            if (dialog.ShowDialog() == true)
            {
                var newItem = dialog.DataItem;

                // 检查TypeIndex和Object是否存在
                if (!string.IsNullOrWhiteSpace(newItem.Object) && newItem.TypeIndex != 0 && !dataItems.Any(item => item.TypeIndex == newItem.TypeIndex || item.Object.Trim() == newItem.Object.Trim()))
                {
                    dataItems.Add(newItem);
                    dataGrid.ItemsSource = new ObservableCollection<DataItem>(dataItems);
                    UpdateTotalCount();
                }
                else
                {
                    MessageBox.Show("类型索引和物体不能为空或空字符串，且不能重复。", "添加失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 获取选定的行
            var selectedItems = dataGrid.SelectedItems.Cast<DataItem>().ToList();

            // 弹出确认提示框
            var result = MessageBox.Show("确认删除选定的行吗？", "确认删除", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            // 如果用户确认删除，则执行删除操作
            if (result == MessageBoxResult.OK)
            {
                // 删除选定的行
                foreach (var selectedItem in selectedItems)
                {
                    dataItems.Remove(selectedItem);
                }

                UpdateTotalCount();
            }
        }


        private void dataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "类型索引")
            {
                originalValue = (e.Row.Item as DataItem).TypeIndex.ToString();
            }
            else if (e.Column.Header.ToString() == "物体")
            {
                originalValue = (e.Row.Item as DataItem).Object;
            }
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var editedItem = e.Row.Item as DataItem;
            var editingElement = e.EditingElement as TextBox;

            if (e.Column.Header.ToString() == "类型索引")
            {
                if (int.TryParse(editingElement.Text, out int newTypeIndex))
                {
                    if (newTypeIndex != 0 && !dataItems.Any(item => item.TypeIndex == newTypeIndex && item != editedItem))
                    {
                        editedItem.TypeIndex = newTypeIndex;
                    }
                    else
                    {
                        MessageBox.Show("类型索引不能为空且不能重复。", "修改失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        editingElement.Text = originalValue; // 还原为原来的值
                    }
                }
                else
                {
                    MessageBox.Show("请输入有效的类型索引。", "修改失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                    editingElement.Text = originalValue; // 还原为原来的值
                }
            }
            else if (e.Column.Header.ToString() == "物体")
            {
                var newObject = editingElement.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newObject) && !dataItems.Any(item => item.Object == newObject && item != editedItem))
                {
                    editedItem.Object = newObject;
                }
                else
                {
                    MessageBox.Show("物体名称不能为空且不能重复。", "修改失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                    editingElement.Text = originalValue; // 还原为原来的值
                }
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dataGrid.SelectedItem as DataItem;
            if (selectedItem != null)
            {
                selectedItemTextBlock.Text = $"选中行：类型索引={selectedItem.TypeIndex}, 物体={selectedItem.Object}";
            }
            else
            {
                selectedItemTextBlock.Text = string.Empty;
            }
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dataGrid.SelectedItem as DataItem;
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("请先选择一行。", "未选择", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void UpdateTotalCount()
        {
            // 更新总条数显示
            totalCountTextBlock.Text = $"总条数: {((ObservableCollection<DataItem>)dataGrid.ItemsSource).Count}";
        }

        public bool? ShowChoose()
        {
            confirmButton.Visibility = Visibility.Visible;
            return ShowDialog();
        }
    }

    // 数据项类
    public class DataItem : INotifyPropertyChanged
    {
        private int typeIndex;
        public int TypeIndex
        {
            get { return typeIndex; }
            set
            {
                typeIndex = value;
                OnPropertyChanged("TypeIndex");
            }
        }

        private string obj;
        public string Object
        {
            get { return obj; }
            set
            {
                obj = value;
                OnPropertyChanged("Object");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 示例数据
    public static class ObjectData
    {
        public static readonly Dictionary<int, string> Data = new Dictionary<int, string>
        {
            {0, "人"}, {1, "自行车"}, {2, "汽车"}, {3, "摩托车"}, {4, "飞机"}, {5, "公共汽车"}, {6, "火车"}, {7, "卡车"},
            {8, "船"}, {9, "交通灯"}, {10, "消防栓"}, {11, "停车标志"}, {12, "停车计时器"}, {13, "长凳"}, {14, "鸟"},
            {15, "猫"}, {16, "狗"}, {17, "马"}, {18, "羊"}, {19, "牛"}, {20, "大象"}, {21, "熊"}, {22, "斑马"},
            {23, "长颈鹿"}, {24, "背包"}, {25, "雨伞"}, {26, "手提包"}, {27, "领带"}, {28, "手提箱"}, {29, "飞盘"},
            {30, "滑雪板"}, {31, "滑雪板"}, {32, "运动球"}, {33, "风筝"}, {34, "棒球棒"}, {35, "棒球手套"}, {36, "滑板"},
            {37, "冲浪板"}, {38, "网球拍"}, {39, "瓶子"}, {40, "酒杯"}, {41, "杯子"}, {42, "叉子"}, {43, "刀"},
            {44, "勺子"}, {45, "碗"}, {46, "香蕉"}, {47, "苹果"}, {48, "三明治"}, {49, "橙子"}, {50, "西兰花"},
            {51, "胡萝卜"}, {52, "热狗"}, {53, "披萨"}, {54, "甜甜圈"}, {55, "蛋糕"}, {56, "椅子"}, {57, "沙发"},
            {58, "盆栽植物"}, {59, "床"}, {60, "餐桌"}, {61, "厕所"}, {62, "电视"}, {63, "笔记本电脑"}, {64, "鼠标"},
            {65, "远程"}, {66, "键盘"}, {67, "手机"}, {68, "微波炉"}, {69, "烤箱"}, {70, "烤面包机"}, {71, "水槽"},
            {72, "冰箱"}, {73, "书"}, {74, "时钟"}, {75, "花瓶"}, {76, "剪刀"}, {77, "泰迪熊"}, {78, "吹风机"},
            {79, "牙刷"}
        };
    }
}
