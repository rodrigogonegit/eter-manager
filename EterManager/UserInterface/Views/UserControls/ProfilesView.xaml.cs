using System.Windows;
using System.Windows.Controls;
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ProfilesTreeView.xaml
    /// </summary>
    public partial class ProfilesView : UserControl
    {
        public ProfilesView()
        {
            InitializeComponent();
            //DataContext = ((App)Application.Current).GetInstance<ProfilesVM>();
        }
    }
}
