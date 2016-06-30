using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EterManager.Base;
using EterManager.DataAccessLayer;
using EterManager.Models;
using EterManager.UserInterface.ViewModels.TreeItem;
using EterManager.UserInterface.Views;
using EterManager.Utilities;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    /// <summary>
    /// VirtualTreeViewWindow VM class
    /// </summary>
    class VirtualTreeViewWindowVm : ViewModelBase
    {        
        #region Fields

        /// <summary>
        /// Working directory
        /// </summary>
        private string _workingDir;

        /// <summary>
        /// Index file extension
        /// </summary>
        private string _indexExtension;

        /// <summary>
        /// XTEA key used to decrypt the index file
        /// </summary>
        private byte[] _indexKey;

        /// <summary>
        /// SearchFileCommand RelayCommand obj
        /// </summary>
        private RelayCommand _searchFile;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        /// Main Ctor
        /// </summary>
        public VirtualTreeViewWindowVm()
        {
            // Initialize members
            VirtualTreeViewItems = new ObservableImmutableList<ITreeViewItem>();

            // Internal initializer
            Initialize();

            // Initialize commands
            InitializeCommands();

            // Initialize virtual tree view
            InitializeVirtualView();
        }

        /// <summary>
        /// Internal initializer
        /// </summary>
        private void Initialize()
        {
            _workingDir = MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory;
            _indexExtension = MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension;
            _indexKey = MainWindowVm.Instance.SelectedWorkingProfile.IndexKey;
            IsTextboxEnabled = true;
        }

        /// <summary>
        /// Initializes all commands
        /// </summary>
        private void InitializeCommands()
        {
            _searchFile = new RelayCommand(p => SearchFileCommandAction(), p => IsTextboxEnabled);
        }

        /// <summary>
        /// Initializes virtual view
        /// </summary>
        private async void InitializeVirtualView()
        {
            await Task.Run(() =>
            {
                // Clear list
                VirtualTreeViewItems.Clear();

                // Create first item
                VirtualTreeViewItems.Add(new TreeItemFolderVm(null)
                {
                    DisplayName = "Virtual view"
                });

                // Get all files
                var files = IOHelper.GetAllFilesFromDir(_workingDir)
                    .Where(x => String.Equals(x.Extension, _indexExtension, StringComparison.CurrentCultureIgnoreCase)).Reverse();
                
                // File counters
                double fileCounter = 0;
                double totalFileCount = files.Count();

                // Loop through all files
                foreach (var file in files)
                {
                    // Get all items
                    var items = EterFilesDal.ReadIndexFile(file.FullName, _indexKey,
                        Path.GetFileNameWithoutExtension(file.Name));

                    if (items == null)
                        continue;

                    // Loop through all items of the index file
                    foreach (var item in items)
                    {
                        // Replace all backslashes with forward slashes
                        var path = item.Filename.Replace("\\", "/");

                        // Split by slash
                        var tokens = path.Split(new[] { '/' });

                        var currentTreeItem = VirtualTreeViewItems.First() as TreeItemFolderVm;

                        // If first layer item
                        if (tokens.Count() == 1)
                        {
                            currentTreeItem.Children.Add(new TreeItemFileVm()
                            {
                                DisplayName = item.Filename,
                                Parent = currentTreeItem,
                                EterFileParent = item.ParentFile,
                                Fullname = item.Filename
                            });
                        }
                        else
                        {
                            // Token counter
                            int counter = 0;

                            // Holds a reference to the current full path
                            string currentFullpath = "";

                            // Loop through tokens
                            foreach (var token in tokens)
                            {
                                counter++;
                                currentFullpath += token + "/";

                                // Check if item already exists
                                var itemInTreeView =
                                    currentTreeItem.Children.FirstOrDefault(
                                        x =>
                                            String.Equals(token, x.DisplayName,
                                                StringComparison.CurrentCultureIgnoreCase));

                                // Create item if not already there
                                if (itemInTreeView == null)
                                {
                                    // If last one
                                    if (counter == tokens.Count())
                                    {
                                        currentTreeItem.Children.Add(new TreeItemFileVm()
                                        {
                                            DisplayName = token,
                                            Parent = currentTreeItem,
                                            EterFileParent = item.ParentFile,
                                            Fullname = item.Filename
                                        });
                                    }
                                    else
                                    {
                                        var obj = new TreeItemFolderVm()
                                        {
                                            DisplayName = token,
                                            Parent = currentTreeItem,
                                            Fullname = currentFullpath
                                        };

                                        currentTreeItem.Children.Add(obj);

                                        currentTreeItem = obj;
                                    }
                                }
                                else
                                {
                                    currentTreeItem = itemInTreeView as TreeItemFolderVm;
                                }
                            }
                        }
                    }

                    State = String.Format("Processing global hash table... {0}%", Math.Round(fileCounter++/totalFileCount * 100, 2));
                }

                State = "Sorting list...";

                VirtualTreeViewItems = SortItemsByFolder(VirtualTreeViewItems);

                (VirtualTreeViewItems.First() as TreeItemFolderVm).IsExpanded = true;

                State = "All files have been processed.";
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sorts list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private ObservableImmutableList<ITreeViewItem> SortItemsByFolder(ObservableImmutableList<ITreeViewItem> list)
        {
            foreach (var child in list)
            {
                var folder = child as TreeItemFolderVm;

                if (folder != null && folder.Children.Count != 0)
                {
                    folder.Children = SortItemsByFolder(folder.Children);
                    folder.Children = new ObservableImmutableList<ITreeViewItem>(folder.Children.OrderBy(x => x.DisplayName).OrderByDescending(p => p.IsFolder));
                }
            }

            return list;
        }

        #endregion

        #region Commands

        #region Command Actions

        /// <summary>
        /// Action performed when SearchFileCommand is hit
        /// </summary>
        private void SearchFileCommandAction()
        {
            // Disable textbox
            IsTextboxEnabled = false;

            var firstItem = (VirtualTreeViewItems.First() as TreeItemFolderVm);
            var allItems = VirtualTreeViewItems.GetAllItems();

            foreach (var item in allItems)
                item.SetVisibility(false);

            firstItem.PerformFileSearch(SearchFileText);

            firstItem.IsExpanded = true;
            firstItem.IsVisible = true;

            IsTextboxEnabled = true;
        }

        //private IEnumerable<ITreeViewItem> PerformSearch(ITreeViewItem treeItem)
        //{
        //    if (treeItem.Fullname.ToLower().Contains(SearchFileText))
        //        treeItem.SetVisibility(true);

        //    if (treeItem.GetType() == typeof(TreeItemFolderVm))
        //    foreach (var child in (treeItem as TreeItemFolderVm).Children)
        //        foreach (var match in PerformSearch(child))
        //            yield return match;
        //}

        #endregion

        #region Command Evaluators

        #endregion

        #region Command Interfaces

        /// <summary>
        /// SearchFileCommand interface
        /// </summary>
        public ICommand SearchFileCommand
        {
            get { return _searchFile; }
        }

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion

        #region Presentation Members

        private ObservableImmutableList<ITreeViewItem> _selectedItems;

        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public ObservableImmutableList<ITreeViewItem> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value, "SelectedItems"); }
        }

        private string _state;

        /// <summary>
        /// State at which the processing is
        /// </summary>
        public string State
        {
            get { return _state; }
            set { SetProperty(ref _state, value, "State"); }
        }

        private ObservableImmutableList<ITreeViewItem> _virtualTreeViewItems;

        /// <summary>
        /// List of the virtual tree view items
        /// </summary>
        public ObservableImmutableList<ITreeViewItem> VirtualTreeViewItems
        {
            get { return _virtualTreeViewItems; }
            set { SetProperty(ref _virtualTreeViewItems, value, "VirtualTreeViewItems"); }
        }

        private bool _isTextboxEnabled;

        /// <summary>
        /// Sets or gets wether the textbox is enabled
        /// </summary>
        public bool IsTextboxEnabled
        {
            get { return _isTextboxEnabled; }
            set { SetProperty(ref _isTextboxEnabled, value, "IsTextboxEnabled"); }
        }

        private string _searchFileText;

        /// <summary>
        /// Search file text pattern
        /// </summary>
        public string SearchFileText
        {
            get { return _searchFileText; }
            set { SetProperty(ref _searchFileText, value, "SearchFileText"); }
        }

        #endregion

        #region Others

        #endregion

        #endregion

        #region DestructorB

        #endregion
    }
}
