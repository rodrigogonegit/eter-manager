using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using EterManager.Base;
using EterManager.Models;
using EterManager.UserInterface.Views;
using EterManager.Utilities;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    public class MainWindowVm : ViewModelBase, IHandle<ClientProfileVm>
    {
        #region Fields

        /// <summary>
        /// OpenWindowCommand RelayCommand obj
        /// </summary>
        private RelayCommand _openWindow;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates new instance of MainVM
        /// </summary>
        public MainWindowVm()
        {
            EventAggregator.Subscribe(this);

            // Internal initializer
            Initialize();

            // Command initializer
            InitializeCommands();

            // Initialize updating service
            InitializeVersionService();

            // Check for new versions
            try
            {
                AppUpdater.CheckVersions();
            }
            catch (Exception ex)
            {
                WindowLog.Error("SERVER_DOWN", "App");
            }
        }

        /// <summary>
        /// Internal initializer
        /// </summary>
        private void Initialize()
        {
            Properties.Settings.Default.Upgrade();
            Instance = this;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            CurrentAppVersion = String.Format("Version: {0} GunnerMBT ©", fvi.FileVersion);

            CanChangeProfile = true;

            // Default settings
            var value = Properties.Settings.Default.MaxSimFiles;
            ConstantsBase.MaxSimFiles = (value <= 0) ? 3 : value;

            // Get profile list
            var profileModelList = ClientProfile.GetAllProfiles();

            // Update view
            ProfileList =
                    new ObservableImmutableList<ClientProfileVm>(
                        profileModelList.Select(x => new ClientProfileVm(x)));

            // Load default profile
            var profileName = Properties.Settings.Default.DefaultProfile;

            if (!String.IsNullOrWhiteSpace(profileName))
            {
                //SelectedWorkingProfile = new ClientProfileVm(ClientProfile.GetProfileByPredicate(p => String.Equals(p.Name, profileName, StringComparison.CurrentCultureIgnoreCase)));
                SelectedWorkingProfile = ProfileList.First(x => x.Name == profileName);

                // To make sure every other class is udpated
                Handle(SelectedWorkingProfile);
            }
        }

        private void InitializeVersionService()
        {
            AppUpdater.NewVersionFound += (sender, args) =>
            {
                if (MessageBox.Show("Download latest version?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    AppUpdater.DownloadLatestVersion();
                    AppUpdater.DownloadCompleted += AppUpdaterOnDownloadCompleted;
                }
            };

            var inst = ViewManager.ShowWindow<ExceptionWindow>();
            (inst.DataContext as ExceptionWindowViewModel).Exception = new AccessViolationException("The exception that is thrown when there is an attempt to read or write protected memory.", new ArithmeticException("Overflow operation"));
            
        }

        /// <summary>
        /// Initializes all commands
        /// </summary>
        private void InitializeCommands()
        {
            _openWindow = new RelayCommand(OpenWindowAction, param => true);
        }

        #endregion

        #region Methdos

        /// <summary>
        /// Called when the profile list is updated from the profile list
        /// </summary>
        public void UpdateProfileListFromProfilesWindow(ObservableImmutableList<ClientProfileVm> list)
        {
            string oldProfileName = "";

            if (SelectedWorkingProfile != null)
                oldProfileName = SelectedWorkingProfile.Name;
            ProfileList = list;
            
            if (!String.IsNullOrWhiteSpace(oldProfileName))
                SelectedWorkingProfile = ProfileList.First(x => x.Name == oldProfileName);
        }

        #endregion

        #region Commands

        #region Command Actions

        /// <summary>
        /// Called by all "Open Window" actions
        /// </summary>
        /// <param name="param"></param>
        private void OpenWindowAction(object param)
        {
            switch (param.ToString())
            {
                case "PROFILES_MANAGER":
                    ViewManager.ShowWindow<ProfileManagerView>();
                    break;
                case "EXIT":
                    ViewManager.TerminateProgram();
                    break;
                case "PACKING_FILTERS":
                    ViewManager.ShowWindow<PackingFiltersView>();
                    break;
                case "VIRTUAL_TREE_VIEW":
                    // If no selected profile, return
                    if (SelectedWorkingProfile == null)
                    {
                        UserInput.ShowMessage("SELECT_PROFILE_FIRST");
                        break;
                    }
                    ViewManager.ShowWindow<VirtualTreeViewWindow>(false, String.Format("Virtual Tree View - {0}", SelectedWorkingProfile.Name));
                    break;
                case "UPDATE_WINDOW":
                    ViewManager.ShowWindow<UpdateMenuView>();
                    break;
                default:
                    WindowLog.Critical(new[] { "INTERNAL_ERROR", "Error at OpenWindowAction with argument:", param });
                    break;
            }
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        /// <summary>
        /// OpenWindowCommand interface
        /// </summary>
        public ICommand OpenWindowCommand
        {
            get { return _openWindow; }
        }

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion

        #region Presentation Members

        private bool _canChangeProfile;

        /// <summary>
        /// Defines wether the combobox is enabled
        /// </summary>
        public bool CanChangeProfile
        {
            get { return _canChangeProfile; }
            set
            {
                SetProperty(ref _canChangeProfile, value, "CanChangeProfile");
            }
        }

        private ObservableImmutableList<ClientProfileVm> _profileList;

        /// <summary>
        /// List of all profiles
        /// </summary>
        public ObservableImmutableList<ClientProfileVm> ProfileList
        {
            get { return _profileList; }
            set { SetProperty(ref _profileList, value, "ProfileList"); }
        }

        private ClientProfileVm _selectedWorkingProfile;

        /// <summary>
        /// Holds reference to the selected profile, assigned when ProfilesVM fires up the event
        /// </summary>
        public ClientProfileVm SelectedWorkingProfile
        {
            get { return _selectedWorkingProfile; }
            set
            {
                SetProperty(ref _selectedWorkingProfile, value, "SelectedWorkingProfile");

                // Update all profile references
                EterHelper.SelectedProfile = value;

                if (ProfilesVm.Instance != null)
                    ProfilesVm.Instance.SelectedProfile = value;

                if (FilesActionVm.Instance != null)
                    FilesActionVm.Instance.ProcessWorkingDirectory();
            }
        }

        private string _currentAppVersion;

        /// <summary>
        /// Current app version
        /// </summary>
        public string CurrentAppVersion
        {
            get { return _currentAppVersion; }
            set { SetProperty(ref _currentAppVersion, value, "CurrentAppVersion"); }
        }
        
        #endregion

        #region Others

        public static MainWindowVm Instance { get; set; }

        #endregion

        #endregion

        #region IHandle<ClientProfileVM> Members

        /// <summary>
        /// Updates selected profile
        /// </summary>
        /// <param name="message"></param>
        public void Handle(ClientProfileVm message)
        {
            SelectedWorkingProfile = message;
            EterHelper.SelectedProfile = message;
            FilesActionVm.Instance.StartMonitoringDirectory();
        }

        #endregion

        #region View Events

        /// <summary>
        /// Called from view when it is closing
        /// </summary>
        public void OnWindowClose(object sender, CancelEventArgs cancelEventArgs)
        {
            Properties.Settings.Default.Save();
        }

        #endregion

        #region AppUpdater Events

        /// <summary>
        /// Handles OnDownloadCompleted
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AsyncCompletedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void AppUpdaterOnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Properties.Settings.Default.UpdateMode == "ASK_INSTALL" && MessageBox.Show("An update has been downloaded", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AppUpdater.InstallLatestVersion();
            }
            else if (Properties.Settings.Default.UpdateMode == "AUTO")
            {
                AppUpdater.InstallLatestVersion();
            }

        }

        #endregion

        #region Destructor

        #endregion
    }
}
