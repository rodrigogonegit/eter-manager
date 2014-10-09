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
        private readonly RelayCommand _addNewProfile;
        private readonly RelayCommand _removeProfile;


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

            _addNewProfile = new RelayCommand(p => AddNewProfileAction(), p => true);
            _removeProfile = new RelayCommand(p => RemoveProfileAction(), p => CanExecuteRemoveProfile());

            #endregion
        }

        #endregion

        #region Commands

        #region Command Actions

        private void AddNewProfileAction()
        {
            
        }

        private void RemoveProfileAction()
        {

        }

        #endregion

        #region Command Evaluators

        private bool CanExecuteRemoveProfile()
        {
            return SelectedProfile != null;
        }

        #endregion

        #region Command Interfaces

        public ICommand AddNewProfileCommand
        {
            get { return _addNewProfile; }
        }

        public ICommand RemoveProfileCommand
        {
            get { return _removeProfile; }
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
