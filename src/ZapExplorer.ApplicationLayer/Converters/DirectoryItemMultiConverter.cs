using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class DirectoryItemMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] == null || values[0] is not ZapArchive)
                return null;

            ZapArchive archive = (ZapArchive)values[0];
            if (values[1] != null && values[1] is DirectoryItem)
            {
                return ((DirectoryItem)values[1]).Items;
            }
            return archive.Items;
        }
    }
}
