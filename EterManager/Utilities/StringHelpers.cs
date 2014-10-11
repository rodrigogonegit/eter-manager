using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Utilities
{
    class StringHelpers
    {
        /// <summary>
        /// Adds forward slash if needed
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddSlashToEnd(string str)
        {
            if (String.IsNullOrWhiteSpace(str)) return String.Empty;
            if (str[str.Length - 1] != '\\' && str[str.Length - 1] !='/')
            {
                str += "/";
            }
            return str;
        }

        /// <summary>
        /// Adds final point if none is found
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddExtensionPoint(string str)
        {
            if (String.IsNullOrWhiteSpace(str)) return String.Empty;
            if (str[0] != '.')
            {
                str = "." + str;
            }
            return str;
        }
    }
}
