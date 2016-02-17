using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Exceptions.EterFiles
{
    class ErrorReadingIndexException : Exception
    {
        public string Reason { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="reason"></param>
        public ErrorReadingIndexException(string reason)
        {
            Reason = reason;
        }
    }
}
