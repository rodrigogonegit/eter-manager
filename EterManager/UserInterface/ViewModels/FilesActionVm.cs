using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EterManager.Base;
using EterManager.Utilities;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    class FilesActionVm : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// SeeAllItemsFilterCommand RelayCommand obj
        /// </summary>
        private RelayCommand _setAllItemsFilter;

        /// <summary>
        /// UnpackSelectedItemsCommand RelayCommand obj
        /// </summary>
        private RelayCommand _unpackSelectedItems;

        /// <summary>
        /// PackSelectedItemsCommand RelayCommand obj
        /// </summary>
        private RelayCommand _packSelectedItems;

        /// <summary>
        /// RemoteItemsCommand RelayCommand obj
        /// </summary>
        private RelayCommand _removeItems;

        /// <summary>
        /// SelectOrDeselectCommand RelayCommand obj
        /// </summary>
        private RelayCommand _selectOrDeselectAll;

        /// <summary>
        /// FSW object used to monitor the working directory
        /// </summary>
        private readonly FileSystemWatcher _fsw = new FileSystemWatcher();

        /// <summary>
        /// Object used to lock the list
        /// </summary>
        private static readonly object Obj = new object();

        /// <summary>
        /// Gets or sets wether all the items are selected
        /// </summary>
        private bool _areItemsSelected = false;

        #endregion

        #region Events

        /// <summary>
        /// Type of the event
        /// </summary>
        public delegate void OnWorkingItemProcessedEventHandler();

        /// <summary>
        /// The actual event
        /// </summary>
        public event OnWorkingItemProcessedEventHandler OnWorkingItemProcessedEvent;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        /// Class' main ctor
        /// </summary>
        public FilesActionVm()
        {
            // Initialize members
            Instance = this;
            _workingItems = new ObservableImmutableList<WorkingItemVm>();
            _selectedWorkingItems = new ObservableImmutableList<WorkingItemVm>();
            Queue = new ConcurrentQueue<WorkingItemVm>();

            // Initialize
            Initialize();

            // Initlaize commands
            InitializeCommands();

        }

        /// <summary>
        /// Internal initializer
        /// </summary>
        private void Initialize()
        {
            _fsw.Created += FswOnCreated;
            _fsw.Deleted += FswOnDeleted;
            _fsw.Renamed += FswOnRenamed;

            // Process dir
            ProcessWorkingDirectory();
        }

        /// <summary>
        /// Instantiate commands
        /// </summary>
        private void InitializeCommands()
        {
            _setAllItemsFilter = new RelayCommand(SetAllItemsFilterAction, param => true);
            _unpackSelectedItems = new RelayCommand(param => UnpackSelectedItemsAction(), param => CanExecuteUnpackSelected());
            _packSelectedItems = new RelayCommand(param => PackSelectedItemsAction(), param => CanExecuteUnpackSelected());
            _removeItems = new RelayCommand(param => RemoveItemsAction(), param => CanExecuteUnpackSelected());
            _selectOrDeselectAll = new RelayCommand(param => SelectOrDeselectAllAction(), param => true);
        }

        #endregion

        #region FileSystemWatcher Event Handlers

        /// <summary>
        /// Starts monitoring the working directory
        /// </summary>
        public void StartMonitoringDirectory()
        {
            try
            {
                _fsw.Path = MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory;
                _fsw.EnableRaisingEvents = true;
            }
            catch (ArgumentException ex)
            {
                Logger.Error("COULD_NOT_MONITOR_DIR", null, MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory);
            }
        }

        /// <summary>
        /// Stops monitoring the directory
        /// </summary>
        public void StopMonitoringDirectory()
        {
            _fsw.EnableRaisingEvents = false;
        }

        /// <summary>
        /// On file created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fileSystemEventArgs"></param>
        private void FswOnCreated(object sender, FileSystemEventArgs e)
        {
            var fileInfo = new FileInfo(e.FullPath);

            if (!String.Equals(fileInfo.Extension, MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension,
                    StringComparison.CurrentCultureIgnoreCase))
                return;

            if (WorkingItemsList.FirstOrDefault(x => x.DisplayName == Path.GetFileNameWithoutExtension(fileInfo.Name)) == null)
            {
                lock (Obj)
                {
                    WorkingItemsList.Add(new WorkingItemVm(StringHelpers.TrimExtension(fileInfo.Name), fileInfo.Name, fileInfo.FullName, fileInfo.DirectoryName));
                }
            }
        }

        /// <summary>
        /// On file deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FswOnDeleted(object sender, FileSystemEventArgs e)
        {
            var item =
                WorkingItemsList.FirstOrDefault(x => String.Equals(x.Filename, e.Name, StringComparison.CurrentCultureIgnoreCase));

            if (item != null)
            {
                if (item.PackFileCommand.CanExecute(null))
                    return;

                lock (Obj)
                {
                    WorkingItemsList.Remove(item);
                }
            }
        }

        /// <summary>
        /// On file renamed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FswOnRenamed(object sender, RenamedEventArgs e)
        {
            var obj =
                WorkingItemsList.FirstOrDefault(
                    x => String.Equals(x.Filename, e.OldName, StringComparison.CurrentCultureIgnoreCase));

            if (obj == null) return;

            var fileInfo = new FileInfo(e.FullPath);

            obj.DisplayName = StringHelpers.TrimExtension(e.Name);
            obj.Filename = e.Name;
            obj.FullPath = e.FullPath;
            obj.DirectoryPath = fileInfo.DirectoryName;
        }

        #endregion

        #region Methods

        #region View Methods

        /// <summary>
        /// Forces the view to update
        /// </summary>
        public void UpdateListViewOnWorkingItemProcessed()
        {
            if (OnWorkingItemProcessedEvent == null) return;

            OnWorkingItemProcessedEvent();
        }

        /// <summary>
        /// Called when the user double clicks
        /// </summary>
        public void MouseDoubleClick()
        {
            if (SelectedWorkingItems == null || !SelectedWorkingItems.Any()) return;

            SelectedWorkingItems.Last().ShowWindLogDetails();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void OnFileDropped(object data)
        {
            var dataObject = (System.Windows.IDataObject)data;
            var items = (string[])dataObject.GetData(DataFormats.FileDrop);

            foreach (var item in items)
            {
                if (File.Exists(item)) // File, then unpack it
                {
                    // File info object
                    var fi = new FileInfo(item);

                    if (fi.Extension != MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension)
                    {
                        UserInput.ShowMessage("DROP_EIX_OR_DIR");
                        continue;
                    }

                    var workingItem = WorkingItemsList.FirstOrDefault(x => x.Filename == fi.Name);

                    // Create working item
                    if (workingItem == null)
                    {
                        workingItem = new WorkingItemVm(Path.GetFileNameWithoutExtension(fi.Name), fi.Name, fi.FullName,
                            fi.DirectoryName);

                        // Add it to the list
                        AddItemToWorkingList(workingItem);
                    }

                    // Unpack it
                    if (workingItem.UnpackFileCommand.CanExecute(null))
                        workingItem.UnpackFileCommand.Execute(null);
                }
                else if (Directory.Exists(item)) // Directory, then pack it
                {
                    // Dir info
                    var di = new DirectoryInfo(item);

                    var workingItem = WorkingItemsList.FirstOrDefault(x => x.DisplayName == di.Name);

                    // Create working item
                    if (workingItem == null)
                    {
                        workingItem = new WorkingItemVm(di.Name,
                            di.Name + MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension, di.FullName + MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension, di.FullName);

                        // Add it to the list
                        AddItemToWorkingList(workingItem);
                    }

                    // Pack it
                    if (workingItem.PackFileCommand.CanExecute(null))
                        workingItem.PackFileCommand.Execute(null);
                }
            }
        }

        #endregion

        #region Queue

        /// <summary>
        /// Called when an action is done (unpack or pack)
        /// </summary>
        /// <param name="processedItem"></param>
        public void ProcessQueueUponActionFinalization(WorkingItemVm processedItem)
        {
            // Force view to update
            UpdateListViewOnWorkingItemProcessed();

            // Decrement one on concurrent files being processed
            CcFiles--;

            // No items to be processed, return
            if (Queue.Count <= 0)
            {
                // Enable user to change profile
                MainWindowVm.Instance.CanChangeProfile = true;
                return;
            }

            // Get max files at a time
            var loopTop = Math.Max(ConstantsBase.MaxSimFiles - CcFiles, 0);

            // Loop through them
            for (int i = 0; i < loopTop; i++)
            {
                WorkingItemVm result;
                Queue.TryDequeue(out result);

                switch (result.QueueActionType)
                {
                    case WorkingItemVm.ActionType.Unpack:
                        if (result.UnpackFileCommand.CanExecute(null))
                            result.UnpackFileCommand.Execute(null);
                        break;
                    case WorkingItemVm.ActionType.Pack:
                        if (result.PackFileCommand.CanExecute(null))
                            result.PackFileCommand.Execute(null);
                        break;
                }
            }
        }

        #endregion

        #region Working List

        /// <summary>
        /// Adds item to the working items list
        /// </summary>
        /// <param name="item"></param>
        public void AddItemToWorkingList(WorkingItemVm item)
        {
            // Return if the item already exists
            if (WorkingItemsList.ToList().Find(x => x.DisplayName == item.DisplayName) != null) return;

            // Add it to and re-order list (TODO: change this, it's ugly and potentially slow)
            WorkingItemsList.Add(item);
            WorkingItemsList = new ObservableImmutableList<WorkingItemVm>(WorkingItemsList.OrderBy(x => x.DisplayName));
        }

        /// <summary>
        /// Processes profile's Working directory
        /// </summary>
        public void ProcessWorkingDirectory()
        {
            // Return if null or no profile is selected
            if (MainWindowVm.Instance == null || MainWindowVm.Instance.SelectedWorkingProfile == null) return;

            WorkingItemsList.Clear();

            try
            {
                // Loop through all files
                foreach (var item in IOHelper.GetAllFilesFromDir(MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory)
                    .Where(t => String.Equals(t.Extension, MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension, StringComparison.CurrentCultureIgnoreCase))
                    .Select(file => new WorkingItemVm(Path.GetFileNameWithoutExtension(file.Name), file.Name, file.FullName, file.DirectoryName)))
                {
                    // Add it to the list
                    AddItemToWorkingList(item);
                }

            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Error("DIR_NOT_FOUND", null, MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory);
            }

            try
            {
                // Loop through all the directories (since there may be files which are unpacked but not existing in the pack dir)
                foreach (var item in IOHelper.GetAllFirstDirectories(MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory))
                {
                    if (WorkingItemsList.FirstOrDefault(x => String.Equals(x.DisplayName, item.Name, StringComparison.CurrentCultureIgnoreCase)) == null)
                    {
                        AddItemToWorkingList(new WorkingItemVm(
                            item.Name,
                            item.Name + MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension,
                            item.FullName + MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension,
                            item.FullName));
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Error("DIR_NOT_FOUND", null, MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory);
            }

            WorkingItemsList = new ObservableImmutableList<WorkingItemVm>(WorkingItemsList.OrderBy(x => x.DisplayName));
        }

        #endregion

        #region Search Filter

        /// <summary>
        /// Filters item by string
        /// </summary>
        private void FilterItemsBySearchText()
        {
            if (!String.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in WorkingItemsList.Where(x => !x.DisplayName.ToLower().Contains(SearchText.ToLower())))
                    item.IsVisible = false;
            }
            else
            {
                foreach (var item in WorkingItemsList.Where(x => !x.IsVisible))
                    item.IsVisible = true;
            }
        }

        #endregion

        #endregion

        #region Commands

        #region Command Actions

        /// <summary>
        /// Action performed when RemoveItemsCommand is called
        /// </summary>
        private void RemoveItemsAction()
        {
            for (int i = 0; i < WorkingItemsList.Count; i++)
            {
                if (WorkingItemsList[i].IsSelected)
                {
                    WorkingItemsList.Remove(WorkingItemsList[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Action performed when PackSelectedItemsCommand is called
        /// </summary>
        private void PackSelectedItemsAction()
        {
            foreach (var workingItem in SelectedWorkingItems.OrderBy(x => x.DisplayName))
                if (workingItem.PackFileCommand.CanExecute(null))
                    workingItem.PackFileCommand.Execute(null);
        }

        /// <summary>
        /// Action performed when UnpackSelectedItemsCommand is called
        /// </summary>
        private void UnpackSelectedItemsAction()
        {
            foreach (var workingItem in SelectedWorkingItems.OrderBy(x => x.DisplayName))
                if (workingItem.UnpackFileCommand.CanExecute(null))
                    workingItem.UnpackFileCommand.Execute(null);
        }

        /// <summary>
        /// Action performed when SetAllItemsFilterCommand is called
        /// </summary>
        /// <param name="o"></param>
        private void SetAllItemsFilterAction(object o)
        {
            foreach (var workingItem in SelectedWorkingItems)
            {
                workingItem.SelectedFilter = Convert.ToInt32(o);
            }
        }

        /// <summary>
        /// Selects or deselects all files
        /// </summary>
        private void SelectOrDeselectAllAction()
        {
            SelectedWorkingItems.Clear();

            foreach (var workingItem in WorkingItemsList)
            {
                workingItem.IsSelected = !_areItemsSelected;
                SelectedWorkingItems.Add(workingItem);
            }
            _areItemsSelected = !_areItemsSelected;
        }

        #endregion

        #region Command Evaluators

        /// <summary>
        /// Evaluates wether it can execute UnpackSelectedCommand
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteUnpackSelected()
        {
            return SelectedWorkingItems.Count > 0;
        }

        #endregion

        #region Command Interfaces

        /// <summary>
        /// RemoveItemsCommand interface
        /// </summary>
        public ICommand RemoveItemsCommand
        {
            get { return _removeItems; }
        }

        /// <summary>
        /// PackSelectedItemsCommand interface
        /// </summary>
        public ICommand PackSelectedItemsCommand
        {
            get { return _packSelectedItems; }
        }

        /// <summary>
        /// UnpackSelectedItemsCommand interface
        /// </summary>
        public ICommand UnpackSelectedItemsCommand
        {
            get { return _unpackSelectedItems; }
        }

        /// <summary>
        /// SetAllITemsFilterCommand interface
        /// </summary>
        public ICommand SetAllItemsFilterCommand
        {
            get { return _setAllItemsFilter; }
        }

        /// <summary>
        /// SelectOrDeselectAllCommand interface
        /// </summary>
        public ICommand SelectOrDeselectAllCommand
        {
            get { return _selectOrDeselectAll; }
        }

        #endregion

        #endregion

        #region Properties

        #region Proxy Properties

        #endregion

        #region Presentation Members

        private int _ccFiles;

        /// <summary>
        /// Number of concurrent files being processed
        /// </summary>
        public int CcFiles
        {
            get { return _ccFiles; }
            set
            {
                if (_ccFiles != value)
                {
                    _ccFiles = value;
                    OnPropertyChanged("CcFiles");
                }
            }
        }

        private ConcurrentQueue<WorkingItemVm> _queue;

        /// <summary>
        /// Queue reference
        /// </summary>
        public ConcurrentQueue<WorkingItemVm> Queue
        {
            get { return _queue; }
            set
            {
                if (_queue != value)
                {
                    _queue = value;
                    OnPropertyChanged("Queue");
                }
            }
        }

        /// <summary>
        /// Max simultaneous files being processed
        /// </summary>
        public int MaxSimFiles
        {
            get { return ConstantsBase.MaxSimFiles; }
            set
            {
                if (ConstantsBase.MaxSimFiles != value)
                {
                    Properties.Settings.Default.MaxSimFiles = value;
                    ConstantsBase.MaxSimFiles = value;
                    OnPropertyChanged("MaxSimFiles");
                }
            }
        }
        
        private ObservableImmutableList<WorkingItemVm> _selectedWorkingItems;

        /// <summary>
        /// List of selected working items
        /// </summary>
        public ObservableImmutableList<WorkingItemVm> SelectedWorkingItems
        {
            get
            {
                var toReturn = new ObservableImmutableList<WorkingItemVm>();

                foreach (var item in _workingItems.Where(x => x.IsSelected))
                    toReturn.Add(item);

                return toReturn;
            }
            set
            {
                if (_selectedWorkingItems != value)
                {
                    _selectedWorkingItems = value;
                    OnPropertyChanged("SelectedWorkingItems");
                }
            }
        }

        private ObservableImmutableList<WorkingItemVm> _workingItems;

        /// <summary>
        /// List of working items
        /// </summary>
        public ObservableImmutableList<WorkingItemVm> WorkingItemsList
        {
            get { return _workingItems; }
            set
            {
                if (_workingItems != value)
                {
                    lock (Obj)
                    {
                        _workingItems = value;
                        OnPropertyChanged("WorkingItemsList");
                    }
                }
            }
        }

        private string _searchText;

        /// <summary>
        /// Search text used to filter items
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                SetProperty(ref _searchText, value, "SearchText");
                FilterItemsBySearchText();
            }
        }

        #endregion

        #region Others

        /// <summary>
        /// Static reference
        /// </summary>
        public static FilesActionVm Instance { get; set; }

        #endregion

        #endregion

        #region Destructor

        #endregion
    }
}
