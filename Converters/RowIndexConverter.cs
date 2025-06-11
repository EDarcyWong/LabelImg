using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace LabelImg.Converters
{
    public class RowIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            if (row != null)
            {
                return row.GetIndex() + 1; // 加一以显示人类可读的行号，而不是索引
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
