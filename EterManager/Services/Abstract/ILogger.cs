using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Services
{
    /// <summary>
    /// TODO: IMPLEMENT WEAK-LINKED APPENDERS
    /// </summary>
    interface ILogger
    {
        /// <summary>
        /// Creates debug level message
        /// </summary>
        /// <param name="data"></param>
        void Debug(params object[] data);

        /// <summary>
        /// Creates waring level message
        /// </summary>
        /// <param name="data"></param>
        void Warning(params object[] data);

        /// <summary>
        /// Creates error level message
        /// </summary>
        /// <param name="data"></param>
        void Error(params object[] data);

        /// <summary>
        /// Creates critical error message
        /// </summary>
        /// <param name="data"></param>
        void Critical(params object[] data);
    }
}
