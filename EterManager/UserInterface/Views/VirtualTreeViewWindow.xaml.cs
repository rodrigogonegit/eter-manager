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
        }
    }
}
