using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LabelImg.Models
{
    public  class TextFileTool
    {

        public static ObservableCollection<TxtDataRow> LoadFile(string filePath)
        {
            var parsedData = new ObservableCollection<TxtDataRow>();

            if (!File.Exists(filePath))
            {
                return parsedData;
            }
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var values = line.Split(' ');
                if (values.Length != 5) throw new FormatException("Each line must contain exactly 5 values.");

                int intValue = int.Parse(values[0]);
                double[] doubleValues = values.Skip(1).Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();

                ValidateValues(doubleValues);

                var dataRow = new TxtDataRow
                {
                    IntValue = intValue,
                    DoubleValue1 = doubleValues[0],
                    DoubleValue2 = doubleValues[1],
                    DoubleValue3 = doubleValues[2],
                    DoubleValue4 = doubleValues[3]
                };

                parsedData.Add(dataRow);
            }

            return parsedData;
        }
        public static string CombinePaths(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentException("Base path cannot be null or empty.", nameof(basePath));
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or empty.", nameof(relativePath));
            }

            // Replace both '\\' and '/' with the current platform's directory separator
            basePath = basePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            // Ensure the relative path starts with a directory separator
            if (!relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativePath = Path.DirectorySeparatorChar + relativePath;
            }

            string combinedPath = Path.Combine(basePath, relativePath.TrimStart(Path.DirectorySeparatorChar));
            return combinedPath;
        }


        public static void SaveFile(string filePath, ObservableCollection<TxtDataRow> data)
        {
            var lines = data.Select(row =>
                $"{row.IntValue} {row.DoubleValue1.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue2.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue3.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue4.ToString(CultureInfo.InvariantCulture)}").ToArray();
            File.WriteAllLines(filePath, lines);
        }

        public static void SaveFile(string filePath, List<TxtDataRow> data)
        {
            var lines = data.Select(row =>
                $"{row.IntValue} {row.DoubleValue1.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue2.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue3.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue4.ToString(CultureInfo.InvariantCulture)}").ToArray();
            File.WriteAllLines(filePath, lines);
        }

        public static void SaveFile(string filePath, List<CutLabelModel> data)
        {
            List<TxtDataRow> list = new List<TxtDataRow>();
            foreach (var cut in data)
            {
                TxtDataRow row = new TxtDataRow();
                row.IntValue = cut.ClassIndex;
                row.DoubleValue1 = double.Parse(cut.XCenterStr);
                row.DoubleValue2 = double.Parse(cut.YCenterStr);
                row.DoubleValue3 = double.Parse(cut.XWeightStr);
                row.DoubleValue4 = double.Parse(cut.YWeightStr);
                list.Add(row);
            }
            var lines = list.Select(row =>
                $"{row.IntValue} {row.DoubleValue1.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue2.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue3.ToString(CultureInfo.InvariantCulture)} {row.DoubleValue4.ToString(CultureInfo.InvariantCulture)}").ToArray();
            File.WriteAllLines(filePath, lines);
        }

        private static void ValidateValues(double[] values)
        {
            if (values[0] <= 0 || values[0] >= 1)
                throw new FormatException("The second value must be greater than 0 and less than 1.");
            if (values[1] <= 0 || values[1] >= 1)
                throw new FormatException("The third value must be greater than 0 and less than 1.");
            if (values[2] <= 0 || values[2] > 1)
                throw new FormatException("The fourth value must be greater than 0 and less than or equal to 1.");
            if (values[3] <= 0 || values[3] > 1)
                throw new FormatException("The fifth value must be greater than 0 and less than or equal to 1.");

            if (!(values[0] >= values[2] / 2 && (1 - values[0]) >= values[2] / 2))
                throw new FormatException("The second value must be greater than or equal to half of the fourth value and 1 minus the second value must be greater than or equal to half of the fourth value.");
            if (!(values[1] >= values[3] / 2 && (1 - values[1]) >= values[3] / 2))
                throw new FormatException("The third value must be greater than or equal to half of the fifth value and 1 minus the third value must be greater than or equal to half of the fifth value.");
        }

        public static void EnsureFileExists(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Directory created at: {directoryPath}");
            }

            if (!File.Exists(filePath))
            {
                // File does not exist, create it
                File.Create(filePath).Dispose();
                Console.WriteLine($"File created at: {filePath}");
            }
            else
            {
                // File exists
                Console.WriteLine($"File already exists at: {filePath}");
            }
        }
    } 
    public class TxtDataRow : INotifyPropertyChanged
    {
        private int _intValue;
        private double _doubleValue1;
        private double _doubleValue2;
        private double _doubleValue3;
        private double _doubleValue4;

        public int IntValue
        {
            get => _intValue;
            set
            {
                if (_intValue != value)
                {
                    _intValue = value;
                    OnPropertyChanged(nameof(IntValue));
                }
            }
        }

        public double DoubleValue1
        {
            get => _doubleValue1;
            set
            {
                if (_doubleValue1 != value)
                {
                    _doubleValue1 = value;
                    OnPropertyChanged(nameof(DoubleValue1));
                }
            }
        }

        public double DoubleValue2
        {
            get => _doubleValue2;
            set
            {
                if (_doubleValue2 != value)
                {
                    _doubleValue2 = value;
                    OnPropertyChanged(nameof(DoubleValue2));
                }
            }
        }

        public double DoubleValue3
        {
            get => _doubleValue3;
            set
            {
                if (_doubleValue3 != value)
                {
                    _doubleValue3 = value;
                    OnPropertyChanged(nameof(DoubleValue3));
                }
            }
        }

        public double DoubleValue4
        {
            get => _doubleValue4;
            set
            {
                if (_doubleValue4 != value)
                {
                    _doubleValue4 = value;
                    OnPropertyChanged(nameof(DoubleValue4));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}