using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Models;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    class IssuesListVm : ViewModelBase
    {
        #region Fields

        // Commands
        private readonly RelayCommand _clearIssueList;
        private readonly RelayCommand _copyIssue;

        // Fields
        private bool _isShowErrors;
        private bool _isShowWarnings;
        private bool _isShowMessages;

        private int _warningCount;
        private int _errorCount;
        private int _messageCount;

        private Issue _selectedIssue;

        private SynchronizationContext _uiContext = SynchronizationContext.Current;


        #endregion

        #region Constructors

        /// <summary>
        /// New instance of class
        /// </summary>
        public IssuesListVm()
        {
            if (Process.GetCurrentProcess().IsVisualStudioDesigner())
                return;

            IssuesList = CollectionViewSource.GetDefaultView(WindowLog.Issues);
            IssuesList.Filter = FilterCollection;
            WindowLog.IssuesChanged += IssuesChanged;

            IsShowErrors = true;
            IsShowMessages = true;
            IsShowWarnings = true;

            #region Command Instantiation

            _clearIssueList = new RelayCommand(p => ClearIssueListAction(), p => true);
            _copyIssue = new RelayCommand(CopyIssueAction, p => SelectedIssue != null);

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
            _uiContext.Post(o => IssuesList.Refresh(), null);
            
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
            WindowLog.Issues.Clear();

            // Reset counters
            ErrorCount = 0;
            WarningCount = 0;
            MessageCount = 0;

            // Refresh Collection
            IssuesList.Refresh();
        }

        /// <summary>
        /// Processes all copy issue commands
        /// </summary>
        /// <param name="param"></param>
        private void CopyIssueAction(object param)
        {
            if (param == null)
                throw new NullReferenceException("param");

            switch (param.ToString())
            {
                case "ALL":
                    Clipboard.SetText(String.Format("Context: {0} \n Description: {1} \n Severity: {2}", SelectedIssue.Context, SelectedIssue.Description, SelectedIssue.Severity));
                    break;
                case "DESCRIPTION":
                    Clipboard.SetText(SelectedIssue.Description);
                    break;
                case "CONTEXT":
                    Clipboard.SetText(SelectedIssue.Context);
                    break;
            }
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        public ICommand ClearIssueListCommand
        {
            get { return _clearIssueList; }
        }

        public ICommand CopyIssueCommand
        {
            get { return _copyIssue; }
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

        public Issue SelectedIssue
        {
            get { return _selectedIssue; }
            set { SetProperty(ref _selectedIssue, value, "SelectedIssue"); }
        }
        

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
