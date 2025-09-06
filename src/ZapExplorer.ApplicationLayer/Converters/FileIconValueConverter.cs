using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Data.Converters;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class FileIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryItem)
            {
                return null;
            }
            else
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
