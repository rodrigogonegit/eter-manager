using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace EterManager.UserInterface.Converters
{
    class ByteArrayToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "" : BitConverter.ToString((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value.ToString().Replace(" ", "");
            hex = hex.Replace("-", "");
            return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
        }
    }
}
