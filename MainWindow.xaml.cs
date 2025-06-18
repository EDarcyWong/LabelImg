using HandyControl.Controls;
using LabelImg.Models;
using LabelImg.ViewModels;
using LabelImg.Views;
using LabelImg.Views.UserControls;
using LabelImg.YOLO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using Window = System.Windows.Window;

namespace LabelImg
{
    public partial class MainWindow : Window
    {
        private ClassTypeWindow typeWindow;
        //private bool isDragging;
        //private bool isResizing;
        //private bool isAnchorDown;
        //private Point startPoint;
        //private Point lastMousePosition;
        //private Point lastMousePositionSub;
        //private double originalWidth;
        //private double originalHeight;
        //private string activeAnchor;
        //private BitmapSource cutImgSource;
        //private Label currentLabel;
        ////private ObservableCollection<CutLabelModel> cutList;
        //private double lblMarginOffsetLeft = 0;
        //private double lblMarginOffsetTop = 0;

        private MainViewModel viewModel;
        //private ImageCutWindow cutWindow;
        //private bool isShowCutWindow = false;
        private LabelGrid LabelGrid;
        private ObservableCollection<TxtDataRow> _parsedData;
        private string _txtFilePath;
        private List<UndoRedoStack> historyList = new();

        //private readonly YoloTool yolo;
        private PythonProcessRunner pythonRunner = new PythonProcessRunner();

		private List<YoloModelItem> _models;

