using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class ExportVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is FileItem)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
