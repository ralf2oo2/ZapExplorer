using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class DirectoryItemMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return null;

            ZapArchive archive = (ZapArchive)values[0];
            if (values[1] != null)
            {
                return ((DirectoryItem)values[1]).Items;
            }
            return archive.Items;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
