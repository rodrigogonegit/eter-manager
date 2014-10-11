using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EterManager.Models;
using EterManager.Services.Concrete;

namespace EterManager.Services
{
    /// <summary>
    /// TODO: IMPLEMENT WEAK-LINKED APPENDERS
    /// </summary>
    interface ILogger
    {

        List<Issue> Issues { get; set; }

        event Logger.IssuesChangedEventHandler IssuesChanged;

        /// <summary>
        /// Creates debug level message
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <param name="data"></param>
        void Information(string key, string context, params object[] data);

        /// <summary>
        /// Creates waring level message
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <param name="data"></param>
        void Warning(string key, string context, params object[] data);

        /// <summary>
        /// Creates error level message
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <param name="data"></param>
        void Error(string key, string context, params object[] data);

        /// <summary>
        /// Creates critical error message
        /// </summary>
        /// <param name="data"></param>
        void Critical(params object[] data);
    }
}
