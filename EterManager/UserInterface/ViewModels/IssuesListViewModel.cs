using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Models;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    class IssuesListViewModel : ViewModelBase
    {
        #region Fields

        // Commands
        private RelayCommand _clearIssueList;

        // Fields
        private bool _isShowErrors;
        private bool _isShowWarnings;
        private bool _isShowMessages;

        private int _warningCount;
        private int _errorCount;
        private int _messageCount;

        #endregion

        #region Constructors

        /// <summary>
        /// New instance of class
        /// </summary>
        public IssuesListViewModel()
        {
            IssuesList = CollectionViewSource.GetDefaultView(Logger.Issues);
            IssuesList.Filter = FilterCollection;
            Logger.IssuesChanged += IssuesChanged;

            IsShowErrors = true;
            IsShowMessages = true;
            IsShowWarnings = true;

            #region Commands

            _clearIssueList = new RelayCommand(p => ClearIssueListAction(), p => true);

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method called on each item to filter collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool FilterCollection(object item)
        {
            var issue = item as Issue;

            return issue.Severity == IssueSeverity.Error && IsShowErrors ||
                   issue.Severity == IssueSeverity.Warning && IsShowWarnings ||
                   issue.Severity == IssueSeverity.Message && IsShowMessages;
        }

        /// <summary>
        /// Called on IssuesChanged event fired from LoggerService
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="severity"></param>
        private void IssuesChanged(object sender, IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Error:
                    ErrorCount++;
                    break;
                case IssueSeverity.Warning:
                    WarningCount++;
                    break;
                case IssueSeverity.Message:
                    MessageCount++;
                    break;
            }
            IssuesList.Refresh();
        }

        #endregion

        #region Commands

        #region Command Actions

        /// <summary>
        /// Called to clear out all issues
        /// </summary>
        private void ClearIssueListAction()
        {
            // Clear list
            Logger.Issues.Clear();

            // Reset counters
            ErrorCount = 0;
            WarningCount = 0;
            MessageCount = 0;

            // Refresh Collection
            IssuesList.Refresh();
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        public ICommand ClearIssueListCommand
        {
            get { return _clearIssueList; }
        }

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion

        #region Presentation Members

        public ICollectionView IssuesList { get; private set; }

        public bool IsShowErrors
        {

            get { return _isShowErrors; }
            set
            {
                SetProperty(ref _isShowErrors, value, "IsShowErrors");
                IssuesList.Refresh();
            }
        }

        public bool IsShowWarnings
        {
            get { return _isShowWarnings; }
            set
            {
                SetProperty(ref _isShowWarnings, value, "IsShowWarnings"); 
                IssuesList.Refresh();
            }
        }

        public bool IsShowMessages
        {
            get { return _isShowMessages; }
            set
            {
                SetProperty(ref _isShowMessages, value, "IsShowMessages");
                IssuesList.Refresh();
            }
        }

        public int ErrorCount
        {
            get { return _errorCount; }
            set { SetProperty(ref _errorCount, value, "ErrorCount"); }
        }

        public int WarningCount
        {
            get { return _warningCount; }
            set { SetProperty(ref _warningCount, value, "WarningCount"); }
        }

        public int MessageCount
        {
            get { return _messageCount; }
            set { SetProperty(ref _messageCount, value, "MessageCount"); }
        }
        

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
