using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EterManager.Base;
using EterManager.DataAccessLayer;
using EterManager.Exceptions.EterFiles;
using EterManager.Models;
using EterManager.UserInterface.Views;
using EterManager.UserInterface.Views.UserControls;
using EterManager.Utilities;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    class WorkingItemVm : ViewModelBase
    {
        #region Private Fields

        /// <summary>
        /// All Icons
        /// </summary>
        private const string BusyIcon = "pack://application:,,,/UserInterface/VisualResources/Images/processing.gif";
        private const string ReadyIcon = "pack://application:,,,/UserInterface/VisualResources/Images/ready.png";
        private const string ErrorReadyIcon = "pack://application:,,,/UserInterface/VisualResources/Images/errorReady.png";
        private const string QueueWaitingIcon = "pack://application:,,,/UserInterface/VisualResources/Images/queue.png";
        private const string CriticalErrorIcon = "pack://application:,,,/UserInterface/VisualResources/Images/remove.png";

        /// <summary>
        /// Counter used for all internal actions
        /// </summary>
        private int _successCounter;

        /// <summary>
        /// UnpackFileCommand RelayCommand obj
        /// </summary>
        private RelayCommand _unpackFile;

        /// <summary>
        /// PackFileCommand RelayCommand obj
        /// </summary>
        private RelayCommand _packFile;

        /// <summary>
        /// ShowIndexDetailsCommand RelayCommand obj
        /// </summary>
        private RelayCommand _showIndexDetails;

        #endregion

        #region Item States Enum

        /// <summary>
        /// All possible states of the current item
        /// </summary>
        [Flags]
        public enum State
        {
            Unpacking = 1,
            Packing = 2,
            Ready = 4,
            ReadyWithErrors = 8,
            CommitingIndexFile = 16,
            CommitingPackFile = 32,
            QueueWaiting = 64,
            CriticalError = 128,
            LongAction = 256
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Main Ctor
        /// </summary>
        public WorkingItemVm(string displayName, string fileName, string fullPath, string directoryPath)
        {
            // Initialize members
            SetItemState(State.Ready);
            ActionProgress = 0;
            ErrorList = new ObservableImmutableList<ErrorItem>();
            HashMismatchFiles = new ObservableImmutableList<string>();
            IsVisible = true;
            DisplayName = displayName;
            Filename = fileName;
            FullPath = fullPath;
            DirectoryPath = directoryPath;

            // Initialize commands
            InstantiateCommands();

            //SelectedFilter = WorkingListVM.Instance.FilterList[0];
        }

        /// <summary>
        /// Initializes all commands
        /// </summary>
        private void InstantiateCommands()
        {
            _unpackFile = new RelayCommand(UnpackFileAction, param => CanExecuteUnpackCommand());
            _packFile = new RelayCommand(PackFileAction, param => CanExecutePackCommand());
            _showIndexDetails = new RelayCommand(param => ShowIndexDetailsAction(), param => CanExecuteShowDetailsCommand());
        }

        #endregion

        #region Methods

        #region Item State

        /// <summary>
        /// Sets item to one of the available states
        /// </summary>
        /// <param name="newState"></param>
        private void SetItemState(State newState)
        {
            ItemState = newState;

            if ((ItemState & State.Ready) == State.Ready)
            {
                Icon = ReadyIcon;
                ActionLabel = "Ready";
                StateTooltip = "Ready to take action";
            }
            else if ((ItemState & State.ReadyWithErrors) == State.ReadyWithErrors)
            {
                Icon = ErrorReadyIcon;
                ActionLabel = "Ready";
                StateTooltip = "Double click for details!";
            }
            else if ((ItemState & State.Unpacking) == State.Unpacking)
            {
                Icon = BusyIcon;
                ActionLabel = "Unpacking...";
                StateTooltip = "Working...";
            }
            else if ((ItemState & State.Packing) == State.Packing)
            {
                Icon = BusyIcon;
                ActionLabel = "Packing...";
                StateTooltip = "Working...";
            }
            else if ((ItemState & State.CommitingIndexFile) == State.CommitingIndexFile)
            {
                Icon = BusyIcon;
                ActionLabel = "Commiting index file...";
                StateTooltip = "Moving file...";
            }
            else if ((ItemState & State.CommitingPackFile) == State.CommitingPackFile)
            {
                Icon = BusyIcon;
                ActionLabel = "Commiting pack file...";
                StateTooltip = "Moving file...";
            }
            else if ((ItemState & State.QueueWaiting) == State.QueueWaiting)
            {
                Icon = QueueWaitingIcon;
                ActionLabel = "Queue...";
                StateTooltip = "Placed in queue, waiting to be processed...";
            }
            else if ((ItemState & State.CriticalError) == State.CriticalError)
            {
                Icon = CriticalErrorIcon;
                ActionLabel = "Critical error!";
                StateTooltip = "An error preventing the action to be perfomed has occured, check the Issues window below (there may be more info the errorLog.txt file).";
            }

        }

        /// <summary>
        /// 0 = unpack, 1 = pack, 2 = commit
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool IsItemReady()
        {
            if (ItemState != State.Ready && ItemState != State.ReadyWithErrors && ItemState != State.QueueWaiting && ItemState != State.CriticalError)
            {
                // If is file being processed already, ignore
                //Logger.LogOutputMessage(Log.LogId.FILE_BEING_PROCESSED_ALREADY, args: this.DisplayName);
                return false;
            }

            if (FilesActionVm.Instance.CcFiles >= ConstantsBase.MaxSimFiles)
            {
                EnqueueItem();
                return false;
            }

            //if (!NewMainVM.Instance.Stopwatch.IsRunning)
            //    NewMainVM.Instance.Stopwatch.Restart();

            return true;
        }

        #endregion

        #region Window Details

        /// <summary>
        /// Updates the Index view window content
        /// </summary>
        private void UpdateIndexList()
        {
            if (_indexDetails == null)
                _indexDetails = new ObservableImmutableList<IndexItem>();

            _indexDetails.Clear();

            var tempList = EterFilesDal.ReadIndexFile(
                Path.Combine(MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory, Filename),
                MainWindowVm.Instance.SelectedWorkingProfile.IndexKey,
                DisplayName);

            foreach (var item in tempList)
            {
                _indexDetails.Add(item);
            }

            NumberOfFiles = IndexDetails.Count;
        }

        /// <summary>
        /// Updates both the index and pack file sizes
        /// </summary>
        public void UpdateFileSizes()
        {
            // Lazy way of doing it...
            try
            {
                var s = String.Format("{0}{1}", MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory,
                    EterHelper.ReplaceWithEixExt(Filename));
                SizeOfIndexFile = new FileInfo(s).Length;
            }
            catch (Exception)
            {
                SizeOfIndexFile = 0;
            }

            try
            {
                SizeOfPackFile = new FileInfo(String.Format("{0}{1}", MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory,
                    EterHelper.ReplaceWithEpkExt(Filename))).Length;
            }
            catch (Exception)
            {
                SizeOfIndexFile = 0;
            }
        }

        #endregion

        #region Log Window

        /// <summary>
        /// Shows the Window Log Details
        /// </summary>
        public void ShowWindLogDetails()
        {
            var wnd = ViewManager.ShowWindow<LogDetailsWindow>(false, String.Format("{0} - Log details", Filename), true);

            wnd.DataContext = this;
        }

        #endregion

        #region Queue

        /// <summary>
        /// Adds item to queue
        /// </summary>
        public void EnqueueItem()
        {
            if (FilesActionVm.Instance.Queue.FirstOrDefault(x => x.DisplayName == DisplayName) != null)
                return;

            FilesActionVm.Instance.Queue.Enqueue(this);
            SetItemState(State.QueueWaiting);
        }

        /// <summary>
        /// Called whenever a "long action" is performed (pack/unpack)
        /// </summary>
        /// <param name="param"></param>
        private void AfterLongAction(object param)
        {
            UpdateFileSizes();
            FilesActionVm.Instance.ProcessQueueUponActionFinalization(this);
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #endregion

        #region Commands

        #region Command Evaluators

        /// <summary>
        /// Evaluates wether UnpackCommand can be executed
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteUnpackCommand()
        {
            return MainWindowVm.Instance.SelectedWorkingProfile != null && File.Exists(Path.Combine(MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory, Filename));
        }

        /// <summary>
        /// Evaluates wether PackCommand can be executed
        /// </summary>
        /// <returns></returns>
        private bool CanExecutePackCommand()
        {
            return MainWindowVm.Instance.SelectedWorkingProfile != null && Directory.Exists((String.Format("{0}{1}", MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory, DisplayName)));
        }

        /// <summary>
        /// Evaluates wether ShowDetails can be executed
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteShowDetailsCommand()
        {
            return MainWindowVm.Instance.SelectedWorkingProfile != null && File.Exists(Path.Combine(MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory, Filename));
        }

        #endregion

        #region Command Actions

        /// <summary>
        /// Action performed when Unpack file is hit
        /// </summary>
        /// <param name="param"></param>
        private async void UnpackFileAction(object param)
        {
            QueueActionType = ActionType.Unpack;

            if (!IsItemReady())
                return;

            FilesActionVm.Instance.CcFiles++;
            SetItemState(State.Unpacking | State.LongAction);

            await Task.Run(() =>
                {
                    // Set state to unpacking
                    SetItemState(State.Unpacking);

                    // Clear lists
                    HashMismatchFiles.Clear();
                    ErrorList.Clear();

                    // Reset counters
                    _successCounter = 0;
                    double lastProgressValue = 0;

                    try
                    {
                        // Unpack file
                        EterFilesDal.UnpackFile(
                            new FileInfo(EterHelper.ReplaceWithEpkExt(Path.Combine(MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory, Filename))),
                            MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory,
                            MainWindowVm.Instance.SelectedWorkingProfile.IndexKey,
                            MainWindowVm.Instance.SelectedWorkingProfile.PackKey,
                            (operationResult, globalProgress) =>
                            {
                                if ((globalProgress - lastProgressValue) >= 5)
                                {
                                    ActionProgress = globalProgress;
                                    lastProgressValue = globalProgress;
                                }

                                if (operationResult == 0)
                                    _successCounter++;
                            },
                            (error, hash) =>
                            {
                                if (error != null)
                                    ErrorList.Add(error);

                                if (!String.IsNullOrWhiteSpace(hash))
                                    HashMismatchFiles.Add(hash);
                            });
                    }
                    catch (ErrorReadingIndexException ex)
                    {
                        Logger.Error("ETER_WRONG_INDEX_KEY", DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (OutOfMemoryException ex)
                    {
                        Logger.Warning("FILE_TOO_BIG", DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (EterPackFileNotFoundException ex)
                    {
                        Logger.Error("ETER_EPK_FILE_NOT_FOUND", DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Logger.Error("FILE_NOT_FOUND", DisplayName, new object[] {ex.FileName});
                        SetItemState(State.CriticalError);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Logger.Error("COULD_NOT_ACCESS_FILE", DisplayName, DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (System.IO.IOException ex)
                    {
                        Logger.Error("ERROR_WITH_CUSTOM_MSG", DisplayName, ex.Message);
                        SetItemState(State.CriticalError);
                    }
                });

            // If any file produced an error, set state accordingly
            if (ErrorList.Count > 0 && ItemState != State.CriticalError)
                SetItemState(State.ReadyWithErrors);
            else if (ErrorList.Count == 0 && ItemState != State.CriticalError)
                SetItemState(State.Ready);

            // Make sure progress bar is at 100%
            ActionProgress = 100;

            // Logging stuff
            if (ItemState != State.CriticalError)
            {
                // Get all failed items
                int failedCount = ErrorList.Count;

                // Were any unnamed files present?
                var item = ErrorList.FirstOrDefault(x => x.ErrorMotive.Contains("(no name)"));

                // If so, add it
                if (item != null)
                {
                    failedCount += Convert.ToInt32(item.Arg) - 1;
                }

                Logger.Information("ETER_UNPACK_RESULT", DisplayName, _successCounter, HashMismatchFiles.Count, failedCount);
            }
                

            AfterLongAction(param);
        }

        /// <summary>
        /// Action performed when Pack file is hit
        /// </summary>
        /// <param name="param"></param>
        private async void PackFileAction(object param)
        {
            if (SelectedFilter < 0)
            {
                UserInput.ShowMessage("USER_SELECT_PACK_TYPE");
                return;
            }

            QueueActionType = ActionType.Pack;

            if (!IsItemReady())
                return;

            FilesActionVm.Instance.CcFiles++;
            SetItemState(State.Packing | State.LongAction);
            //await Task.Delay(1000);
            //this.SetItemState(State.Ready);
            //WorkingListVM.Instance.ProcessQueueUponActionFinalization(this);
            //return;

            HashMismatchFiles.Clear();
            ErrorList.Clear();
            _successCounter = 0;

            var filesInDir = IOHelper.GetAllFilesFromDir(String.Format("{0}{1}", MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory, DisplayName));
            var indexItems = new List<IndexItem>();

            string indexFilePath = String.Format("{0}{1}",
                    MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory,
                    String.Format("{0}{1}",
                        DisplayName,
                        MainWindowVm.Instance.SelectedWorkingProfile.IndexExtension));

            string packFilePath = String.Format("{0}{1}",
                    MainWindowVm.Instance.SelectedWorkingProfile.WorkingDirectory,
                    String.Format("{0}{1}",
                        DisplayName,
                        MainWindowVm.Instance.SelectedWorkingProfile.PackExtension));

            if (File.Exists(packFilePath))
            {
                // File already packed, but will be overwritten
                Logger.Warning("FILE_ALREADY_PACKED_BUT_OVER", Filename);
                File.Delete(packFilePath);
            }

            if (File.Exists(indexFilePath))
                File.Delete(EterHelper.ReplaceWithEpkExt(indexFilePath));

            await Task.Run(() =>
                {
                    int counter = 0;
                    foreach (var file in filesInDir)
                    {
                        //int type = -1;

                        //string fileExtension = Path.GetExtension(file.FullName);

                        //if (SelectedFilter.RawExtensions != null)
                        //{
                        //    foreach (var rawExt in SelectedFilter.RawExtensions)
                        //        if (rawExt.ToLower() == fileExtension.ToLower())
                        //            type = 0;

                        //    foreach (var lzoExt in SelectedFilter.LzoExtensions)
                        //        if (lzoExt.ToLower() == fileExtension.ToLower())
                        //            type = 1;

                        //    foreach (var xteaExt in SelectedFilter.XteaExtensions)
                        //        if (xteaExt.ToLower() == fileExtension.ToLower())
                        //            type = 2;
                        //}

                        //if (type == -1)
                        //    type = SelectedFilter.NotIncludedExtensionsType;

                        string toReplaceStr = String.Format("{0}{1}", MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory, DisplayName).Replace("/", "\\");

                        string fileName = file.FullName.Substring(file.FullName.IndexOf(toReplaceStr) + toReplaceStr.Length + 1);

                        indexItems.Add(new IndexItem(
                            counter,
                            fileName,
                            null,
                            0,
                            0,
                            null,
                            0,
                            SelectedFilter,
                            DisplayName));
                        counter++;
                    }

                    double lastProgressValue = 0;

                    try
                    {
                        EterFilesDal.BuildIndexAndPackFiles(
                            indexItems,
                            packFilePath,
                            String.Format("{0}{1}\\",
                                MainWindowVm.Instance.SelectedWorkingProfile.UnpackDirectory,
                                DisplayName),
                            MainWindowVm.Instance.SelectedWorkingProfile.IndexKey,
                            MainWindowVm.Instance.SelectedWorkingProfile.PackKey,
                            (error) =>
                            {
                                if (error != null)
                                    ErrorList.Add(error);
                            },
                            (result, progress) =>
                            {
                                if (result == 0)
                                    _successCounter++;

                                if (((progress - lastProgressValue) >= 5))
                                {
                                    ActionProgress = progress;
                                    lastProgressValue = progress;
                                }
                            },
                            () => SetItemState(State.CriticalError));
                    }
                    catch (OutOfMemoryException ex)
                    {
                        Logger.Warning("FILE_TOO_BIG", DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (EterPackFileNotFoundException ex)
                    {
                        Logger.Error("ETER_EPK_FILE_NOT_FOUND", DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Logger.Error("FILE_NOT_FOUND", DisplayName, new object[] {ex.FileName});
                        SetItemState(State.CriticalError);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Logger.Error("COULD_NOT_ACCESS_FILE", DisplayName, DisplayName);
                        SetItemState(State.CriticalError);
                    }
                    catch (System.IO.IOException ex)
                    {
                        Logger.Error("ERROR_WITH_CUSTOM_MSG", DisplayName, ex.Message);
                        SetItemState(State.CriticalError);
                    }
                });

            SetItemState(ErrorList.Count > 0 ? State.ReadyWithErrors : State.Ready);

            ActionProgress = 100;
            Logger.Information("ETER_PACK_RESULT", DisplayName, new object[] { _successCounter, HashMismatchFiles.Count, ErrorList.Count });
            AfterLongAction(param);
        }

        /// <summary>
        /// Action performed when ShowIndexDetails is hit
        /// </summary>
        private void ShowIndexDetailsAction()
        {
            var wnd = ViewManager.ShowWindow<IndexDetailsWindow>(false, String.Format("{0} - Index details", Filename), true);

            UpdateIndexList();
            UpdateFileSizes();
            wnd.DataContext = this;
        }

        #endregion

        #region Command Interfaces

        /// <summary>
        /// Command interface for ShowIndexDetailsWindow
        /// </summary>
        public ICommand ShowIndexDetailsCommand
        {
            get { return _showIndexDetails; }
        }

        /// <summary>
        /// Command interface for PackFileCommand
        /// </summary>
        public ICommand PackFileCommand
        {
            get { return _packFile; }
        }

        /// <summary>
        /// Command interface for UnpackFileCommand
        /// </summary>
        public ICommand UnpackFileCommand
        {
            get { return _unpackFile; }
        }

        #endregion

        #endregion

        #region Proxy Properties

        private State _itemState;

        /// <summary>
        /// Item state property
        /// </summary>
        public State ItemState
        {
            get { return _itemState; }
            set
            {
                if (_itemState != value)
                {
                    _itemState = value;
                    OnPropertyChanged("ItemState");
                }
            }
        }

        private string _displayName;

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value, "DisplayName"); }
        }

        private string _fileName;

        /// <summary>
        /// The actual file name
        /// </summary>
        public string Filename
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value, "Filename"); }
        }

        private string _fullPath;

        /// <summary>
        /// The actual file full path
        /// </summary>
        public string FullPath
        {
            get { return _fullPath; }
            set { SetProperty(ref _fullPath, value, "FullPath"); }
        }

        private string _directoryPath;

        /// <summary>
        /// File's directory path
        /// </summary>
        public string DirectoryPath
        {
            get { return _directoryPath; }
            set { SetProperty(ref _directoryPath, value, "DirectoryPath"); }
        }

        private int _selectedFilter;

        /// <summary>
        /// Selected filter to pack with
        /// </summary>
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { SetProperty(ref _selectedFilter, value, "SelectedFilter"); }
        }

        #endregion

        #region Presentation Members

        #region Index Details Window

        private ObservableImmutableList<IndexItem> _indexDetails;

        /// <summary>
        /// IndexItems list
        /// </summary>
        public ObservableImmutableList<IndexItem> IndexDetails
        {
            get { return _indexDetails; }
            set
            {
                if (_indexDetails != value)
                {
                    _indexDetails = value;
                    OnPropertyChanged("IndexDetails");
                }
            }
        }

        private int _numberOfFiles;

        /// <summary>
        /// Total number of files
        /// </summary>
        public int NumberOfFiles
        {
            get { return IndexDetails.Count; }
            set { SetProperty(ref _numberOfFiles, value, "NumberOfFiles"); }
        }

        private long _sizeOfIndexFile;

        /// <summary>
        /// Size of EIX file
        /// </summary>
        public long SizeOfIndexFile
        {
            get { return _sizeOfIndexFile; }
            set { SetProperty(ref _sizeOfIndexFile, value, "SizeOfIndexFile"); }
        }

        private long _sizeOfPackFile;

        /// <summary>
        /// Size of EPK file
        /// </summary>
        public long SizeOfPackFile
        {
            get { return _sizeOfPackFile; }
            set { SetProperty(ref _sizeOfPackFile, value, "SizeOfPackFile"); }
        }

        #endregion

        #region Window Log Details

        private ObservableImmutableList<ErrorItem> _errorList;

        /// <summary>
        /// List of files which failed to be processed
        /// </summary>
        public ObservableImmutableList<ErrorItem> ErrorList
        {
            get { return _errorList; }
            set
            {
                if (_errorList != value)
                {
                    _errorList = value;
                    OnPropertyChanged("ErrorList");
                }
            }
        }

        private ObservableImmutableList<string> _hashMisthmatchFiles;

        /// <summary>
        /// List of files whose checksum did not match
        /// </summary>
        public ObservableImmutableList<string> HashMismatchFiles
        {
            get { return _hashMisthmatchFiles; }
            set
            {
                if (_hashMisthmatchFiles != value)
                {
                    _hashMisthmatchFiles = value;
                    OnPropertyChanged("HashMismatchFiles");
                }
            }
        }

        #endregion

        #region Main View

        private string _stateTooltip;

        /// <summary>
        /// Tooltip shown in the state icon
        /// </summary>
        public string StateTooltip
        {
            get { return _stateTooltip; }
            set
            {
                if (_stateTooltip != value)
                {
                    _stateTooltip = value;
                    OnPropertyChanged("StateTooltip");
                }
            }
        }

        private string _actionLabel;

        /// <summary>
        /// Current action label text
        /// </summary>
        public string ActionLabel
        {
            get { return _actionLabel; }
            set
            {
                if (_actionLabel != value)
                {
                    _actionLabel = value;
                    OnPropertyChanged("ActionLabel");
                }
            }
        }

        private double _actionProgress;

        /// <summary>
        /// Current action progress
        /// </summary>
        public double ActionProgress
        {
            get { return _actionProgress; }
            set
            {
                if (_actionProgress != value)
                {
                    _actionProgress = value;
                    OnPropertyChanged("ActionProgress");
                }
            }
        }

        private string _icon;

        /// <summary>
        /// Icon's path
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged("Icon");
                }
            }
        }

        private bool _isSelected;

        /// <summary>
        /// Sets wether the item is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        private bool _isVisible;

        /// <summary>
        /// Defines wether the item should be visible
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value, "IsVisible"); }
        }
        
        #endregion

        #endregion

        #region Other

        /// <summary>
        /// Action type
        /// </summary>
        public ActionType QueueActionType { get; set; }

        public enum ActionType
        {
            Unpack,
            Pack,
            Commit
        }

        #endregion
    }
}