		public MainWindow(string ysnlFilePath)
        {
            InitializeComponent();
			//string modelPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YOLO", "models", "yolo11x.onnx");
			//yolo = new YoloTool(modelPath);

			LoadModels();
			var app = Application.Current as App;
            if (app != null)
            {
                // 使用 app.Config 进行配置
                var dbConfig = app.Config.DatabaseConfig;
                var loggingConfig = app.Config.LoggingConfig;
                var appSettings = app.Config.AppSettings;

                //// 例如，显示数据库类型
                //MessageBox.Show($"数据库类型: {dbConfig.DbType}\n" +
                //                $"服务器: {dbConfig.Server}\n" +
                //                $"数据库名称: {dbConfig.DatabaseName}\n" +
                //                $"用户名: {dbConfig.Username}\n" +
                //                $"日志路径: {loggingConfig.LogFilePath}\n" +
                //                $"日志级别: {loggingConfig.LogLevel}\n" +
                //                $"UI 主题: {appSettings.UITheme}\n" +
                //                $"默认语言: {appSettings.DefaultLanguage}\n" +
                //                $"自动保存间隔: {appSettings.AutoSaveInterval} 分钟",
                //                "配置信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // 订阅 SideMenu 的 MenuItemClicked 事件
            sideMenu.MenuItemClicked += SideMenuControl_MenuItemClicked;

            //cutList = new ObservableCollection<CutLabelModel>();
            viewModel = (MainViewModel)this.DataContext;
            
            LoadData(ysnlFilePath);
        }
        protected override void OnClosed(EventArgs e)
        {
            pythonRunner.Dispose();
            base.OnClosed(e);
        }

        private void MySwitch_Click(object sender, RoutedEventArgs e)
        {
            if (MySwitch.IsChecked == true)
            {
                try
                {
                    pythonRunner.Start();
                    MessageBox.Show("Python 服务启动成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("启动 Python 服务失败：" + ex.Message);
                }
            }
            else
            {
                try
                {
                    pythonRunner.KillProcessByPort(5000);
                    MessageBox.Show("Python 服务关闭成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("关闭 Python 服务失败：" + ex.Message);
                }
            }
        }


        //private void YoloComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //	var client = new YoloClient("http://localhost:5000");

        //          // 切换模型
        //          //await client.LoadModelAsync("models/yolo_other.pt");
        //          Debug.WriteLine($"YoloComboBox.SelectedValue={YoloComboBox.SelectedValue}");
        //}
        private void LoadModels()
		{
			try
			{
				string json = File.ReadAllText("models.json");
				_models = JsonSerializer.Deserialize<List<YoloModelItem>>(json);
				YoloComboBox.ItemsSource = _models;
				YoloComboBox.DisplayMemberPath = "DisplayName";

                //YoloModelItem item = new YoloModelItem();
                //item.FileName  = "yolo11x.pt";
                //item.DisplayName = "YOLOv11x";
                //item.DownloadUrl = "https://github.com/ultralytics/assets/releases/download/v8.3.0/yolo11x.pt";
                //YoloComboBox.SelectedItem  = item;
                YoloComboBox.SelectedIndex = 20;
            }
			catch (Exception ex)
			{
				MessageBox.Show("模型配置加载失败: " + ex.Message);
			}
		}

		private async void YoloComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (YoloComboBox.SelectedItem is YoloModelItem model)
			{
				string modelsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YOLO", "models");
				Directory.CreateDirectory(modelsDir);

				string filePath = Path.Combine(modelsDir, model.FileName);

				if (!File.Exists(filePath))
				{
					try
					{
                        using var client = new HttpClient();
                        var data = await client.GetByteArrayAsync(model.DownloadUrl);
                        await File.WriteAllBytesAsync(filePath, data);
                        MessageBox.Show($"已下载模型到: {filePath}");


                        var client2 = new YoloClient("http://localhost:5000");

                        // 切换模型
                        await client2.LoadModelAsync($"models/{model.FileName}");
                        Debug.WriteLine($"YoloComboBox.SelectedValue={YoloComboBox.SelectedValue}");
					}
					catch (Exception ex)
					{
						MessageBox.Show($"下载失败: {ex.Message}");
					}
				}
				else
				{
					//MessageBox.Show($"模型已存在: {filePath}");
				}

				// 可在这里通知你的检测逻辑加载 filePath
			}
		}

		private async void OnDetectClick(object sender, RoutedEventArgs e)
        {

            //Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            //// Show OpenFileDialog
            //if (openFileDialog.ShowDialog() == true)
            //{

            //}

            if(LabelGrid==null || LabelGrid.ImagePath == null || !File.Exists(LabelGrid.ImagePath))
            {
                MessageBox.Show("请选择一张图片");
                return;
            }
            var imagePath = LabelGrid.ImagePath;

            try
            {
                var client = new YoloClient("http://localhost:5000");

                var results = await client.DetectAndParseAsync(imagePath);

                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => {
                    foreach (var result in results)
                    {
                        Debug.WriteLine($"Label: {result.label}, Confidence: {result.confidence:P1}");
                        Debug.WriteLine($"Center: ({result.center[0]}, {result.center[1]})");

                        CutLabelModel cut = new();
                        cut.Name = $"LBL_{RandomStringGenerator.Generate(6)}";
                        cut.ClassIndex = result.class_id;
                        cut.XCenter = result.center_norm[0];
                        cut.YCenter = result.center_norm[1];
                        cut.XWeight = result.weight[0];
                        cut.YWeight = result.weight[1];

                        LabelGrid.viewModel.CutList.Add(cut);
                        LabelGrid.OnControlSizeChanged();
                    }
                }));
                MessageBox.Show("检测完成，控制台查看结果");
            }
            catch (Exception ex)
            {
                MessageBox.Show("检测失败: " + ex.Message);
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
        private void SideMenuControl_MenuItemClicked(object? sender, MenuItemClickedEventArgs e)
        {
            Debug.WriteLine(e.MenuItem + " " + e.Checked);
            if (e.MenuItem == "保存路径")
            {
                setDataFolder();
            }
            else if (e.MenuItem == "添加图片")
            {
                loadImage();
            }
            else if (e.MenuItem == "添加标注")
            {
                addFlag();
            }
            else if (e.MenuItem == "显示标注图")
            {
                if (LabelGrid == null) return;
                LabelGrid.ShowImageCutWindow(e.Checked);
            }
            else if (e.MenuItem == "标注类别管理")
            {
                // Ensure the window is created and available
                if (typeWindow == null || !typeWindow.IsLoaded)
                {
                    typeWindow = new ClassTypeWindow();
                }

                // If the window is initialized and not visible, show the window
                if (!typeWindow.IsVisible)
                {
                    typeWindow.ShowDialog();
                }
                else
                {
                    typeWindow.Activate();
                }
            }
        }

        private void addFlag()
        {

            if (LabelGrid == null) return;
            // 创建 ClassTypeWindow 实例
            var classTypeWindow = new ClassTypeWindow();

            // 显示对话框
            var result = classTypeWindow.ShowChoose();

            // 检查对话框是否关闭，并且用户是否点击了确定按钮
            if (result.HasValue && result.Value)
            {
                // 获取用户选定的行
                var selectedItem = classTypeWindow.SelectedItem;

                // 执行其他操作，比如显示选定的行的信息等
                //MessageBox.Show($"选定的行：类型索引={selectedItem.TypeIndex}, 物体={selectedItem.Object}");
                LabelGrid.AddCut(selectedItem.TypeIndex);
            }

        }
        private void Add_Flag_Click(object sender, RoutedEventArgs e)
        {
            addFlag();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            //isDragging = false;
            //isResizing = false;
            //activeAnchor = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }


        //private void cbShowCut_Checked(object sender, RoutedEventArgs e)
        //{
        //    ShowImageCutWindow();
        //}

        //private void cbShowCut_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (cutWindow != null)
        //    {
        //        cutWindow.Close();
        //    }
        //}

        private void Window_Closed(object sender, EventArgs e)
        {
            //if (cutWindow != null)
            //{
            //    cutWindow.Close();
            //}

            // 退出应用程序的逻辑
            Application.Current.Shutdown();
        }


        private void LoadData(string ysnlFilePath)
        {
            solutionExplorer.RenamedEvent += SolutionExplorer_RenamedEvent;
            solutionExplorer.LoadSolution(ysnlFilePath);

            //var solution = new SolutionItem { Index = 0, Name = "Yolo测试解决方案", IconPath = "/Images/solution1.png", IsExpanded = true };
            //var project1 = new SolutionItem { Index = 1, Name = "Project 'Project1'", IconPath = "/Images/folder4.png" };
            //project1.Items.Add(new SolutionItem { Index = 2, Name = "File1.png", IconPath = "/Images/pic3.png" });
            //project1.Items.Add(new SolutionItem { Index = 3, Name = "File2.png", IconPath = "/Images/pic3.png" });

            //var project2 = new SolutionItem { Index = 4, Name = "Project 'Project2'", IconPath = "/Images/folder4.png" };
            //project2.Items.Add(new SolutionItem { Index = 5, Name = "File3.png", IconPath = "/Images/pic3.png" });
            //project2.Items.Add(new SolutionItem { Index = 6, Name = "File4.png", IconPath = "/Images/pic3.png" });

            //solution.Items.Add(project1);
            //solution.Items.Add(project2);
            //solutionExplorer.LoadSolution(solution);

        }

        private void SolutionExplorer_RenamedEvent(object sender, RoutedEventArgs e)
        {
            tbMain.Items.Clear();
        }

        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblInfos.SelectedItem != null)
            {
                var selectedRowData = lblInfos.SelectedItem;
                if (selectedRowData is CutLabelModel)
                {
                    LabelGrid.ActiveLabelByName(((CutLabelModel)selectedRowData).Name);
                }
            }
        }

        private void SelectDataGridRow(CutLabelModel item)
        {
            lblInfos.SelectedItem = item;
            lblInfos.ScrollIntoView(item); // Optionally scroll to the item
        }


        private void modifyClassIndexMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 获取右键点击的单元格
            var cell = lblInfos.SelectedCells[0];

            // 获取单元格所在行和列的索引
            var row = lblInfos.Items.IndexOf(cell.Item);
            var col = cell.Column.DisplayIndex;

            // 获取单元格的值
            var oldValue = (cell.Item as CutLabelModel).ClassIndex;

            // 打开 ClassTypeWindow 对话框，并获取用户选择的 classIndex
            var classTypeWindow = new ClassTypeWindow();
            if (classTypeWindow.ShowChoose() == true)
            {
                var selectedItem = classTypeWindow.SelectedItem;
                var newValue = selectedItem.TypeIndex;

                // 更新选定的单元格的值
                ((CutLabelModel)lblInfos.Items[row]).ClassIndex = newValue;
            }
        }

