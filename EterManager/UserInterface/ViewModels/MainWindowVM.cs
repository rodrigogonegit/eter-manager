using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EterManager.Base;
using EterManager.UserInterface.Views;
using EterManager.Utilities;
using EterManager.Services;

namespace EterManager.UserInterface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly RelayCommand _openWindow;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates new instance of MainVM
        /// </summary>
        public MainViewModel()
        {
            _openWindow = new RelayCommand(OpenWindowAction, param => true);
        }

        #endregion

        #region Commands

        #region Command Actions

        private void OpenWindowAction(object param)
        {
            switch (param.ToString())
            {
                case "PROFILES_MANAGER":
                    ViewManager.ShowWindow<ProfileManagerView>();
                    break;
            }
        }

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

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

        #endregion

        #region Others

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
