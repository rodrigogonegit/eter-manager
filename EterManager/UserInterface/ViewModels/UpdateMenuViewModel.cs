using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Services.Abstract;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    class UpdateMenuViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMenuViewModel"/> class.
        /// </summary>
        public UpdateMenuViewModel()
        {
            InitializeCommands();
            CanCheckUpdates = true;
            AppUpdater.NewVersionFound += (sender, args) =>
            {
                if (MessageBox.Show("Download latest version?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    IsUpdating = true;
                    AppUpdater.DownloadLatestVersion();
                    AppUpdater.DownloadProgressChanged += AppUpdaterOnDownloadProgressChanged;
                    AppUpdater.DownloadCompleted += AppUpdaterOnDownloadCompleted;
                }
            };
        }

        private void AppUpdaterOnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadProgress = 100;
            IsUpdating = false;
        }

        private void AppUpdaterOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = (int)(e.BytesReceived * 1.0 / e.TotalBytesToReceive * 100.0);
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            _checkUpdates = new RelayCommand(p => CheckUpdatesAction(), p => CanCheckUpdates);
        }

        #endregion

        private RelayCommand _checkUpdates;

        /// <summary>
        /// Action performed when CheckUpdates is called
        /// </summary>
        private void CheckUpdatesAction()
        {
            CanCheckUpdates = false;
            try
            {
                AppUpdater.CheckVersions();
                Changelog = AppUpdater.ToString();
            }
            catch (Exception)
            {
                UserInput.ShowMessage("Could not reach server! Please try again later");
            }
            
            CanCheckUpdates = true;
        }

        /// <summary>
        /// Gets or sets the check updates command.
        /// </summary>
        /// <value>
        /// The check updates command.
        /// </value>
        public ICommand CheckUpdatesCommand => _checkUpdates;

        private bool _canCheckUpdates;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can check updates.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can check updates; otherwise, <c>false</c>.
        /// </value>
        public bool CanCheckUpdates
        {
            get { return _canCheckUpdates; }
            set { SetProperty(ref _canCheckUpdates, value, "CanCheckUpdates"); }
        }

        private string _changeLog;

        /// <summary>
        /// Gets or sets the changelog.
        /// </summary>
        /// <value>
        /// The changelog.
        /// </value>
        public string Changelog
        {
            get { return _changeLog; }
            set { SetProperty(ref _changeLog, value, "Changelog"); }
        }

        private int _downloadProgress;

        /// <summary>
        /// Gets or sets the download progress.
        /// </summary>
        /// <value>
        /// The download progress.
        /// </value>
        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set { SetProperty(ref _downloadProgress, value, "DownloadProgress"); }
        }

        private bool _isUpdating;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is updating.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updating; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdating
        {
            get { return _isUpdating; }
            set { SetProperty(ref _isUpdating, value, "IsUpdating"); }
        }


    }
}
