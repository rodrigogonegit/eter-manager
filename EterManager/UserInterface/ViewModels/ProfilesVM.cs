using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using EterManager.Base;
using EterManager.Models;
using EterManager.Utilities;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    class ProfilesVm : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// AddProfileCommand RelayCommand obj
        /// </summary>
        private RelayCommand _addProfile;

        /// <summary>
        /// RemoteProfileCommand RelayCommand obj
        /// </summary>
        private RelayCommand _removeProfile;

        /// <summary>
        /// SelectProfile RelayCommand obj
        /// </summary>
        private RelayCommand _selectProfile;

        /// <summary>
        /// SaveProfile RelayCommand obj
        /// </summary>
        private RelayCommand _saveProfile;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        /// Instantiates new object of class
        /// </summary>
        public ProfilesVm()
        {
            Instance = this;
            ProfileList = new ObservableImmutableList<ClientProfileVm>();

            Initialize();
            InitializeCommands();
        }

        private void Initialize()
        {
            // Load list
            ProfileList =
                new ObservableImmutableList<ClientProfileVm>(
                    ClientProfile.GetAllProfiles().Select(x => new ClientProfileVm(x)));

            if (MainWindowVm.Instance.SelectedWorkingProfile != null)
            {
                SelectedProfile = MainWindowVm.Instance.SelectedWorkingProfile;
            }
            else if (ProfileList == null || ProfileList.Count == 0)
            {
                SelectedProfile = new ClientProfileVm()
                {
                    Name = "Default Profile",
                    IndexKey = new byte[] { 0xB9, 0x9E, 0xB0, 0x02, 0x6F, 0x69, 0x81, 0x05, 0x63, 0x98, 0x9B, 0x28, 0x79, 0x18, 0x1A, 0x00 },
                    PackKey = new byte[] { 0x22, 0xB8, 0xB4, 0x04, 0x64, 0xB2, 0x6E, 0x1F, 0xAE, 0xEA, 0x18, 0x00, 0xA6, 0xF6, 0xFB, 0x1C },
                    PackExtension = ".epk",
                    IndexExtension = ".eix",
                    WorkingDirectory = "WORKING_DIR",
                    UnpackDirectory = "UNPACK_DIR"
                };

                ProfileList.Add(SelectedProfile);
            }
            else
            {
                SelectedProfile = ProfileList.FirstOrDefault(x => x.IsDefault) ?? ProfileList.First();

            }
        }

        /// <summary>
        /// Initializes all commands
        /// </summary>
        private void InitializeCommands()
        {
            _addProfile = new RelayCommand(p => AddNewProfileAction(), p => true);
            _removeProfile = new RelayCommand(p => RemoveProfileAction(), p => SelectedProfile != null);
            _selectProfile = new RelayCommand(p => SelectProfileAction(), p => SelectedProfile != null);
            _saveProfile = new RelayCommand(p => SaveProfileAction(), p => SelectedProfile != null);
        }

        #endregion

        #region Methods

        public void ProfileListBoxDoubleClick()
        {
            if (SelectedProfile == null) return;

            // Make sure everything else is notified of action
            SelectProfileAction();
        }

        #endregion

        #region Commands

        #region Command Actions

        /// <summary>
        /// Adds new profile to the list
        /// </summary>
        private void AddNewProfileAction()
        {
            // Check if there is a blank profile already, if so select it
            ClientProfileVm outProfile = ProfileList.FirstOrDefault(x => x.Name == null || x.Name == "Default Profile");

            if (outProfile != null)
            {
                SelectedProfile = outProfile;
            }
            else
            {
                var profile = new ClientProfileVm()
                {
                    Name = "Default Profile",
                    IndexKey = new byte[] { 0xB9, 0x9E, 0xB0, 0x02, 0x6F, 0x69, 0x81, 0x05, 0x63, 0x98, 0x9B, 0x28, 0x79, 0x18, 0x1A, 0x00 },
                    PackKey = new byte[] { 0x22, 0xB8, 0xB4, 0x04, 0x64, 0xB2, 0x6E, 0x1F, 0xAE, 0xEA, 0x18, 0x00, 0xA6, 0xF6, 0xFB, 0x1C },
                    PackExtension = ".epk",
                    IndexExtension = ".eix",
                    WorkingDirectory = "WORKING_DIR",
                    UnpackDirectory = "UNPACK_DIR"
                };

                ProfileList.Add(profile);
                SelectedProfile = profile;
            }
        }

        /// <summary>
        /// Removes selected profile from the list
        /// </summary>
        private void RemoveProfileAction()
        {
            if (SelectedProfile != null)
            {
                if (MainWindowVm.Instance.SelectedWorkingProfile != null &&
                    SelectedProfile.Name == MainWindowVm.Instance.SelectedWorkingProfile.Name)
                {
                    FilesActionVm.Instance.WorkingItemsList.Clear();
                    FilesActionVm.Instance.StopMonitoringDirectory();
                    MainWindowVm.Instance.SelectedWorkingProfile = null;
                }

                SelectedProfile.RemoveProfile();
                ProfileList.Remove(SelectedProfile);
                MainWindowVm.Instance.UpdateProfileListFromProfilesWindow(ProfileList);
            }
        }

        /// <summary>
        /// Fires up "SelectedWorkingProfile" event using the EventAggregator
        /// </summary>
        private void SelectProfileAction()
        {
            if (SelectedProfile != null && !SelectedProfile.HasErrors)
            {
                EventAggregator.Publish(SelectedProfile);
                //MainWindowVm.Instance.SelectedWorkingProfile = SelectedProfile;
                FilesActionVm.Instance.StartMonitoringDirectory();
            }
        }

        /// <summary>
        /// Calls save method on Profile's VM class
        /// </summary>
        private void SaveProfileAction()
        {
            if (SelectedProfile != null)
            {
                if (SelectedProfile.IsDefault)
                {
                    foreach (var profile in ProfileList.Where(x => x.Name != SelectedProfile.Name))
                    {
                        profile.IsDefault = false;
                        profile.SaveProfile(false);
                    }

                    Properties.Settings.Default.DefaultProfile = SelectedProfile.Name;
                }

                SelectedProfile.SaveProfile();
                MainWindowVm.Instance.UpdateProfileListFromProfilesWindow(ProfileList);
            }
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        public ICommand AddProfileCommand => _addProfile;

        public ICommand RemoveProfileCommand => _removeProfile;

        public ICommand SelectProfileCommand => _selectProfile;

        public ICommand SaveCommand => _saveProfile;

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion
        
        #region Presentation Members

        private ObservableImmutableList<ClientProfileVm> _profileList;

        /// <summary>
        /// List of all profiles
        /// </summary>
        public ObservableImmutableList<ClientProfileVm> ProfileList
        {
            get { return _profileList; }
            set { SetProperty(ref _profileList, value, "ProfileList"); }
        }

        private ClientProfileVm _selectedProfile;

        /// <summary>
        /// The currently selected profile
        /// </summary>
        public ClientProfileVm SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value, "SelectedProfile"); }
        }

        #endregion

        #region Others

        /// <summary>
        /// Static reference to the instance
        /// </summary>
        public static ProfilesVm Instance { get; set; }

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
