using System.Windows.Input;
using EterManager.Base;
using EterManager.UserInterface.Views;
using EterManager.Utilities;

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
                default:
                    Logger.Critical(new[] { "INTERNAL_ERROR", "Error at OpenWindowAction with argument:", param });
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
