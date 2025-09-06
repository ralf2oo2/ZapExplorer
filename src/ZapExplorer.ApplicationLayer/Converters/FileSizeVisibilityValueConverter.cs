using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class FileSizeVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DirectoryItem)
                return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
