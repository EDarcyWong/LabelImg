using LabelImg.Helpers;
using LabelImg.Views.UserControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LabelImg.Views
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        private ObservableCollection<SolutionStartItem> _solutionItems;
        private ICollectionView _collectionView;

        public StartWindow()
        {
            InitializeComponent();
            LoadSolutions();
            _collectionView = CollectionViewSource.GetDefaultView(_solutionItems);
            SolutionListBox.ItemsSource = _collectionView;
        }

        private void LoadSolutions()
        {
            _solutionItems = new ObservableCollection<SolutionStartItem>();
            SolutionTool.GetRecordItems(_solutionItems);
            //const string filePath = "solutions.txt";

            //if (!File.Exists(filePath))
            //{
            //    // 创建新的空文件
            //    File.Create(filePath).Close();
            //}

            //var lines = File.ReadAllLines(filePath);
            //foreach (var line in lines)
            //{
            //    var parts = line.Split(',');
            //    _solutionItems.Add(new SolutionStartItem
            //    {
            //        SolutionName = parts[0],
            //        SolutionPath = parts[1],
            //        UpdateTime = DateTime.Parse(parts[2])
            //    });
            //}
        }

        private void FilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _collectionView.Filter = item =>
            {
                if (item is SolutionStartItem solutionItem)
                {
                    return solutionItem.SolutionName.Contains(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
            _collectionView.Refresh();
        }

        private void SolutionListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SolutionListBox.SelectedItem is SolutionStartItem selectedItem)
            {
                var mainWindow = new MainWindow(selectedItem.SolutionPath);
                mainWindow.Show();
                this.Close();
            }
        }

        private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var addSolutionWindow = new AddSolutionWindow();
            addSolutionWindow.Show();
            this.Close();
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "ysln files (*.ysln)|*.ysln";

            // Show OpenFileDialog
            if (openFileDialog.ShowDialog() == true)
            {
                // 打开 MainWindow
                MainWindow mainWindow = new MainWindow(openFileDialog.FileName);
                mainWindow.Show();
                this.Close();
            }

        }
    }

    public class SolutionStartItem
    {
        public string SolutionName { get; set; }
        public string SolutionPath { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
