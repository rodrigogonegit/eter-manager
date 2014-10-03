using EterManager.Services;
using EterManager.Services.Abstract;
using EterManager.Services.Concrete;
using EterManager.UserInterface.Views;
using Ninject;
using System.Windows;

namespace EterManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private IKernel _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetBindings();
            InitializeMainView();
        }

        private void SetBindings()
        {
            _container = new StandardKernel();
            _container.Bind<IViewManager>().To<ViewManager>().InSingletonScope();
            _container.Bind<ILogger>().To<Logger>().InSingletonScope();
        }

        private void InitializeMainView()
        {
            Current.MainWindow = new MainView();
            Current.MainWindow.Show();
        }

        public T GetInstance<T>()
        {
            var obj = _container.Get<T>();
            return obj;
        }
    }
}
