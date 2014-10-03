using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.Serialization;
using System.Xml.Serialization;
using EterManager.Base;
using EterManager.Models;

namespace EterManager.UserInterface.ViewModels
{
    class ClientProfileVM : ViewModelBase
    {
        #region Fields

        private ClientProfile _profile;

        #endregion

        #region Constructors

        /// <summary>
        /// Null argument overload
        /// </summary>
        public ClientProfileVM() : this(null) { }

        /// <summary>
        /// Creates new profile instance
        /// </summary>
        /// <param name="profile"></param>
        public ClientProfileVM(ClientProfile profile)
        {
            _profile = profile;
        }

        #endregion

        #region Public Methods

        public void Save()
        {
            try
            {
                // Create file stream
                using (var writer = new StreamWriter(String.Format("{0}{1}.xml", ProfilesPath, Name)))
                {
                    var serializer = new XmlSerializer(typeof(ClientProfile));
                    serializer.Serialize(writer, _profile);
                }
            }
            catch (InvalidOperationException e)
            {
                // TODO: HANDLE EXCEPTION
                //_profile.Name = "SS";
                throw;
            }
            // TODO: HANDLE ALL EXCEPETIONS THROWN BY CREATION OF FILE


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

        public string Name
        {
            get { return _profile.Name; }
            set
            {
                if (_profile.Name != value)
                {
                    _profile.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string WorkingDirectory
        {
            get { return _profile.WorkingDirectory; }
            set
            {
                if (_profile.WorkingDirectory != value)
                {
                    _profile.WorkingDirectory = value;
                    OnPropertyChanged("WorkingDirectory");
                }
            }
        }

        public string UnpackDirectory
        {
            get { return _profile.UnpackDirectory; }
            set
            {
                if (_profile.UnpackDirectory != value)
                {
                    _profile.UnpackDirectory = value;
                    OnPropertyChanged("UnpackDirectory");
                }
            }
        }

        public byte[] IndexKey
        {
            get { return _profile.IndexKey; }
            set
            {
                if (_profile.IndexKey != value)
                {
                    _profile.IndexKey = value;
                    OnPropertyChanged("IndexKey");
                }
            }
        }

        public byte[] PackKey
        {
            get { return _profile.PackKey; }
            set
            {
                if (_profile.PackKey != value)
                {
                    _profile.PackKey = value;
                    OnPropertyChanged("PackKey");
                }
            }
        }

        public string IndexExtension
        {
            get { return _profile.IndexExtension; }
            set
            {
                if (_profile.IndexExtension != value)
                {
                    _profile.IndexExtension = value;
                    OnPropertyChanged("IndexExtension");
                }
            }
        }

        public string PackExtension
        {
            get { return _profile.PackExtension; }
            set
            {
                if (_profile.PackExtension != value)
                {
                    _profile.PackExtension = value;
                    OnPropertyChanged("PackExtension");
                }
            }
        }

        #endregion

        #region Presentation Members

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
