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
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<ExceptionWindowViewModel>();
            }
        }
    }
}
