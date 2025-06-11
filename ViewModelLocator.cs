using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using LabelImg.ViewModels;
using System.Windows;

namespace LabelImg
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => Ioc.Default.GetService<MainViewModel>();

        public LGViewModel LGViewModel => Ioc.Default.GetService<LGViewModel>();
    }

    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<MainViewModel>()
                    .AddSingleton<LGViewModel>()
                    .BuildServiceProvider());
        }
    }
}
