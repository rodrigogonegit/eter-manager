using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EterManager.UserInterface.Converters
{
    /// <summary>
    /// Converts a long value to a size string (1024 == 1mb)
    /// </summary>
    class LongToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            var number = System.Convert.ToDouble(value);

            var numberToTest = number;
            var numberToReturn = number;

            string unitStr = "bytes";

            var tttt = Math.Abs(number/1024.0/1024.0/1024.0);

            if ((numberToTest = Math.Abs(number / 1024.0)) > 0.5)
            {
                numberToReturn = numberToTest;
                unitStr = "kbs";
                if ((numberToTest = Math.Abs(number / 1024.0 / 1024.0)) > 0.5)
                {
                    numberToReturn = numberToTest;
                    unitStr = "mbs";
                    if ((numberToTest = Math.Abs(number / 1024.0 / 1024.0 / 1024.0)) > 0.5)
                    {
                        numberToReturn = numberToTest;
                        unitStr = "gbs";
                    }
                }
            }

            return String.Format("{0} {1}", Math.Round(numberToReturn, 3), unitStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
