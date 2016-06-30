using System.ComponentModel;
using EterManager.UserInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EterManager.UserInterface.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<MainWindowVm>();
            }

            Closing += OnClosing;

            Height = Properties.Settings.Default.MainWindowSize.Height;
            Width = Properties.Settings.Default.MainWindowSize.Width;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Properties.Settings.Default.MainWindowSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);

            (DataContext as MainWindowVm).OnWindowClose(sender, cancelEventArgs);
            Application.Current.Shutdown();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            (DataContext as MainWindowVm).OnWindowActivated();
        }
    }
}
