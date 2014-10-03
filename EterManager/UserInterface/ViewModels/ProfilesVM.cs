using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EterManager.Base;
using EterManager.Models;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    class ProfilesVM : ViewModelBase
    {
        #region Fields

        // Profile list
        private ObservableImmutableList<ClientProfileVM> _profileList = new ObservableImmutableList<ClientProfileVM>();

        #endregion

        #region Constructors

        public ProfilesVM()
        {
            #region Load Profile List

            // Create serializer
            var deserializer = new XmlSerializer(typeof(ClientProfile));

            // Loop through directory's files
            foreach (var file in new DirectoryInfo(ProfilesPath).GetFiles("*.xml"))
            {
                ProfileList.Add(
                    new ClientProfileVM(
                        deserializer.Deserialize(
                        new StreamReader(file.FullName)) as ClientProfile));
            }

            #endregion
        }

        #endregion

        #region Commands

        #region Command Actions

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion
        
        #region Presentation Members

        ObservableImmutableList<ClientProfileVM> ProfileList
        {
            get { return _profileList; }
        }

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
