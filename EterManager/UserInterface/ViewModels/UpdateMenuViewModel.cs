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
            CanCheckUpdates = true;
            Changelog = AppUpdater.LatestVersion != null ? AppUpdater.ToString() : "";
            LastCheckDate = Properties.Settings.Default.LastVersionCheck;
            UpdateModeIndex = Properties.Settings.Default.UpdateMode;
            CheckPeriodIndex = Properties.Settings.Default.AutomaticCheckPeriod;

            // Initializes everything related to this VM
            Initialize();

            // Initialize all commands
            InitializeCommands();

            // Initialize update service
            InitializeUpdateService();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            if (AppUpdater.IsUpdatePackageAvailable())
                ActionButtonText = "Install update!";
            else if (AppUpdater.IsUpdateAvailable)
                ActionButtonText = "Update!";
            else
                ActionButtonText = "Check now!";
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            _checkUpdates = new RelayCommand(p => CheckUpdatesAction(), p => CanCheckUpdates);
        }

        /// <summary>
        /// Initializes all related to the Update service
        /// </summary>
        private void InitializeUpdateService()
        {
            AppUpdater.DownloadProgressChanged += AppUpdaterOnDownloadProgressChanged;
            AppUpdater.DownloadCompleted += AppUpdaterOnDownloadCompleted;

            // Handle New version found
            AppUpdater.CheckVersionsCompleted += async (sender, args) =>
            {
                if (args.AskToDownloadHandled ||
                    (!args.AskToDownloadHandled && args.TargetSubscriberType != typeof(UpdateMenuViewModel)))
                    return;

                // TODO: refactor this, it's fucking messy
                if (args.HasNewVersionAvailable)
                {
                    // 0 = Ask to download and install
                    // 1 = Ask to install
                    // 2 = Auto download and install

                    if (UpdateModeIndex == 0)
                    {
                        // Independt ifs since the user might refuse to download
                        if (!AppUpdater.IsUpdatePackageAvailable() &&
                            MessageBox.Show("There's a new version available. Would you like to download it?",
                                "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes)
                        {
                            IsUpdating = true;
                            await AppUpdater.DownloadLatestVersion();
                        }
                        if (
                            MessageBox.Show("Would you like to install the update?", "Update available",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes)
                        {
                            AppUpdater.InstallLatestVersion();
                        }
                    }
                    else // No need to check since the possible values are 0 to 2
                    {
                        IsUpdating = true;
                        await AppUpdater.DownloadLatestVersion();

                        if (UpdateModeIndex == 1)
                        {
                            if (
                                MessageBox.Show("Would you like to install the update?", "Update available",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                                MessageBoxResult.Yes)
                            {
                                AppUpdater.InstallLatestVersion();
                            }
                        }
                        else
                        {
                            await AppUpdater.DownloadLatestVersion();
                            AutoClosingMessageBox.Show("The update will be installed in 3 seconds...",
                                "Closing in 3 seconds!", 3000);
                            AppUpdater.InstallLatestVersion();
                        }
                    }
                }
                else
                    MessageBox.Show("The latest version is already installed!", "No update available",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                IsUpdating = false;
                args.AskToDownloadHandled = true;
            };
        }

        #endregion

        #region Update Service Events

        /// <summary>
        /// Called on DownloadCompleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppUpdaterOnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ActionButtonText = "Install!";
        }

        /// <summary>
        /// Called on DownloadProgressChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppUpdaterOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ActionButtonText = $"Updating [{(int) (e.BytesReceived*1.0/e.TotalBytesToReceive*100.0)}%]";
        }

        #endregion

        #region Commands

        #region RelayCommand objects  

        /// <summary>
        /// Handles the command functionality for CheckUpdates
        /// </summary>
        private RelayCommand _checkUpdates;

        #endregion

        #region Command Actions

        /// <summary>
        /// Action performed when CheckUpdates is called
        /// </summary>
        private async void CheckUpdatesAction()
        {
            CanCheckUpdates = false;

            try
            {
                //if (AppUpdater.IsUpdatePackageAvailable())
                //    AppUpdater.InstallLatestVersion();
                //else if (AppUpdater.IsUpdateAvailable)
                //    await AppUpdater.DownloadLatestVersion();
                //else
                await AppUpdater.CheckVersions(typeof(UpdateMenuViewModel));

                OnUpdateView?.Invoke(this);

                Changelog = AppUpdater.ToString();
            }
            catch (WebException)
            {
                System.Windows.Forms.MessageBox.Show("Could not reach server! Please try again later",
                    "Could not reach server", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }

            CanCheckUpdates = true;
        }

        #endregion

        #region ICommand

        /// <summary>
        /// Gets or sets the check updates command.
        /// </summary>
        /// <value>
        /// The check updates command.
        /// </value>
        public ICommand CheckUpdatesCommand => _checkUpdates;

        #endregion

        #endregion

        #region Properties

        #region Presentation
    
        private string _actionButtonText;

        /// <summary>
        /// Gets or sets the action button text.
        /// </summary>
        /// <value>
        /// The action button text.
        /// </value>
        public string ActionButtonText
        {
            get { return _actionButtonText; }
            set { SetProperty(ref _actionButtonText, value, "ActionButtonText"); }
        }

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

        private int _checkPeriod;

        /// <summary>
        /// Automatic check period
        /// </summary>
        public int CheckPeriodIndex
        {
            get { return _checkPeriod; }
            set
            {
                SetProperty(ref _checkPeriod, value, "CheckPeriodIndex");

                Properties.Settings.Default.AutomaticCheckPeriod = value;
                Properties.Settings.Default.Save();
            }
        }

        private DateTime _lastCheckDate;

        /// <summary>
        /// Last time an update was checked
        /// </summary>
        public DateTime LastCheckDate
        {
            get { return _lastCheckDate; }
            set
            {
                SetProperty(ref _lastCheckDate, value, "LastCheckDate");
                Properties.Settings.Default.LastVersionCheck = value;
                Properties.Settings.Default.Save();
            }
        }

        private int _updateModeIndex;

        /// <summary>
        /// Update mode index
        /// </summary>
        public int UpdateModeIndex
        {
            get { return _updateModeIndex; }
            set
            {
                SetProperty(ref _updateModeIndex, value, "UpdateModeIndex");
                Properties.Settings.Default.UpdateMode = value;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #endregion

        #region Update View

        /// <summary>
        /// Forces UI to refresh
        /// </summary>
        /// <param name="sender">The sender.</param>
        public delegate void UpdateViewDelegate(object sender);

        /// <summary>
        /// Occurs when [on update view].
        /// </summary>
        public event UpdateViewDelegate OnUpdateView;

        #endregion
    }
}