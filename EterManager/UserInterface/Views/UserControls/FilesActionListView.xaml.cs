using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EterManager.UserInterface.ViewModels;

namespace EterManager.UserInterface.Views.UserControls
{
    /// <summary>
    /// Interaction logic for FilesActionListView.xaml
    /// </summary>
    public partial class FilesActionListView
    {
        // Percentages
        private double state = 0.1;
        private double filename = 0.25;
        private double progress = 0.3;
        private double actions = 0.20;
        private double PackFilter = 0.12;

        private FilesActionVm _dataContext;


        public FilesActionListView()
        {
            InitializeComponent();

            _dataContext = ((App)Application.Current).GetInstance<FilesActionVm>();

            // To avoid design-time issues
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = _dataContext;
            }

            _dataContext.OnWorkingItemProcessedEvent += ForceLvRefresh;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var lv = (ListView) sender;
            var gv = lv.View as GridView;

            gv.Columns[0].Width = state * lv.ActualWidth;
            gv.Columns[1].Width = filename * lv.ActualWidth;
            gv.Columns[2].Width = progress * lv.ActualWidth;
            gv.Columns[3].Width = actions * lv.ActualWidth;
            gv.Columns[4].Width = PackFilter * lv.ActualWidth;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = (ListView)sender;
            var gv = lv.View as GridView;

            gv.Columns[0].Width = state * lv.ActualWidth;
            gv.Columns[1].Width = filename * lv.ActualWidth;
            gv.Columns[2].Width = progress * lv.ActualWidth;
            gv.Columns[3].Width = actions * lv.ActualWidth;
            gv.Columns[4].Width = PackFilter * lv.ActualWidth;
        }

        public void ForceLvRefresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void mainLv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _dataContext.MouseDoubleClick();
        }

        private void MainLv_OnDrop(object sender, DragEventArgs e)
        {
            (DataContext as FilesActionVm).OnFileDropped(e.Data);
        }
    }
}
