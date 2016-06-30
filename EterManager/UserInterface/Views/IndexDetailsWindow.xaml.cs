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

namespace EterManager.UserInterface.Views
{
    /// <summary>
    /// Interaction logic for IndexDetailsWindow.xaml
    /// </summary>
    public partial class IndexDetailsWindow : Window
    {
        public IndexDetailsWindow()
        {
            InitializeComponent();

            Height = Properties.Settings.Default.IndexDetailsWindowSize.Height;
            Width = Properties.Settings.Default.IndexDetailsWindowSize.Width;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.IndexDetailsWindowSize = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
        }
    }
}
