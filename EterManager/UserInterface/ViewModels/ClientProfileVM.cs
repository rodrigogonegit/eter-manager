using System;
using System.Diagnostics;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Models;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    class ClientProfileVM : ViewModelBase
    {
        #region Fields

        private readonly ClientProfile _profile;

        // Commands
        private readonly RelayCommand _save;

        #endregion

        #region Constructors

        /// <summary>
        /// Null argument overload
        /// </summary>
        public ClientProfileVM() : this(new ClientProfile()) { }

        /// <summary>
        /// Creates new profile instance
        /// </summary>
        /// <param name="profile"></param>
        public ClientProfileVM(ClientProfile profile)
        {
            _profile = profile;

            #region Command Instantiation

            _save = new RelayCommand(p => SaveAction(), p => true);

            #endregion
        }

        #endregion

        #region Public Methods

        #endregion

        #region Commands

        #region Command Actions

        public void SaveAction()
        {
            try
            {
                _profile.Save();
            }
                //catch (Exception e)
                //{
                //    // Log to file and alert user
                //    Logger.Critical(new object[] { Locale.GetString("ERROR_SAVING_PROFILE"), e });
                //    UserInput.ShowMessage(Locale.GetString("ERROR_SAVING_PROFILE"));
                //}
            finally
            {
                
            }
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        public ICommand SaveCommand
        {
            get { return _save; }
        }

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
