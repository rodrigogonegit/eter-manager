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
    class ProfilesVM : ViewModelBase
    {
        #region Fields

        // Profile list
        private ObservableImmutableList<ClientProfileVM> _profileList = new ObservableImmutableList<ClientProfileVM>();
        private ClientProfileVM _selectedProfile = new ClientProfileVM();

        // Commands
        private readonly RelayCommand _addProfile;
        private readonly RelayCommand _removeProfile;
        private readonly RelayCommand _selectProfile;


        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates new object of class
        /// </summary>
        public ProfilesVM()
        {
            #region Load Profile List

            var t = new ClientProfile()
            {
                Name = "MyNewProfile",
                IndexExtension = ".eix",
                PackExtension = ".epk",
                IndexKey = new byte[] { 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, },
                PackKey = new byte[] { 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, 0xB5, },
                WorkingDirectory = @"c:\",
                UnpackDirectory = @"c:\"
            };

            // Create serializer
            var deserializer = new XmlSerializer(typeof(ClientProfile));

            //using (StreamWriter writer = new StreamWriter("AppData/Profiles/MyNewProfile.xml"))
            //{
            //    deserializer.Serialize(writer, t);
            //}

            // Loop through directory's files
            foreach (var file in new DirectoryInfo(ConstantsBase.ProfilesPath).GetFiles("*.xml"))
            {
                ProfileList.Add(
                    new ClientProfileVM(
                        deserializer.Deserialize(
                        new StreamReader(file.FullName)) as ClientProfile));
            }

            #endregion

            #region Command Instantiation

            _addProfile = new RelayCommand(p => AddNewProfileAction(), p => true);
            _removeProfile = new RelayCommand(p => RemoveProfileAction(), p => SelectedProfile != null);
            _selectProfile = new RelayCommand(p => SelectProfileAction(), p => SelectedProfile != null);

            #endregion
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
            ClientProfileVM outProfile = ProfileList.FirstOrDefault(x => x.Name == null);

            if (outProfile != null)
            {
                SelectedProfile = outProfile;
            }
            else
            {
                ProfileList.Add(
                    new ClientProfileVM(
                        new ClientProfile())
                    );
            }
        }

        /// <summary>
        /// Removes selected profile from the list
        /// </summary>
        private void RemoveProfileAction()
        {
            if (SelectedProfile != null)
            {
                ProfileList.Remove(SelectedProfile);
            }
        }

        private void SelectProfileAction()
        {
            if (SelectedProfile != null)
            {
                EventAggregator.Publish(SelectedProfile);
            }
        }
        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        public ICommand AddProfileCommand
        {
            get { return _addProfile; }
        }

        public ICommand RemoveProfileCommand
        {
            get { return _removeProfile; }
        }

        public ICommand SelectProfileCommand
        {
            get { return _selectProfile; }
        }

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion
        
        #region Presentation Members

        public ObservableImmutableList<ClientProfileVM> ProfileList
        {
            get { return _profileList; }
            set { SetProperty(ref _profileList, value, "ProfileList"); }
        }

        public ClientProfileVM SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value, "SelectedProfile"); }
        }

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
