using System;
using System.Collections.Specialized;
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
                DataContext = ((App)Application.Current).GetInstance<IssuesListVm>();
            }

            //((INotifyCollectionChanged)mainLv.Items).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (MainLv.Items.Count <= 0)
                return;
            MainLv.ScrollIntoView(MainLv.Items[MainLv.Items.Count - 1]);
        }
    }
}
