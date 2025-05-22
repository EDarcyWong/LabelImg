using LabelImg.Views.UserControls;
using LabelImg.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Text;

namespace LabelImg.Helpers
{
    public class SolutionTool
    {
        public SolutionInfo ReadYsnlFile(string ysnlPath)
        {
            if (!File.Exists(ysnlPath))
            {
                MessageBox.Show("配置文件不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                XDocument doc = XDocument.Load(ysnlPath);
                SolutionInfo solutionInfo = new SolutionInfo();

                var solutionElement = doc.Element("Solution");
                if (solutionElement != null)
                {
                    var propertyGroup = solutionElement.Element("PropertyGroup");
                    if (propertyGroup != null)
                    {
                        solutionInfo.SolutionName = propertyGroup.Element("SolutionName")?.Value;
                        solutionInfo.Path = propertyGroup.Element("Path")?.Value;
                    }

                    var projectElements = solutionElement.Elements("Project");
                    foreach (var project in projectElements)
                    {
                        ProjectInfo projectInfo = new ProjectInfo();

                        var projectPropertyGroup = project.Element("PropertyGroup");
                        if (projectPropertyGroup != null)
                        {
                            projectInfo.ProjectName = projectPropertyGroup.Element("ProjectName")?.Value;
                        }

                        var imagesGroup = project.Element("ImagesGroup");
                        if (imagesGroup != null)
                        {
                            var trainGroup = imagesGroup.Element("TrainGroup");
                            if (trainGroup != null)
                            {
                                var images = trainGroup.Elements("Images");
                                foreach (var image in images)
                                {
                                    string filePath = image.Attribute("Include")?.Value;
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        projectInfo.TrainImages.Add(Path.GetFileName(filePath));
                                    }
                                }
                            }

                            var valGroup = imagesGroup.Element("ValGroup");
                            if (valGroup != null)
                            {
                                var images = valGroup.Elements("Images");
                                foreach (var image in images)
                                {
                                    string filePath = image.Attribute("Include")?.Value;
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        projectInfo.ValImages.Add(Path.GetFileName(filePath));
                                    }
                                }
                            }
                        }

                        var labelsGroup = project.Element("LabelsGroup");
                        if (labelsGroup != null)
                        {
                            var trainGroup = labelsGroup.Element("TrainGroup");
                            if (trainGroup != null)
                            {
                                var labels = trainGroup.Elements("Labels");
                                foreach (var label in labels)
                                {
                                    string filePath = label.Attribute("Include")?.Value;
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        projectInfo.TrainLabels.Add(Path.GetFileName(filePath));
                                    }
                                }
                            }

                            var valGroup = labelsGroup.Element("ValGroup");
                            if (valGroup != null)
                            {
                                var labels = valGroup.Elements("Labels");
                                foreach (var label in labels)
                                {
                                    string filePath = label.Attribute("Include")?.Value;
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        projectInfo.ValLabels.Add(Path.GetFileName(filePath));
                                    }
                                }
                            }
                        }

                        solutionInfo.Projects.Add(projectInfo);
                    }
                }

                return solutionInfo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取配置文件时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        public static void GetRecordItems(ObservableCollection<SolutionStartItem> _solutionItems)
        {

            const string filePath = "solutions.txt";

            if (!File.Exists(filePath))
            {
                // 创建新的空文件
                File.Create(filePath).Close();
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                _solutionItems.Add(new SolutionStartItem
                {
                    SolutionName = parts[0],
                    SolutionPath = parts[1],
                    UpdateTime = DateTime.Parse(parts[2])
                });
            }
        }

        public static void AddRecordItem(string solutionName, string solutionPath, DateTime updateTime)
        {
            ObservableCollection<SolutionStartItem> items = new ObservableCollection<SolutionStartItem>();
            GetRecordItems(items);
            var itemsToRemove = items.Where(m => m.SolutionName.Equals(solutionName)).ToList();
            foreach (var item in itemsToRemove)
            {
                items.Remove(item);
            }

            const string filePath = "solutions.txt";
            StringBuilder builder = new StringBuilder();
            foreach (var item in items)
            {
                builder.Append($"{item.SolutionName},{item.SolutionPath},{item.UpdateTime}");
            }
            builder.Append($"{solutionName},{solutionPath},{updateTime}");
             
            File.WriteAllText(filePath, builder.ToString());

        }
    }

    public class SolutionInfo
    {
        public string SolutionName { get; set; }
        public string Path { get; set; }
        public List<ProjectInfo> Projects { get; set; }

        public SolutionInfo()
        {
            Projects = new List<ProjectInfo>();
        }
    }

    public class ProjectInfo
    {
        public string ProjectName { get; set; }
        public List<string> TrainImages { get; set; }
        public List<string> ValImages { get; set; }
        public List<string> TrainLabels { get; set; }
        public List<string> ValLabels { get; set; }

        public ProjectInfo()
        {
            TrainImages = new List<string>();
            ValImages = new List<string>();
            TrainLabels = new List<string>();
            ValLabels = new List<string>();
        }
    }

}
