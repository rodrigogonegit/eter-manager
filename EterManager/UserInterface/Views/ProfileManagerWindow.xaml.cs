using System;
using System.Collections.Generic;
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
    /// Interaction logic for ProfileManagerView.xaml
    /// </summary>
    public partial class ProfileManagerView : Window
    {
        public ProfileManagerView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ((App)Application.Current).GetInstance<ProfilesVM>();
        }
    }
}
