using System.Windows;
using System.Windows.Controls;
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ProfilesTreeView.xaml
    /// </summary>
    public partial class ProfilesView
    {
        public ProfilesView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dc = (DataContext as ProfilesVm);

            if (dc.SelectProfileCommand.CanExecute(null))
                dc.SelectProfileCommand.Execute(null);
        }
    }
}
