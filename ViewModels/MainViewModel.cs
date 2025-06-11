using CommunityToolkit.Mvvm.ComponentModel;
using LabelImg.Models;
using System.Collections.ObjectModel;

namespace LabelImg.ViewModels
{
    //public partial class SolutionItem : ObservableObject
    //{
    //    [ObservableProperty]
    //    private string name;
    //    [ObservableProperty]
    //    private string iconPath;
    //    [ObservableProperty]
    //    private bool isExpanded;
    //    [ObservableProperty]
    //    private bool isEnabled = true;


    //    public ObservableCollection<SolutionItem> Children { get; set; } = new ObservableCollection<SolutionItem>();
    //}

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CutLabelModel> cutList;

        //[ObservableProperty]
        //private ObservableCollection<SolutionItem> solutionItems;
        

        public MainViewModel()
        {
            CutList = new ObservableCollection<CutLabelModel>
            {
            };

            // Example data for demonstration
            //SolutionItems = new ObservableCollection<SolutionItem>
            //{
            //    new SolutionItem
            //    {
            //        Name = "Solution 'MySolution'",
            //        IconPath = "Images/solution1.png",
            //        IsExpanded = true, // Set IsExpanded to true
            //        IsEnabled = false,
            //        Children = new ObservableCollection<SolutionItem>
            //        {
            //            new SolutionItem
            //            {
            //                Name = "Project 'MyProject'",
            //                IconPath = "Images/folder4.png",
            //                IsEnabled = true,
            //                Children = new ObservableCollection<SolutionItem>
            //                {
            //                    new SolutionItem { Name = "Properties", IconPath = "Images/pic3.png" },
            //                    new SolutionItem { Name = "References", IconPath = "Images/pic3.png" },
            //                    new SolutionItem { Name = "App.xaml", IconPath = "Images/pic3.png" },
            //                    new SolutionItem { Name = "MainWindow.xaml", IconPath = "Images/pic3.png" },
            //                    new SolutionItem { Name = "MainWindow.xaml.cs", IconPath = "Images/pic3.png" }
            //                }
            //            }
            //        }
            //    },
            //    new SolutionItem { Name = "Properties222", IconPath = "Images/pic3.png" },
            //};
        }
    }

}