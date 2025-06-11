using CommunityToolkit.Mvvm.ComponentModel;
using LabelImg.Helpers;
using LabelImg.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelImg.Views.UserControls
{
    /// <summary>
    /// SolutionExplorer.xaml 的交互逻辑
    /// </summary>
    public partial class SolutionExplorer : UserControl
    {
        public delegate void MyCustomEventHandler(object sender, SolutionClickEventArgs e);
        public delegate void RenamedEventHandler(object sender, RoutedEventArgs e);
        // 使用自定义事件数据
        public event MyCustomEventHandler MyCustomEvent;
        public event RenamedEventHandler RenamedEvent;
        public ObservableCollection<SolutionItem> Items { get; set; }

        private Border borderSolution;
        public string SolutionPath { get; set; }
        public string SolutionFilePath { get; set; }
        public SolutionExplorer()
        {
            InitializeComponent();
            Items = new ObservableCollection<SolutionItem>();
            this.DataContext = this;
            //LoadSolution("");
        }
        protected virtual void OnMyCustomEvent(SolutionClickEventArgs e)
        {
            MyCustomEventHandler handler = MyCustomEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnRenamedEvent(RoutedEventArgs e)
        {
            RenamedEventHandler handler = RenamedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 添加对选中项更改的事件处理程序
            // Retrieve the resource from UserControl's resources. 
        }
         

        private void MenuItem_AddImage_Click(object sender, RoutedEventArgs e)
        {
            // Add image logic here
            var selectedItem = solutionTreeView.SelectedItem;
            if (selectedItem != null)
            {
                SolutionItem solutionItem = (SolutionItem)selectedItem;
                if(solutionItem.Name == "train" || solutionItem.Name == "val")
                {
                    Debug.WriteLine(LabelImg.Models.TextFileTool.CombinePaths(SolutionPath, solutionItem.RelativePath));
                    // 创建一个OpenFileDialog来选择图片
                    var openFileDialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Multiselect = true,
                        Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
                    };

                    bool? result = openFileDialog.ShowDialog();

                    if (result == true)
                    {
                        // 用户选择的图片文件路径
                        string[] selectedFiles = openFileDialog.FileNames;

                        // 目标目录，可以根据需要更改
                        string targetDirectory = LabelImg.Models.TextFileTool.CombinePaths(SolutionPath, solutionItem.RelativePath);

                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }

                        foreach (var file in selectedFiles)
                        {
                            try
                            {
                                string extension = Path.GetExtension(file); // 包括点，例如 ".png"

                                // 使用 System.Drawing 读取和转换图像
                                using (var bitmap = new System.Drawing.Bitmap(file))
                                {
                                    // 获取文件名并替换扩展名为 .jpg
                                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                                    string newFileName = fileNameWithoutExtension + extension;

                                    // 目标文件路径
                                    string destFile = System.IO.Path.Combine(targetDirectory, newFileName);

                                    // 如果目标路径已存在相同名称的文件，则添加括号编号
                                    int count = 1;
                                    while (File.Exists(destFile))
                                    {
                                        newFileName = $"{fileNameWithoutExtension} ({count}){extension}";
                                        destFile = System.IO.Path.Combine(targetDirectory, newFileName);
                                        count++;
                                    }
                                    // 保存为 jpg 格式
                                    bitmap.Save(destFile, System.Drawing.Imaging.ImageFormat.Jpeg);

                                    // 在 Items 列表中递归查找指定名称的项
                                    var parentItem = FindItemByRelativePath(Items, solutionItem.RelativePath);
                                    if (parentItem != null)
                                    {
                                        // 获取目录中所有的 JPG 文件
                                        string[] imageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                                        string[] imageFiles = Array.Empty<string>();

                                        if (Directory.Exists(targetDirectory))
                                        {
                                            imageFiles = Directory
                                                .EnumerateFiles(targetDirectory)
                                                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                                                .ToArray();
                                        }
                                        parentItem.Items.Clear();
                                        // 循环处理每个 JPG 文件
                                        foreach (string jpgFile in imageFiles)
                                        {
                                            // 获取文件名（不包含路径）
                                            string fileName = System.IO.Path.GetFileName(jpgFile);

                                            // 创建新的 SolutionItem，并添加到 parentItem.Items 列表中
                                            parentItem.Items.Add(
                                                new SolutionItem {
                                                    SubIndex = Items.Count, Name = fileName, IconPath = "/Images/pic3.png", RelativePath = $"{solutionItem.RelativePath}/{fileName}",
                                                    Level = 3,
                                                    ParentIndex = parentItem.SubIndex,
                                                }
                                            );
                                            parentItem.IsExpanded = true;
                                            //Items[0].Items[parentItem.ParentIndex].IsExpanded = true;
                                        }
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error converting or copying file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        MessageBox.Show("Images have been successfully copied and converted to JPG format.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private SolutionItem FindItemByRelativePath(IEnumerable<SolutionItem> items, string relativePath)
        {
            foreach (var item in items)
            {
                if (item.RelativePath == relativePath)
                {
                    return item;
                }

                // 递归搜索嵌套项
                SolutionItem nestedItem = FindItemByRelativePath(item.Items, relativePath);
                if (nestedItem != null)
                {
                    return nestedItem;
                }
            }

            return null;
        }
        private SolutionItem FindAndRemoveItemByRelativePath(List<SolutionItem> items, string relativePath)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item.RelativePath == relativePath)
                {
                    items.RemoveAt(i);
                    return item;
                }

                // 递归搜索嵌套项
                var nestedItem = FindAndRemoveItemByRelativePath(item.Items.ToList(), relativePath);
                if (nestedItem != null)
                {
                    item.Items.Remove(nestedItem);
                    return nestedItem;
                }
            }

            return null;
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            // 处理打开操作
            var selectedItem = solutionTreeView.SelectedItem;
            if (selectedItem != null)
            {
                MessageBox.Show($"Open item: {selectedItem}");
            }
        }
        private void MenuItem_Rename_Click(object sender, RoutedEventArgs e)
        {
            RenameAllImages();
            OnRenamedEvent(e);
            LoadSolution(SolutionFilePath);
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            // 处理删除操作
            var selectedItem = solutionTreeView.SelectedItem;
            if (selectedItem != null)
            {
                SolutionItem sitem = (SolutionItem)selectedItem;
                FindItemByRelativePath(Items, sitem.RelativePath.Replace($"/{sitem.Name}", "")).IsExpanded = true;
                //Items[0]
                string relativePath = sitem.RelativePath;
                List<SolutionItem> list = Items.ToList();
                string fullPath = TextFileTool.CombinePaths(SolutionPath, relativePath);
                string parentRelative = relativePath.Replace($"/{sitem.Name}", "");
                SolutionItem p = FindItemByRelativePath(Items, parentRelative);

                if (p.Level == 2)
                {
                    p.IsExpanded = true;
                    Items[0].Items[p.ParentIndex].IsExpanded = true;
                }
                // 检查文件是否存在
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show("文件不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 显示确认对话框
                MessageBoxResult result = MessageBox.Show($"确定要删除文件 {fullPath} 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // 如果用户选择 "Yes"，则删除文件
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(fullPath);
                        MessageBox.Show("文件已成功删除。", "删除成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        FindAndRemoveItemByRelativePath(list, relativePath);
                        Items.Clear();
                        foreach (var item in list)
                        {
                            Items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除文件时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
         

        private MenuItem CreateMenuItem(string header, RoutedEventHandler clickHandler)
        {
            var menuItem = new MenuItem
            {
                Header = header
            };
            menuItem.Click += clickHandler;
            var menuItemStyle = (Style)FindResource("CustomMenuItemStyle");
            menuItem.Style = menuItemStyle;
            return menuItem;
        }
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        public void LoadSolution(string ysnlFilePath)
        {
            if (!File.Exists(ysnlFilePath))
            {
                return;
            }
            SolutionFilePath = ysnlFilePath;
            SolutionPath = Directory.GetParent(ysnlFilePath)!.FullName;
            Items.Clear();

            // 读取 .ysnl 文件并解析
            SolutionInfo solutionInfo = (new SolutionTool()).ReadYsnlFile(ysnlFilePath);
            if (solutionInfo != null)
            {
                var solution = new SolutionItem
                {
                    SubIndex = 0,
                    Name = $"解决方案{solutionInfo.SolutionName}",
                    IconPath = "/Images/solution1.png",
                    IsExpanded = true,
                    Level = 0,
                    ParentIndex = 0
                };

                var imgs = new SolutionItem
                {
                    SubIndex = 1,
                    Name = $"images",
                    IconPath = "/Images/folder4.png",
                    RelativePath = $"/images/",
                    Level = 1,
                    ParentIndex = 0
                };
                solution.Items.Add(imgs);

                var labels = new SolutionItem
                {
                    SubIndex = 1,
                    Name = $"labels",
                    IconPath = "/Images/folder4.png",
                    RelativePath = $"/labels/",
                    Level = 1,
                    ParentIndex = 0
                };
                solution.Items.Add(labels);

                var train1 = new SolutionItem
                {
                    SubIndex = 0,
                    Name = $"train",
                    IconPath = "/Images/folder4.png",
                    RelativePath = $"/images/train",
                    Level = 2,
                    ParentIndex = 2
                };

                var val1 = new SolutionItem
                {
                    SubIndex = 1,
                    Name = $"val",
                    IconPath = "/Images/folder4.png",
                    RelativePath = $"/images/val",
                    Level = 2,
                    ParentIndex = 2
                };
                imgs.Items.Add(train1);
                imgs.Items.Add(val1);

                loadJpg(Directory.GetParent(ysnlFilePath)?.FullName + "\\images\\train\\", train1, 2);
                loadJpg(Directory.GetParent(ysnlFilePath)?.FullName + "\\images\\val\\", val1, 2);


                //var train2 = new SolutionItem
                //{
                //    SubIndex = 0,
                //    Name = $"train",
                //    IconPath = "/Images/folder4.png",
                //    RelativePath = $"/labels/train",
                //    Level = 2,
                //    ParentIndex = 2
                //};

                //var val2 = new SolutionItem
                //{
                //    SubIndex = 1,
                //    Name = $"val",
                //    IconPath = "/Images/folder4.png",
                //    RelativePath = $"/images/val",
                //    Level = 2,
                //    ParentIndex = 2
                //};
                //labels.Items.Add(train2);
                //labels.Items.Add(val2);

                Items.Add(solution);
            }
        }
        private void loadJpg(string targetDirectory, SolutionItem parentItem, int parentIndex)
        {
            string[] imageExtensions = new[] { ".jpg", ".jpeg", ".png" };
            string[] imageFiles = Array.Empty<string>();

            if (Directory.Exists(targetDirectory))
            {
                imageFiles = Directory
                    .EnumerateFiles(targetDirectory)
                    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();
            }
            //string[] jpgFiles = Directory.GetFiles(targetDirectory, "*.jpg");
            parentItem.Items.Clear();
            // 循环处理每个 JPG 文件
            foreach (string jpgFile in imageFiles)
            {
                // 获取文件名（不包含路径）
                string fileName = System.IO.Path.GetFileName(jpgFile);

                // 创建新的 SolutionItem，并添加到 parentItem.Items 列表中
                parentItem.Items.Add(
                    new SolutionItem { SubIndex = Items.Count, Name = fileName, IconPath = "/Images/pic3.png", RelativePath = $"{parentItem.RelativePath}/{fileName}",
                        Level = 3,
                        ParentIndex = parentIndex
                    }
                );
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        
        private void solutionTreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 从事件参数中获取TreeView
            var treeView = sender as TreeView;
            if (treeView == null) return;

            // 获取被选中的项目（注意此处是您的数据模型）
            var selectedItem = treeView.SelectedItem;

            if (selectedItem == null) return;


            SolutionItem selectedTreeViewItem = (SolutionItem)selectedItem;

            if (selectedTreeViewItem.Name.EndsWith(".png") || selectedTreeViewItem.Name.EndsWith(".jpg") || selectedTreeViewItem.Name.EndsWith(".txt"))
            {
                Debug.WriteLine(selectedTreeViewItem.Name);
                OnMyCustomEvent(new SolutionClickEventArgs(SolutionPath, selectedTreeViewItem));
            }

            if (borderSolution == null)
            {

                // 从TreeView的当前选中项获取TreeViewItem
                var container = treeView.ItemContainerGenerator.ContainerFromItem(selectedItem) as TreeViewItem;

                // 如果找不到TreeViewItem，尝试向上寻找
                if (container == null)
                {
                    var selected = e.OriginalSource as DependencyObject;
                    if (selected != null)
                    {
                        container = FindParent<TreeViewItem>(selected);
                    }
                }

                // 您现在有了TreeViewItem（如果它存在的话），可以尝试根据您的需求更改其样式或子控件属性
                // 示例：更改TreeViewItem的背景颜色
                if (container != null)
                {
                    // 寻找Border并更改其背景颜色
                    // 注意，这个示例可能需要根据实际的样式和结构修改
                    var border = container.Template.FindName("boderSolutionControl", container) as Border;
                    if (border != null)
                    {
                        //border.Background = new SolidColorBrush(Colors.Red); // 可以选择您想要的颜色
                        borderSolution = border;
                    }
                }
            }

            if (borderSolution == null) return;
            // 获取选中项
            //SolutionItem selectedTreeViewItem = (SolutionItem)selectedItem;
            if (selectedTreeViewItem != null)
            {
                if (selectedTreeViewItem.Level == 0)
                {
                    borderSolution.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7"));
                }
                else
                {
                    borderSolution.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void solutionTreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // 如果在设计器模式下，跳过一些代码
                return;
            }
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;

                var clickedItem = treeViewItem.DataContext as SolutionItem;

                if (clickedItem != null)
                {
                    var contextMenu = new ContextMenu();
                    var contextMenuStyle = (Style)FindResource("CustomContextMenuStyle");
                    if (contextMenuStyle != null)
                    {
                        contextMenu.Style = contextMenuStyle;
                    }


                    if (clickedItem.Level == 0)
                    {
                        contextMenu.Items.Add(CreateMenuItem("Open", MenuItem_Open_Click));
                    }
                    else if (clickedItem.Level == 1)
                    {
                        contextMenu.Items.Add(CreateMenuItem("自动命名图片", MenuItem_Rename_Click));
                    }
                    else if (clickedItem.Level == 2)
                    {
                        contextMenu.Items.Add(CreateMenuItem("Add Image", MenuItem_AddImage_Click));
                    }
                    else if (clickedItem.Level == 3)
                    {
                        contextMenu.Items.Add(CreateMenuItem("Delete", MenuItem_Delete_Click));
                    }

                    treeViewItem.ContextMenu = contextMenu;
                    contextMenu.IsOpen = true;
                }
            }
        }

        void RenameAllImages()
        {
            string imagesFolderPath = SolutionPath+"/dataset/images"; // 修改为你的images文件夹路径
            string labelsFolderPath = SolutionPath + "/dataset/labels"; // 修改为你的labels文件夹路径
            string[] subFolders = { "train", "val" };
            Dictionary<string, string> renameMapping = new Dictionary<string, string>();
            int currentNumber = 1;

            HashSet<string> usedFileNames = new HashSet<string>();

            foreach (var subFolder in subFolders)
            {
                string imageFolderPath = Path.Combine(imagesFolderPath, subFolder);
                string labelFolderPath = Path.Combine(labelsFolderPath, subFolder);

                if (Directory.Exists(imageFolderPath))
                {
                    var imageFiles = Directory.GetFiles(imageFolderPath)
                        .Where(f => IsImageFile(f)).ToList();

                    foreach (var file in imageFiles)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string extension = Path.GetExtension(file);

                        // 检查文件名是否符合7位数字的规则
                        if (!IsValidFileName(fileName) || usedFileNames.Contains(fileName + extension))
                        {
                            string newFileName;
                            do
                            {
                                newFileName = GenerateNewFileName(currentNumber, extension);
                                currentNumber++;
                            } while (usedFileNames.Contains(newFileName));

                            string newFilePath = Path.Combine(imageFolderPath, newFileName);

                            // 重命名图片文件
                            File.Move(file, newFilePath);
                            renameMapping.Add(file, newFilePath);
                            usedFileNames.Add(newFileName);

                            // 同步重命名对应的标签文件
                            string oldLabelFile = Path.Combine(labelFolderPath, fileName + ".txt");
                            if (File.Exists(oldLabelFile))
                            {
                                string newLabelFile = Path.Combine(labelFolderPath, Path.GetFileNameWithoutExtension(newFileName) + ".txt");
                                File.Move(oldLabelFile, newLabelFile);
                                renameMapping.Add(oldLabelFile, newLabelFile);
                            }
                        }
                        else
                        {
                            usedFileNames.Add(fileName + extension);
                        }
                    }
                }
            }

            // 输出重命名前后的名称对
            foreach (var kvp in renameMapping)
            {
                Debug.WriteLine($"Old Name: {kvp.Key} => New Name: {kvp.Value}");
            }
        }


         bool IsImageFile(string filePath)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png"};
            string extension = Path.GetExtension(filePath).ToLower();
            return validExtensions.Contains(extension);
        }

         bool IsValidFileName(string fileName)
        {
            return fileName.Length == 7 && int.TryParse(fileName, out _);
        }

         string GenerateNewFileName(int number, string extension)
        {
            string numberStr = number.ToString().PadLeft(7, '0');
            return numberStr + extension;
        }
    }

    public class SolutionClickEventArgs
    {
        public SolutionClickEventArgs() { }
        
        public SolutionClickEventArgs(string _solutionPath, SolutionItem _item)
        {
            SolutionPath = _solutionPath;
            SolutionItem = _item;
        }
        public string SolutionPath { get; set; }
        public SolutionItem SolutionItem { get; set; }
    }

    public partial class SolutionItem : ObservableObject
    {
        [ObservableProperty]
        private int subIndex;

        [ObservableProperty]
        private int level;

        [ObservableProperty]
        private int parentIndex;

        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string iconPath;

        [ObservableProperty]
        private string relativePath;

        [ObservableProperty]
        private ObservableCollection<SolutionItem> items;

         

        public SolutionItem()
        {
            Items = new ObservableCollection<SolutionItem>();
        }

    }

    public class CustomTreeViewItemStyleSelector : StyleSelector
    {
        public Style DefaultStyle { get; set; }
        public Style HiddenExpanderStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var treeViewItem = item as SolutionItem;
            if (treeViewItem != null && treeViewItem.Level == 0)
            {
                return HiddenExpanderStyle;
            }
            return DefaultStyle;
        }
    }
}