        private void CreateDataFolders(string dataFolderPath)
        {
            //创建资源存放文件夹
            // Base folder path
            //string baseFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            //// Data folder path
            //string dataFolderPath = Path.Combine(baseFolderPath, "data");

            // Subfolders paths
            string imagesFolderPath = Path.Combine(dataFolderPath, "images");
            string labelsFolderPath = Path.Combine(dataFolderPath, "labels");

            string s = "";
            if (Directory.Exists(imagesFolderPath))
            {
                s = "images";
            }
            if (Directory.Exists(labelsFolderPath))
            {
                if (!string.IsNullOrEmpty(s)) s += ",";
                s += "labels";
            }
            if (!string.IsNullOrWhiteSpace(s))
            {
                MessageBoxResult re = MessageBox.Show("已存在文件夹" + s + "是否继续？", "Tip", MessageBoxButton.OKCancel);
                if (re == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            string trainImagesFolderPath = Path.Combine(imagesFolderPath, "train");
            string valImagesFolderPath = Path.Combine(imagesFolderPath, "val");

            string trainLabelsFolderPath = Path.Combine(labelsFolderPath, "train");
            string valLabelsFolderPath = Path.Combine(labelsFolderPath, "val");

            // Create directories if they don't exist
            CreateDirectoryIfNotExists(dataFolderPath);
            CreateDirectoryIfNotExists(imagesFolderPath);
            CreateDirectoryIfNotExists(labelsFolderPath);
            CreateDirectoryIfNotExists(trainImagesFolderPath);
            CreateDirectoryIfNotExists(valImagesFolderPath);
            CreateDirectoryIfNotExists(trainLabelsFolderPath);
            CreateDirectoryIfNotExists(valLabelsFolderPath);
        }

        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void setDataFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "请选择data文件夹";
                dialog.ShowNewFolderButton = true;

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    var path = dialog.SelectedPath;
                    CreateDataFolders(path);
                }
            }
        }

