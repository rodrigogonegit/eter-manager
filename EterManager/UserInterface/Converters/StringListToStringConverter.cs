using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EterManager.UserInterface.Converters
{
    class StringListToStringConverter : IValueConverter
    {
        /// <summary>
        /// List<string> to string 
        /// /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string tempString = "";

            if (parameter.ToString() == "NEW_LINE")
            {
                foreach (string line in (value as IList<string>))
                    tempString += line + Environment.NewLine;
            }
            else if (parameter.ToString() == "COMMA_AND_SPACE")
            {
                foreach (string line in (value as IList<string>))
                {
                    if (line.Trim() != String.Empty)
                        tempString += line + ", ";
                }
            }

            if (tempString.Trim() == "," || tempString.Trim() == "")
                return String.Empty;

            return tempString;

        }

        /// <summary>
        /// String to List<string>
        /// /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string[] tokens = null;

            if (parameter.ToString() == "NEW_LINE")
                tokens = System.Text.RegularExpressions.Regex.Split(value.ToString(), " ");
            else if (parameter.ToString() == "COMMA_AND_SPACE")
                tokens = System.Text.RegularExpressions.Regex.Split(value.ToString(), ",");

            var list = tokens.ToList();

            for (int i = 0; i < list.Count; i++)
                list[i] = list[i].Trim();

            return list;
        }
    }
}
