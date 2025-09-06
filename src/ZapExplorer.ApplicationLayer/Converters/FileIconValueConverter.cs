using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class FileIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryItem)
            {
                return LoadResourceAsBitmap("avares://ZapExplorer.ApplicationLayer/Assets/folder.png");
            }
            return LoadResourceAsBitmap("avares://ZapExplorer.ApplicationLayer/Assets/file.png");;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Bitmap LoadResourceAsBitmap(string resourceName)
        {
            var uri = new Uri(resourceName);
            return new Bitmap(AssetLoader.Open(uri));
        }
    }
}
