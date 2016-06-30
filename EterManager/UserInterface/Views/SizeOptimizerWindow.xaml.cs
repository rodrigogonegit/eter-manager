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
    /// Interaction logic for SizeOptimizerWindow.xaml
    /// </summary>
    public partial class SizeOptimizerWindow : Window
    {
        public SizeOptimizerWindow()
        {
            InitializeComponent();

            // To avoid design-time issues
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ((App)Application.Current).GetInstance<SizeOptimizerWindowViewModel>();
            }

            Height = Properties.Settings.Default.SizeOptimizerWindowSize.Height;
            Width = Properties.Settings.Default.SizeOptimizerWindowSize.Width;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SizeOptimizerWindowSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
        }
    }
}
