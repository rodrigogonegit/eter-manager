using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Models;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    public class ClientProfileVM : ViewModelBase, INotifyDataErrorInfo
    {
        #region Fields

        private readonly ClientProfile _profile;
        private readonly Dictionary<String, List<String>> _errors = new Dictionary<string, List<string>>();

        // Commands

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

            #endregion
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves current profile to file
        /// </summary>
        public void SaveProfile()
        {
            try
            {
                _profile.Save();
                Logger.Information("PROFILE_SAVED", null, Name);
            }
            catch (ProfileNameAlreadyExistsException e)
            {
                Name = _profile.OriginalName;
                UserInput.ShowMessage("PROFILE_NAME_ALREADY_EXISTS");
            }
            catch (IOException e)
            {
                Logger.Error("COULD_NOT_ACCESS_FILE", String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name), e.Message);
            }
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
                if (_profile.Name != value && IsNameValid(value))
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
                if (_profile.WorkingDirectory != value && IsWorkingDirectoryValid(value))
                {
                    _profile.WorkingDirectory = StringHelpers.AddSlashToEnd(value);
                    OnPropertyChanged("WorkingDirectory");
                }
            }
        }

        public string UnpackDirectory
        {
            get { return _profile.UnpackDirectory; }
            set
            {
                if (_profile.UnpackDirectory != value && IsUnpackDirectoryValid(value))
                {
                    _profile.UnpackDirectory = StringHelpers.AddSlashToEnd(value);
                    OnPropertyChanged("UnpackDirectory");
                }
            }
        }

        public byte[] IndexKey
        {
            get { return _profile.IndexKey; }
            set
            {
                if (_profile.IndexKey != value && IsKeyValid(BitConverter.ToString(value), "IndexKey"))
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
                    _profile.IndexExtension = StringHelpers.AddExtensionPoint(value);
                    OnPropertyChanged("IndexExtension");
                }
            }
        }

        public string PackExtension
        {
            get { return _profile.PackExtension; }
            set
            {
                if (_profile.PackExtension != StringHelpers.AddExtensionPoint(value))
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

        #region Validation

        #region Custom Validation

        /// <summary>
        /// VAlidates Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsNameValid(string name)
        {
            if (name.Trim() == "")
            {
                AddError("Name", "Name cannot be null.", false);
                return false;
            }

            // If so, then try to remove
            RemoveError("Name", "Name cannot be null");

            var isUnique = _profile.IsUniqueName(name);

            if (!isUnique)
                AddError("Name", "Name already exists!", false);
            else
                RemoveError("Name", "Name already exists!");

            return isUnique;
        }

        /// <summary>
        /// Validates Working Directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsWorkingDirectoryValid(string path)
        {
            var exists = Directory.Exists(path);
            if (!exists)
                 AddError("WorkingDirectory", "Path cannot be reached", false);
             else
             {
                 RemoveError("WorkingDirectory", "Path cannot be reached");
             }
             return exists;
        }

        /// <summary>
        /// Validates Unpack Directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsUnpackDirectoryValid(string path)
        {
            var exists = Directory.Exists(path);
            if (!exists)
                AddError("UnpackDirectory", "Path cannot be reached", false);
            else
            {
                RemoveError("UnpackDirectory", "Path cannot be reached");
            }
            return exists;
        }

        private bool IsKeyValid(string key, string property)
        {
            try
            {
                string hex = key.Replace(" ", "");
                hex = hex.Replace("-", "");
                Enumerable.Range(0, hex.Length)
             .Where(x => x % 2 == 0)
             .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16));
            }
            catch (Exception)
            {
                AddError(property, "Hex string could not be parsed.", false);
                return false;
            }

            RemoveError(property, "Hex string could not be parsed.");
            return true;
        }

        #endregion

        #region Generic Members

        /// <summary>
        /// Adds the specified error to the errors collection if it is not
        /// already present, inserting it in the first position if isWarning is
        /// false. Raises the ErrorsChanged event if the collection changes.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="error"></param>
        /// <param name="isWarning"></param>
        public void AddError(string propertyName, string error, bool isWarning)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                if (isWarning) _errors[propertyName].Add(error);
                else _errors[propertyName].Insert(0, error);
                RaiseErrorsChanged(propertyName);
            }
        }


        /// <summary>
        /// Removes the specified error from the errors collection if it is
        /// present. Raises the ErrorsChanged event if the collection changes.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="error"></param>
        public void RemoveError(string propertyName, string error)
        {
            if (_errors.ContainsKey(propertyName) &&
                _errors[propertyName].Contains(error))
            {
                _errors[propertyName].Remove(error);
                if (_errors[propertyName].Count == 0) _errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        #endregion

        #region INotifyDataErrorInfo Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName) ||
                !_errors.ContainsKey(propertyName)) return null;
            return _errors[propertyName];
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        #endregion
    }
}
