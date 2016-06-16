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
        private double _state = 0.1;
        private double _filename = 0.25;
        private double _progress = 0.3;
        private double _actions = 0.20;
        private double _packFilter = 0.12;

        private readonly FilesActionVm _dataContext;


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

            gv.Columns[0].Width = _state * lv.ActualWidth;
            gv.Columns[1].Width = _filename * lv.ActualWidth;
            gv.Columns[2].Width = _progress * lv.ActualWidth;
            gv.Columns[3].Width = _actions * lv.ActualWidth;
            gv.Columns[4].Width = _packFilter * lv.ActualWidth;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = (ListView)sender;
            var gv = lv.View as GridView;

            gv.Columns[0].Width = _state * lv.ActualWidth;
            gv.Columns[1].Width = _filename * lv.ActualWidth;
            gv.Columns[2].Width = _progress * lv.ActualWidth;
            gv.Columns[3].Width = _actions * lv.ActualWidth;
            gv.Columns[4].Width = _packFilter * lv.ActualWidth;
        }

        public void ForceLvRefresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void mainLv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _dataContext.WorkingItemDoubleClick();
        }

        private void MainLv_OnDrop(object sender, DragEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            (DataContext as FilesActionVm).OnFileDropped(e.Data);
        }
    }
}
