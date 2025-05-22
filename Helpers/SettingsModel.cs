using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelImg.Helpers
{
    internal class SettingsModel
    {
    }
    public class DatabaseConfig
    {
        public string DbType { get; set; }
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoggingConfig
    {
        public string LogFilePath { get; set; }
        public string LogLevel { get; set; }
    }

    public class AppSettings
    {
        public string UITheme { get; set; }
        public string DefaultLanguage { get; set; }
        public int AutoSaveInterval { get; set; }
    }

    public class AppConfig
    {
        public DatabaseConfig DatabaseConfig { get; set; }
        public LoggingConfig LoggingConfig { get; set; }
        public AppSettings AppSettings { get; set; }
    }

}
