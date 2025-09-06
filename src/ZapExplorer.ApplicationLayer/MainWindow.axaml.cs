using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Ursa.Controls;
using ZapExplorer.ApplicationLayer.Windows;
using ZapExplorer.BusinessLayer;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer;

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
    
    private Item? _selectedItem;

    public Item? SelectedItem
    {
        get { return _selectedItem; }
        set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
    }

    private ObservableCollection<DirectoryItem> _breadCrumbsBar;

    public ObservableCollection<DirectoryItem> BreadCrumbsBar
    {
        get { return _breadCrumbsBar; }
        set { _breadCrumbsBar = value; OnPropertyChanged(nameof(BreadCrumbsBar)); }
    }
    
    public MainWindow()
    {
        bool exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1;
        if (!exists)
        {
            DirectoryInfo dir = new DirectoryInfo(AddFileService.FILES_PATH);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        } ;
        InitializeComponent();
        DataContext = this;
        
        _zapFileService = new ZapFileService();
        _addFileService = new AddFileService();
        BreadCrumbsBar = new ObservableCollection<DirectoryItem>();
    }
    
    private async void NewFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = await MessageBox.ShowAsync("There are unsaved changes, Discard changes?", "Warning", MessageBoxIcon.None, MessageBoxButton.YesNo);
                if (!(result == MessageBoxResult.OK))
                {
                    return;
                }
            }
            Title = "ZapExplorer";
            NewFileWindow newFileWindow = new NewFileWindow();
            await newFileWindow.ShowDialog(this);
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

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = await MessageBox.ShowAsync("There are unsaved changes, Discard changes?", "Warning", MessageBoxIcon.None,MessageBoxButton.YesNo);
                if (!(result == MessageBoxResult.OK))
                {
                    return;
                }
            }

            _addFileService.RefreshFolder();
            ZapArchive = null;
            CurrentDirectory = null;
            BreadCrumbsBar.Clear();

            Title = "ZapExplorer";
            
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select zap archive",
                AllowMultiple = false,
                FileTypeFilter = new [] {Types.Types.Zap}
            });

            if (files.Count >= 1)
            {
                ZapArchive = _zapFileService.GetArchive(Uri.UnescapeDataString(files.First().Path.AbsolutePath));
                FileInfo fi = new FileInfo(Uri.UnescapeDataString(files.First().Path.AbsolutePath));
                Title = fi.Name;
            }
        }

        private async void AddFile(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to add",
                AllowMultiple = true
            });

            if (files.Count >= 1)
            {
                foreach (var file in files)
                {
                    if(CurrentDirectory == null)
                    {
                        ZapArchive.Items.Add(_addFileService.AddFile(Uri.UnescapeDataString(file.Path.AbsolutePath)));
                    }
                    else
                    {
                        CurrentDirectory.Items.Add(_addFileService.AddFile(Uri.UnescapeDataString(file.Path.AbsolutePath)));
                    }
                }
            }
        }
        private async void CreateFolder(object sender, RoutedEventArgs e)
        {
            CreateFolderWindow createFolderWindow = new CreateFolderWindow();
            await createFolderWindow.ShowDialog(this);
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

        private async void WindowClosing(object sender, CancelEventArgs e)
        {
            if(_addFileService.UnsavedProgress)
            {
                var result = await MessageBox.ShowAsync("There are unsaved changes, Do you want to quit?", "Warning", MessageBoxIcon.None, MessageBoxButton.YesNo);
                if (!(result == MessageBoxResult.OK))
                {
                    e.Cancel = true;
                    return;
                }
            }
            _addFileService.PurgeFolder();
        }

        private async void SaveFile(object sender, RoutedEventArgs e)
        {
            _zapFileService.SaveArchive(ZapArchive, ZapArchive.Origin);
            _addFileService.RefreshFolder();
            await MessageBox.ShowAsync("Saving Complete");
        }

        private async void SaveFileAs(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save zap archive",
                DefaultExtension = "zap",
                FileTypeChoices = new [] {Types.Types.Zap}
            });

            if (file != null)
            {
                FileInfo fi = new FileInfo(Uri.UnescapeDataString(file.Path.AbsolutePath));
                ZapArchive.Origin = Uri.UnescapeDataString(file.Path.AbsolutePath);
                OnPropertyChanged(nameof(ZapArchive));
                Title = fi.Name;
                _zapFileService.SaveArchive(ZapArchive, Uri.UnescapeDataString(file.Path.AbsolutePath));
                _addFileService.RefreshFolder();
                await MessageBox.ShowAsync("Saving Complete");
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

            CurrentDirectory = null;
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
            if(CurrentDirectory == null)
                ZapArchive.Items.Remove((Item)((MenuItem)sender).Tag);
            else
                CurrentDirectory.Items.Remove((Item)((MenuItem)sender).Tag);
            _addFileService.UnsavedProgress = true;
        }

        private async void ExportItem(object sender, RoutedEventArgs e)
        {
            FileItem fileItem = (FileItem)((MenuItem)sender).Tag;

            FileInfo fi = new FileInfo(fileItem.Name);
            
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export file",
                DefaultExtension = fi.Extension,
                FileTypeChoices = new [] {Types.Types.Zap}
            });

            if (file != null)
            {
                _zapFileService.ExportItem(fileItem, Uri.UnescapeDataString(file.Path.AbsolutePath));
            }
        }

        private async void ExportArchive(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            });

            if (folder != null)
            {
                _zapFileService.ExportArchive(ZapArchive, Uri.UnescapeDataString(folder.First().Path.AbsolutePath));
            }
        }


        private void OnItemMouseDoubleClick(object? sender, TappedEventArgs e)
        {
            if (!e.Pointer.IsPrimary)
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