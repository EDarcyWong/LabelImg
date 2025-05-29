using LabelImg.Helpers;
using LabelImg.Views.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace LabelImg.Views
{
    /// <summary>
    /// AddSolutionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddSolutionWindow : Window
    {
        private List<string> pathHistory = new List<string>();

        public AddSolutionWindow()
        {
            InitializeComponent();
            SolutionOptionComboBox.SelectedIndex = 0; // 默认选择“创建新解决方案”
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                LocationComboBox.Text = selectedPath;

                if (!pathHistory.Contains(selectedPath))
                {
                    pathHistory.Add(selectedPath);
                    LocationComboBox.Items.Add(selectedPath);
                }

                UpdateInfoLabel();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string projectName = ProjectNameTextBox.Text;
            string location = LocationComboBox.Text;

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(location))
            {
                MessageBox.Show("请填写项目名称并选择位置。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string projectPath = Path.Combine(location, projectName);
            string nestedProjectPath = Path.Combine(projectPath, "dataset");
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }
            else
            {
                if (Directory.GetFileSystemEntries(projectPath).Length > 0)
                {
                    MessageBox.Show("项目文件夹不为空，请选择其他名称。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            // 创建嵌套的 projectName 文件夹
            Directory.CreateDirectory(nestedProjectPath);

            // 在嵌套的 projectName 文件夹下创建 images 和 labels 文件夹及其子文件夹
            Directory.CreateDirectory(Path.Combine(nestedProjectPath, "images", "train"));
            Directory.CreateDirectory(Path.Combine(nestedProjectPath, "images", "val"));
            Directory.CreateDirectory(Path.Combine(nestedProjectPath, "labels", "train"));
            Directory.CreateDirectory(Path.Combine(nestedProjectPath, "labels", "val"));

            // 创建 .ysnl 文件并写入内容
            string ysnlFilePath = Path.Combine(projectPath, projectName + ".ysln");
            string ysnlContent = $@"
<Solution>
  <PropertyGroup>
    <SolutionName>{projectName}</SolutionName>
    <Path>{projectPath}</Path>
  </PropertyGroup>
  <Project>
    <ImagesGroup>
      <TrainGroup>
      </TrainGroup> 
      <ValGroup>
      </ValGroup> 
    </ImagesGroup>
    <LabelsGroup>
      <TrainGroup>
      </TrainGroup> 
      <ValGroup>
      </ValGroup> 
    </LabelsGroup>
  </Project>
  <Project>
    <PropertyGroup>
      <ProjectName>python</ProjectName>
    </PropertyGroup>
  </Project>
</Solution>";

            File.WriteAllText(ysnlFilePath, ysnlContent.Trim());

            SolutionTool.AddRecordItem(projectName, ysnlFilePath, DateTime.Now);

            // 打开 MainWindow
            MainWindow mainWindow = new MainWindow(ysnlFilePath);
            mainWindow.Show();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationComboBox.SelectedItem != null)
            {
                UpdateInfoLabel();
            }
        }

        private void UpdateInfoLabel()
        {
            InfoLabel.Content = $"项目将会在 {LocationComboBox.Text}\\{ProjectNameTextBox.Text} 中创建。";
        }
    }
}