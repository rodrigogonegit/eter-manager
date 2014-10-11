using System.Windows;
using System.Windows.Controls;
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views.UserControls
{
    /// <summary>
    /// Interaction logic for IssuesListView.xaml
    /// </summary>
    public partial class IssuesListView : UserControl
    {
        public IssuesListView()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<IssuesListViewModel>();
            }
        }
    }
}