        private void loadImage()
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            // Show OpenFileDialog
            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected image
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                LabelGrid.ImageSource = bitmap;
            }
        }

        private void SaveBitmapSourceAsJpeg(BitmapSource bitmapSource)
        {
            // Create SaveFileDialog
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Save the image
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string txt = "";
            foreach (var item in LabelGrid.viewModel.CutList)
            {
                txt += $"{item.ClassIndex} {item.XCenter} {item.YCenter} {item.XWeight} {item.YWeight}\r";
            }
            Debug.WriteLine(txt);
            string filePath = "d:\\output.txt";

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(txt);
                }
                MessageBox.Show("Text saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SaveBitmapSourceAsJpeg(LabelGrid.ImageSource);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            // 新建文件的逻辑
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件的逻辑
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // 退出应用程序的逻辑
            Application.Current.Shutdown();
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //OnControlSizeChanged();
        }
        private void gsLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }
        private void gsRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // 处理 GridSplitter 拖动事件
            // 您可以在这里添加任何逻辑来响应拖动事件
            // 例如，调整两个 TabControl 的大小或其他操作
            //OnControlSizeChanged();
        }


        void MyControl_MyCustomEvent(object sender, SolutionClickEventArgs e)
        {
            Debug.WriteLine($"got {e.SolutionItem.Name}");
            AddNewTabWithLabelGrid(e.SolutionPath, e.SolutionItem);
            
        }

        private void LabelGrid_ActiveLabelChanged(object sender, RoutedEventArgs e)
        {
            if (e is ActiveLabelChangedEventArgs args)
            {
                CutLabelModel cutLabel = args.CutLabel;
                // 处理事件，例如更新UI或处理业务逻辑
                SelectDataGridRow(cutLabel);
            }
        }

        private void LabelGrid_LabelDataChanged(object sender, RoutedEventArgs e)
        {
            if (e is LabelDataChangedEventArgs args)
            {
                //Debug.WriteLine("LabelGrid_LabelDataChanged:");

                //Debug.WriteLine("New List:");
                //foreach (var item in args.List)
                //{
                //    Debug.WriteLine(item.ToString());
                //}

                //Debug.WriteLine("Old List:");
                //foreach (var item in args.OldList)
                //{
                //    Debug.WriteLine(item.ToString());
                //}

                //Debug.WriteLine($"New List: {string.Join(", ", args.List)}");
                //Debug.WriteLine($"Old List: {string.Join(", ", args.OldList)}");

                //if (historyList.Count > 0)
                //{
                //    if (historyList[historyList.Count-1].RedoStack)
                //}


                TextFileTool.SaveFile(((LabelGrid)sender).LabelPath, args.List);
            }
        }

        private void AddNewTabWithLabelGrid(string solutionPath, SolutionItem solutionItem)
        {
            _txtFilePath = "";
            string tabname = solutionItem.Name;

            GetImageProperties(TextFileTool.CombinePaths(solutionPath, solutionItem.RelativePath));
            // 检查是否已经存在具有相同标题的 TabItem
            foreach (TabItem item in tbMain.Items)
            {
                if (item.Header.ToString() == tabname)
                {
                    // 如果标签页已经存在，则设置其为选中状态并返回
                    tbMain.SelectedItem = item;
                    return;
                }
            }
            // 创建新的 TabItem
            TabItem newTabItem = new TabItem();
            newTabItem.Header = tabname;
            newTabItem.Style = (Style)FindResource("CustomTabItem");

            if (tabname.EndsWith(".png") || tabname.EndsWith(".jpg"))
            {
                // 创建新的 LabelGrid 控件
                var newLabelGrid = new LabelImg.Views.UserControls.LabelGrid();
                newLabelGrid.ImagePath = TextFileTool.CombinePaths(solutionPath, solutionItem.RelativePath);
                newLabelGrid.LabelPath = TextFileTool.CombinePaths(solutionPath, solutionItem.RelativePath)
                    .Replace("\\images\\", "\\labels\\").Replace(".jpg", ".txt").Replace(".png", ".txt");

                if (!File.Exists(newLabelGrid.ImagePath))
                {
                    MessageBox.Show("图片不存在", "提示", MessageBoxButton.OK);
                    return;
                }
                //在添加事件前初始化图片上的label
                List<TxtDataRow> rows = TextFileTool.LoadFile(newLabelGrid.LabelPath).ToList();
                newLabelGrid.viewModel.CutList.Clear();
                foreach (TxtDataRow row in rows)
                {
                    CutLabelModel cut = new CutLabelModel();
                    cut.Name = $"LBL_{RandomStringGenerator.Generate(6)}";
                    cut.ClassIndex = row.IntValue;
                    cut.XCenter = row.DoubleValue1;
                    cut.YCenter = row.DoubleValue2;
                    cut.XWeight = row.DoubleValue3;
                    cut.YWeight = row.DoubleValue4;

                    newLabelGrid.viewModel.CutList.Add(cut);
                }
                // 读取文件到内存流中
                byte[] imageBytes = File.ReadAllBytes(newLabelGrid.ImagePath);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 确保图片完全加载到内存
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // 确保图片可以跨线程使用

                    // 将图片设置到Image控件
                    newLabelGrid.ImageSource = bitmap;
                }
                //BitmapImage bitmap = new BitmapImage(new Uri(newLabelGrid.ImagePath));
                //newLabelGrid.ImageSource = bitmap;

                newLabelGrid.ActiveLabelChanged += LabelGrid_ActiveLabelChanged;
                newLabelGrid.LabelDataChanged += LabelGrid_LabelDataChanged;


                // 将 LabelGrid 添加到 TabItem 的内容中
                newTabItem.Content = newLabelGrid;

                // 将 TabItem 添加到 CustomTabControl 中
                tbMain.Items.Add(newTabItem);
                tbMain.SelectedItem = newTabItem;
            }
            //else if (tabname.EndsWith(".txt"))
            //{
            //    try
            //    {
            //        _txtFilePath = TextFileTool.CombinePaths(solutionPath, solutionItem.RelativePath).Replace("/", "\\");
            //        _parsedData = TextFileTool.LoadFile(_txtFilePath);
            //        foreach (var dataRow in _parsedData)
            //        {
            //            dataRow.PropertyChanged += TxtFileDataRow_PropertyChanged;
            //        }
            //        var lblInfos = new DataGrid
            //        {
            //            AutoGenerateColumns = false,
            //            ItemsSource = _parsedData,
            //            Margin = new Thickness(10, 50, 10, 10)
            //        };
            //        // 定义列并设置表头名称
            //        lblInfos.Columns.Add(new DataGridTextColumn { Header = "类型", Binding = new System.Windows.Data.Binding("IntValue"), IsReadOnly = true });
            //        lblInfos.Columns.Add(new DataGridTextColumn { Header = "XCenter", Binding = new System.Windows.Data.Binding("DoubleValue1"), IsReadOnly = true });
            //        lblInfos.Columns.Add(new DataGridTextColumn { Header = "YCenter", Binding = new System.Windows.Data.Binding("DoubleValue2"), IsReadOnly = true });
            //        lblInfos.Columns.Add(new DataGridTextColumn { Header = "XWeight", Binding = new System.Windows.Data.Binding("DoubleValue3"), IsReadOnly = true });
            //        lblInfos.Columns.Add(new DataGridTextColumn { Header = "YWeight", Binding = new System.Windows.Data.Binding("DoubleValue4"), IsReadOnly = true });

            //        newTabItem.Content = lblInfos;
            //        // 将 TabItem 添加到 CustomTabControl 中
            //        tbMain.Items.Add(newTabItem);
            //        tbMain.SelectedItem = newTabItem;
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error parsing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }

            //}
        }

        public static readonly DependencyProperty DemoModelProperty = DependencyProperty.Register(
            "DemoModel", typeof(PropertyGridDemoModel), typeof(HandyControl.Controls.PropertyGrid), new PropertyMetadata(default(PropertyGridDemoModel)));

        public PropertyGridDemoModel DemoModel
        {
            get => (PropertyGridDemoModel)GetValue(DemoModelProperty);
            set => SetValue(DemoModelProperty, value);
        }
        private void GetImageProperties(string imagePath)
        {
            try
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath))
                {
                    // 获取图片宽度和高度
                    int width = image.Width;
                    int height = image.Height;

                    // 获取图片的水平和垂直分辨率
                    float horizontalResolution = image.HorizontalResolution;
                    float verticalResolution = image.VerticalResolution;

                    // 获取图片格式
                    ImageFormat format = image.RawFormat;

                    // 获取图片的其他属性
                    System.Drawing.Imaging.PropertyItem[] propertyItems = image.PropertyItems;

                    // 输出图片属性
                    Debug.WriteLine($"Width: {width}");
                    Debug.WriteLine($"Height: {height}");
                    Debug.WriteLine($"Horizontal Resolution: {horizontalResolution}");
                    Debug.WriteLine($"Vertical Resolution: {verticalResolution}");
                    Debug.WriteLine($"Format: {format}");
                    // 获取图片文件的大小
                    FileInfo fileInfo = new FileInfo(imagePath);
                    long fileSizeInBytes = fileInfo.Length;
                    double fileSizeInKB = fileSizeInBytes / 1024.0;
                    //                    tbProperty.Text = @$"
                    //路径：{imagePath}
                    //尺寸：{width}x{height}
                    //类型：{format}
                    //大小：{fileSizeInKB:F2} KB
                    //";

                    pPath.Text = imagePath;
                    pDimension.Text = @$"{width}x{height}";
                    pType.Text = format.ToString();
                    pSize.Text = @$"{fileSizeInKB:F2} KB";

                    DemoModel = new PropertyGridDemoModel
                    {
                        String = "TestString",
                        Enum = Gender.Female,
                        Boolean = true,
                        Integer = 98,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };


                    // 输出所有属性项
                    foreach (var propertyItem in propertyItems)
                    {
                        Debug.WriteLine($"Property ID: {propertyItem.Id}, Type: {propertyItem.Type}, Length: {propertyItem.Len}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void TxtFileDataRow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataRow = sender as TxtDataRow;
            if (dataRow != null)
            {
                TextFileTool.SaveFile(_txtFilePath, _parsedData);
            }
        }

        private void tbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                // 获取当前选中的 TabItem
                var selectedTabItem = (sender as TabControl)?.SelectedItem as TabItem;

                if (selectedTabItem != null)
                {
                    // 获取 TabItem 的内容
                    var content = selectedTabItem.Content;

                    // 判断内容是否为 LabelGrid 类型并进行相应处理
                    if (content is LabelGrid lg)
                    {
                        LabelGrid = lg;
                        if (!string.IsNullOrWhiteSpace(LabelGrid.LabelPath))
                        {
                            TextFileTool.EnsureFileExists(LabelGrid.LabelPath);
                        }
                        LabelGrid.viewModel.CutList.CollectionChanged += OnCollectionChanged;
                        lblInfos.ItemsSource = null;
                        lblInfos.ItemsSource = LabelGrid.viewModel.CutList;

                        Debug.WriteLine("tbMain_selection changed ");
                    }
                    else
                    {
                       // MessageBox.Show("Selected Tab does not contain a LabelGrid.");
                    }
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 当集合更改时执行的逻辑，例如：
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // 处理新增项
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // 处理移除项
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // 处理被替换项
                    break;
                case NotifyCollectionChangedAction.Move:
                    // 处理移动项
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // 处理清空集合
                    break;
            }

             
        }

        private void midRow3Things(UIElement item, string title)
        {
            var h = midGrid.RowDefinitions[2].ActualHeight;
            if (h <= 0)
            {
                midGrid.RowDefinitions[2].Height = new GridLength(150);
                midGrid.RowDefinitions[2].MinHeight = 100;
            }
            else
            {
                if (item.Visibility == Visibility.Visible)
                {
                    midGrid.RowDefinitions[2].Height = new GridLength(0);
                    midGrid.RowDefinitions[2].MinHeight = 0;
                }
            }
            foreach (UIElement child in midGridRow3.Children)
            {
                child.Visibility = Visibility.Collapsed;
            }

            item.Visibility = Visibility.Visible;
            lblMidRow3Title.Content = title;

        }

        private void MidBtnDingClick(object sender, RoutedEventArgs e)
        {
            midGrid.RowDefinitions[2].Height = new GridLength(0);
            midGrid.RowDefinitions[2].MinHeight = 0;
        }

        private void MidBtnDelClick(object sender, RoutedEventArgs e)
        {
            midGrid.RowDefinitions[2].Height = new GridLength(0);
            midGrid.RowDefinitions[2].MinHeight = 0;
        }

        private void LB_lblinfos_Click(object sender, RoutedEventArgs e)
        {
            midRow3Things(lblInfos, ((LabelButton)sender).Content.ToString());

        }

        private void LB_terminalBox_Click(object sender, RoutedEventArgs e)
        {
            midRow3Things(terminalTextBox, ((LabelButton)sender).Content.ToString());

        }

        private void LB_textOut_Click(object sender, RoutedEventArgs e)
        {
            midRow3Things(textOut, ((LabelButton)sender).Content.ToString());

        }
        


        private void Right1BtnDingClick(object sender, RoutedEventArgs e)
        {
            rightGrid.RowDefinitions[0].Height = new GridLength(0);
            rightGrid.RowDefinitions[1].Height = new GridLength(0);
            rightGrid.RowDefinitions[0].MinHeight = 0;

            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (h2 <= 0)
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].MinWidth = 0;
            }
        }

        private void Right2BtnDingClick(object sender, RoutedEventArgs e)
        {
            rightGrid.RowDefinitions[1].Height = new GridLength(0);
            rightGrid.RowDefinitions[2].Height = new GridLength(0);
            rightGrid.RowDefinitions[2].MinHeight = 0;
            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (h1 <= 0)
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].MinWidth = 0;
            }
        }

        private void Right1BtnDelClick(object sender, RoutedEventArgs e)
        {
            rightGrid.RowDefinitions[0].Height = new GridLength(0);
            rightGrid.RowDefinitions[1].Height = new GridLength(0);
            rightGrid.RowDefinitions[0].MinHeight = 0;
            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (h2 <= 0)
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].MinWidth = 0;
            }
        }

        private void Right2BtnDelClick(object sender, RoutedEventArgs e)
        {
            rightGrid.RowDefinitions[1].Height = new GridLength(0);
            rightGrid.RowDefinitions[2].Height = new GridLength(0);
            rightGrid.RowDefinitions[2].MinHeight = 0;
            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (h1 <= 0)
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].Width = new GridLength(0);
                grid2.ColumnDefinitions[4].MinWidth = 0;
            }
        }

        private void LB_lblsolution_Click(object sender, RoutedEventArgs e)
        {
            var w = grid2.ColumnDefinitions[4].ActualWidth;
            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (w > 0)
            {
                if (h2 > 0)
                {
                    if (h1 > 0)
                    {
                        rightGrid.RowDefinitions[0].Height = new GridLength(0);
                        rightGrid.RowDefinitions[0].MinHeight = 0;
                        rightGrid.RowDefinitions[1].Height = new GridLength(0);
                        if (h2 <= 0)
                        {
                            grid2.ColumnDefinitions[3].Width = new GridLength(0);
                            grid2.ColumnDefinitions[4].Width = new GridLength(0);
                        }
                        else
                        {
                            rightGrid.RowDefinitions[1].Height = new GridLength(0);
                        }
                    }
                    else
                    {
                        rightGrid.RowDefinitions[0].Height = new GridLength(300);
                        rightGrid.RowDefinitions[0].MinHeight = 150;
                        rightGrid.RowDefinitions[1].Height = new GridLength(4);
                    }
                }
                else
                {
                    if (h1 > 0)
                    {
                        grid2.ColumnDefinitions[3].Width = new GridLength(0);
                        grid2.ColumnDefinitions[4].Width = new GridLength(0);
                        grid2.ColumnDefinitions[4].MinWidth = 0;
                    }
                    else
                    {
                        grid2.ColumnDefinitions[3].Width = new GridLength(5);
                        grid2.ColumnDefinitions[4].Width = new GridLength(300);
                        rightGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                        rightGrid.RowDefinitions[0].MinHeight = 150;
                    }
                }
            }
            else
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(5);
                grid2.ColumnDefinitions[4].Width = new GridLength(300);

                rightGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                rightGrid.RowDefinitions[0].MinHeight = 150;

                rightGrid.RowDefinitions[1].Height = new GridLength(0);
                rightGrid.RowDefinitions[2].Height = new GridLength(0);
                rightGrid.RowDefinitions[2].MinHeight = 0;


            }
        }

        private void LB_lblProperties_Click(object sender, RoutedEventArgs e)
        {
            var w = grid2.ColumnDefinitions[4].ActualWidth;
            var h1 = rightGrid.RowDefinitions[0].ActualHeight;
            var h2 = rightGrid.RowDefinitions[2].ActualHeight;
            if (w > 0)
            {
                if (h1 > 0)
                {
                    if (h2 > 0)
                    {
                        rightGrid.RowDefinitions[2].Height = new GridLength(0);
                        rightGrid.RowDefinitions[2].MinHeight = 0;
                        rightGrid.RowDefinitions[1].Height = new GridLength(0);
                        if (h1 <= 0)
                        {
                            grid2.ColumnDefinitions[3].Width = new GridLength(0);
                            grid2.ColumnDefinitions[4].Width = new GridLength(0);
                        }
                        else
                        {
                            rightGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                            rightGrid.RowDefinitions[1].Height = new GridLength(0);
                        }
                    }
                    else
                    {
                        rightGrid.RowDefinitions[2].Height = new GridLength(300);
                        rightGrid.RowDefinitions[2].MinHeight = 150;
                        rightGrid.RowDefinitions[1].Height = new GridLength(4);
                    }
                }
                else
                {
                    if (h2 > 0)
                    {
                        grid2.ColumnDefinitions[3].Width = new GridLength(0);
                        grid2.ColumnDefinitions[4].Width = new GridLength(0);
                        grid2.ColumnDefinitions[4].MinWidth = 0;
                    }
                    else
                    {
                        grid2.ColumnDefinitions[3].Width = new GridLength(5);
                        grid2.ColumnDefinitions[4].Width = new GridLength(300);
                        rightGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                        rightGrid.RowDefinitions[2].MinHeight = 150;
                    }
                }
            }
            else
            {
                grid2.ColumnDefinitions[3].Width = new GridLength(5);
                grid2.ColumnDefinitions[4].Width = new GridLength(300);

                rightGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                rightGrid.RowDefinitions[2].MinHeight = 150;

                rightGrid.RowDefinitions[1].Height = new GridLength(0);
                rightGrid.RowDefinitions[0].Height = new GridLength(0);
                rightGrid.RowDefinitions[0].MinHeight = 0;


            }
        }


        // Theme
        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            //checkBox.SetResourceReference(BackgroundProperty, "AccentBrush");
            //Resources["PrimaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff5722"));
            //Resources["PrimaryBrush"] = (Brush)FindResource("BrushDanger");
            if (!(sender is Button button))
            {
                return;
            }
            string resourceStr = "pack://application:,,,/Resource/Style/Theme/BaseLight.xaml";
            if (button.Content.ToString() == "BaseDark")
            {
                resourceStr = "pack://application:,,,/Resource/Style/Theme/BaseDark.xaml";
            }
            UpdataResourceDictionary(resourceStr, 0);
        }

        // Updata ResourceDictionary
        private void UpdataResourceDictionary(string resourceStr, int pos)
        {
            if (pos < 0 || pos > 2)
            {
                return;
            }
            if (pos >= 0)
            {
                var resource = new ResourceDictionary { Source = new Uri(resourceStr) };
                Application.Current.Resources.MergedDictionaries.RemoveAt(pos);
                Application.Current.Resources.MergedDictionaries.Insert(pos, resource);
            }

        }
	}

	public class UndoRedoStack
    {
        public LabelGrid key { get; set; }
        public Stack<List<CutLabelModel>> UndoStack { get; } = new();
        public Stack<List<CutLabelModel>> RedoStack { get; } = new();
    }

}
