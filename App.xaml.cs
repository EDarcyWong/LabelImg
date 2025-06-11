using LabelImg.Helpers;
using LabelImg.ViewModels;
using LabelImg.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace LabelImg
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public AppConfig Config { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadConfig();
            // 现在你可以使用 Config 对象中的配置了

            StartWindow startWindow = new StartWindow();
            startWindow.Show();
        }

        private void LoadConfig()
        {
            string configFilePath = "appsettings.json";
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                Config = JsonSerializer.Deserialize<AppConfig>(json);
            }
            else
            {
                MessageBox.Show("配置文件未找到", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }

}
