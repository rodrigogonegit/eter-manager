using System.Drawing;
using System.Windows;
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views
{
    /// <summary>
    /// Interaction logic for VirtualTreeViewWindow.xaml
    /// </summary>
    public partial class VirtualTreeViewWindow
    {
        public VirtualTreeViewWindow()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<VirtualTreeViewWindowVm>();
            }

            Height = Properties.Settings.Default.VirtualViewWindowSize.Height;
            Width = Properties.Settings.Default.VirtualViewWindowSize.Width;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.VirtualViewWindowSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
        }
    }
}
