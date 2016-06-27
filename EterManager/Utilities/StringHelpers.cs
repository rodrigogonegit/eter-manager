using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                str += "\\";
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

        /// <summary>
        /// Removes the extension from the filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string TrimExtension(string fileName)
        {
            return String.IsNullOrWhiteSpace(fileName) ? 
                String.Empty : Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
