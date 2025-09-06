using ByteSizeLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ZapExplorer.ApplicationLayer.Converters
{
    public class FileSizeNotationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ByteSize size = ByteSize.FromBytes((int)value);
            string notation = $"{size.Bytes} {ByteSize.ByteSymbol}";
            if (size.Bytes >= 1000)
            {
                notation = $"{Math.Round(size.KiloBytes, 1)} {ByteSize.KiloByteSymbol}";
            }
            if (size.Bytes >= 1000000)
            {
                notation = $"{Math.Round(size.MegaBytes, 1)} {ByteSize.MegaByteSymbol}";
            }
            if (size.Bytes >= 1000000000)
            {
                notation = $"{Math.Round(size.GigaBytes, 1)} {ByteSize.GigaByteSymbol}";
            }
            return notation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
