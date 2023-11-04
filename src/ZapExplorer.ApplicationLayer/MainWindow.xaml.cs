using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZapExplorer.ApplicationLayer.Windows;
using ZapExplorer.BusinessLayer;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ZapFileService _zapFileService;
        private readonly AddFileService _addFileService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private DirectoryItem? _currentDirectory;

        public DirectoryItem? CurrentDirectory
        {
            get { return _currentDirectory; }
            set { _currentDirectory = value; OnPropertyChanged(nameof(CurrentDirectory)); }
        }

        private ZapArchive? _zapArchive;

        public ZapArchive? ZapArchive
        {
            get { return _zapArchive; }
            set { _zapArchive = value; OnPropertyChanged(nameof(ZapArchive)); }
        }

        private ObservableCollection<DirectoryItem> _breadCrumbsBar;

        public ObservableCollection<DirectoryItem> BreadCrumbsBar
        {
            get { return _breadCrumbsBar; }
            set { _breadCrumbsBar = value; OnPropertyChanged(nameof(BreadCrumbsBar)); }
        }



        public MainWindow()
        {
            bool exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (!exists)
            {
                DirectoryInfo dir = new DirectoryInfo(AddFileService.FILES_PATH);
                if (dir.Exists)
                {
                    dir.Delete(true);
                }
            }
            InitializeComponent();
            DataContext = this;
            _zapFileService = new ZapFileService();
            _addFileService = new AddFileService();
            BreadCrumbsBar = new ObservableCollection<DirectoryItem>();
        }

        private void NewFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = MessageBox.Show("There are unsaved changes, Discard changes?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            Title = "ZapExplorer";
            NewFileWindow newFileWindow = new NewFileWindow();
            newFileWindow.ShowDialog();
            if (newFileWindow.Confirmed)
            {
                Title = "New File";
                _addFileService.RefreshFolder();
                ZapArchive = new ZapArchive();
                ZapArchive.PaddingSize = newFileWindow.PaddingSize;
                CurrentDirectory = null;
                BreadCrumbsBar.Clear();
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = MessageBox.Show("There are unsaved changes, Discard changes?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            _addFileService.RefreshFolder();
            ZapArchive = null;
            CurrentDirectory = null;
            BreadCrumbsBar.Clear();

            Title = "ZapExplorer";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select zap archive";
            openFileDialog.DefaultExt = "zap";
            openFileDialog.Filter = "zap archives (*.zap)|*zap";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            if(openFileDialog.ShowDialog() == true)
            {
                ZapArchive = _zapFileService.GetArchive(openFileDialog.FileName);
                FileInfo fi = new FileInfo(openFileDialog.FileName);
                Title = fi.Name;
            }
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to add";
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    if(CurrentDirectory == null)
                    {
                        ZapArchive.Items.Add(_addFileService.AddFile(file));
                    }
                    else
                    {
                        CurrentDirectory.Items.Add(_addFileService.AddFile(file));
                    }
                }
                ZapArchive.SortItems();
            }
        }
        private void CreateFolder(object sender, RoutedEventArgs e)
        {
            CreateFolderWindow createFolderWindow = new CreateFolderWindow();
            createFolderWindow.ShowDialog();
            if(createFolderWindow.Confirmed)
            {
                DirectoryItem directoryItem = new DirectoryItem(createFolderWindow.FolderName);
                if (CurrentDirectory == null)
                {
                    ZapArchive.Items.Add(directoryItem);
                }
                else
                {
                    CurrentDirectory.Items.Add(directoryItem);
                }
                ZapArchive.SortItems();
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if(_addFileService.UnsavedProgress)
            {
                var result = MessageBox.Show("There are unsaved changes, Do you want to quit?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _addFileService.PurgeFolder();
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            _zapFileService.SaveArchive(ZapArchive, ZapArchive.Origin);
            _addFileService.RefreshFolder();
        }

        private void SaveFileAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save zap archive";
            saveFileDialog.DefaultExt = "zap";
            saveFileDialog.Filter = "zap archives (*.zap)|*zap";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                FileInfo fi = new FileInfo(saveFileDialog.FileName);
                ZapArchive.Origin = saveFileDialog.FileName;
                OnPropertyChanged(nameof(ZapArchive));
                Title = fi.Name;
                _zapFileService.SaveArchive(ZapArchive, saveFileDialog.FileName);
                _addFileService.RefreshFolder();
            }
        }

        private void Return(object sender, RoutedEventArgs e)
        {
            if(CurrentDirectory == null)
                return;
            BreadCrumbsBar.Remove(BreadCrumbsBar.Last());
            if(BreadCrumbsBar.Count == 0)
            {
                CurrentDirectory = null;
                return;
            }
            CurrentDirectory = BreadCrumbsBar.Last();
        }
        private void ChangeDirectory(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button.Tag is DirectoryItem)
            {
                CurrentDirectory = (DirectoryItem)button.Tag;
                int directoryPos = BreadCrumbsBar.IndexOf(CurrentDirectory);
                if((DirectoryItem)button.Tag == BreadCrumbsBar.Last())
                    return;
                for(int i = 0; i < BreadCrumbsBar.Count - directoryPos; i++)
                {
                    BreadCrumbsBar.Remove(BreadCrumbsBar.Last());
                }
            }
            else
            {
                CurrentDirectory = null;
                BreadCrumbsBar.Clear();
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            ZapArchive.Items.Remove((Item)((MenuItem)sender).Tag);
            _addFileService.UnsavedProgress = true;
        }

        private void ExportItem(object sender, RoutedEventArgs e)
        {
            FileItem fileItem = (FileItem)((MenuItem)sender).Tag;

            FileInfo fi = new FileInfo(fileItem.Name);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export file";
            saveFileDialog.DefaultExt = fi.Extension;
            saveFileDialog.FileName = fi.Name;
            saveFileDialog.Filter = "All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                _zapFileService.ExportItem(fileItem, saveFileDialog.FileName);
            }
        }

        private void ExportArchive(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vistaFolderBrowserDialog = new VistaFolderBrowserDialog();
            vistaFolderBrowserDialog.Description = "Select folder";
            vistaFolderBrowserDialog.ShowNewFolderButton = true;

            if (vistaFolderBrowserDialog.ShowDialog() == true)
            {
                _zapFileService.ExportArchive(ZapArchive, vistaFolderBrowserDialog.SelectedPath + @"\");
            }
        }

        private void OnItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            MouseEventArgs mouseEventArgs = (MouseEventArgs)e;
            if (mouseEventArgs.LeftButton != MouseButtonState.Pressed)
                return;
            ListBox? listBox = sender as ListBox;
            if(listBox.SelectedItem is DirectoryItem)
            {
                CurrentDirectory = (DirectoryItem)listBox.SelectedItem;
                BreadCrumbsBar.Add(CurrentDirectory);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
