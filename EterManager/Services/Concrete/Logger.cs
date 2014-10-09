using EterManager.Models;
using EterManager.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EterManager.Services.Concrete
{
    class Logger : ILogger
    {
        #region ILocale Members

        private readonly ILocale _locale = ((App)Application.Current).GetInstance<ILocale>();

        #endregion

        #region Events

        public delegate void IssuesChangedEventHandler(object sender, IssueSeverity severity);
        public event IssuesChangedEventHandler IssuesChanged;

        #endregion

        #region Properties

        public List<Issue> Issues { get; set; }

        #endregion

        #region Constructors

        public Logger()
        {
            Issues = new List<Issue>();
        }

        #endregion

        #region ILogger Members

        public void Debug(string key, string context, params object[] data)
        {
            Issues.Add(
                new Issue(
                    IssueSeverity.Message,
                    String.Format(_locale.GetString(key), data),
                    context
                    ));
            IssuesChanged(this, IssueSeverity.Message);
        }

        public void Warning(string key, string context, params object[] data)
        {
            Issues.Add(
                new Issue(
                    IssueSeverity.Warning, 
                    String.Format(_locale.GetString(key), data),
                    context
                    ));
            IssuesChanged(this, IssueSeverity.Warning);
        }

        public void Error(string key, string context, params object[] data)
        {
            Issues.Add(
                new Issue(
                    IssueSeverity.Error,
                    String.Format(_locale.GetString(key), data),
                    context
                    ));
            IssuesChanged(this, IssueSeverity.Error);
        }

        public void Critical(params object[] data)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}