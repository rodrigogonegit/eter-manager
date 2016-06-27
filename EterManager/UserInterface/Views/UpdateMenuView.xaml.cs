using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views
{
    /// <summary>
    /// Interaction logic for UpdateMenuView.xaml
    /// </summary>
    public partial class UpdateMenuView : Window
    {
        private double _oldWndHeight = 370;

        public UpdateMenuView()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<UpdateMenuViewModel>();
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var wnd = ((Window)this);

            _oldWndHeight = wnd.ActualHeight;
            wnd.Height = wnd.ActualHeight - _oldWndHeight + 20;
        }

        private void changeLogExpander_Expanded(object sender, RoutedEventArgs e)
        {
            var wnd = ((Window)this);

            if (_oldWndHeight != 0)
                wnd.Height = _oldWndHeight;
        }
    }
}
