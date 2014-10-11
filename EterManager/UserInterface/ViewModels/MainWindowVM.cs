using System.Windows.Input;
using Caliburn.Micro;
using EterManager.Base;
using EterManager.UserInterface.Views;
using EterManager.Utilities;

namespace EterManager.UserInterface.ViewModels
{
    public class MainViewModel : ViewModelBase, IHandle<ClientProfileVM>
    {
        #region Fields

        // Commands
        private readonly RelayCommand _openWindow;

        // Fields
        private ClientProfileVM _selectedProfile;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates new instance of MainVM
        /// </summary>
        public MainViewModel()
        {
            _openWindow = new RelayCommand(OpenWindowAction, param => true);
            EventAggregator.Subscribe(this);
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

        /// <summary>
        /// Holds reference to the selected profile, assigned when ProfilesVM fires up the event
        /// </summary>
        public ClientProfileVM SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value, "SelectedProfile"); }
        }

        #endregion

        #region Others

        #endregion

        #endregion

        #region IHandle<ClientProfileVM> Members

        /// <summary>
        /// Updates selected profile
        /// </summary>
        /// <param name="message"></param>
        public void Handle(ClientProfileVM message)
        {
            // TODO: Handler not firing up, event is published but subscriber does not respond
            SelectedProfile = message;
        }

        #endregion

        #region Destructor

        #endregion
    }
}
