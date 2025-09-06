using System;
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
using ZapExplorer.ApplicationLayer.ViewModels;
using ZapExplorer.ApplicationLayer.Windows;
using ZapExplorer.BusinessLayer;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly ZapFileService _zapFileService;
    private readonly AddFileService _addFileService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
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
        };
        
        _mainWindowViewModel = new MainWindowViewModel();
        InitializeComponent();
        DataContext = _mainWindowViewModel;
        
        _zapFileService = new ZapFileService();
        _addFileService = new AddFileService();
    }
    
    private async void NewFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = await MessageBox.ShowAsync("There are unsaved changes, Discard changes?", "Warning", MessageBoxIcon.Warning, MessageBoxButton.YesNo);
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
                _mainWindowViewModel.ZapArchive = new ZapArchive();
                _mainWindowViewModel.ZapArchive.PaddingSize = newFileWindow.PaddingSize;
                _mainWindowViewModel.CurrentDirectory = null;
                _mainWindowViewModel.BreadCrumbsBar.Clear();
            }
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            if (_addFileService.UnsavedProgress)
            {
                var result = await MessageBox.ShowAsync("There are unsaved changes, Discard changes?", "Warning", MessageBoxIcon.Warning,MessageBoxButton.YesNo);
                if (!(result == MessageBoxResult.OK))
                {
                    return;
                }
            }

            _addFileService.RefreshFolder();
            _mainWindowViewModel.ZapArchive = null;
            _mainWindowViewModel.CurrentDirectory = null;
            _mainWindowViewModel.BreadCrumbsBar.Clear();

            Title = "ZapExplorer";
            
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select zap archive",
                AllowMultiple = false,
                FileTypeFilter = new [] {Types.Types.Zap},
            });

            if (files.Count >= 1)
            {
                _mainWindowViewModel.ZapArchive = _zapFileService.GetArchive(Uri.UnescapeDataString(files.First().Path.AbsolutePath));
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
                    if(_mainWindowViewModel.CurrentDirectory == null)
                    {
                        _mainWindowViewModel.ZapArchive.Items.Add(_addFileService.AddFile(Uri.UnescapeDataString(file.Path.AbsolutePath)));
                    }
                    else
                    {
                        _mainWindowViewModel.CurrentDirectory.Items.Add(_addFileService.AddFile(Uri.UnescapeDataString(file.Path.AbsolutePath)));
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
                if (_mainWindowViewModel.CurrentDirectory == null)
                {
                    _mainWindowViewModel.ZapArchive.Items.Add(directoryItem);
                }
                else
                {
                    _mainWindowViewModel.CurrentDirectory.Items.Add(directoryItem);
                }
                _mainWindowViewModel.ZapArchive.SortItems();
            }
        }
        
        private async void SaveFile(object sender, RoutedEventArgs e)
        {
            _zapFileService.SaveArchive(_mainWindowViewModel.ZapArchive, _mainWindowViewModel.ZapArchive.Origin);
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
                _mainWindowViewModel.ZapArchive.Origin = Uri.UnescapeDataString(file.Path.AbsolutePath);
                OnPropertyChanged(nameof(_mainWindowViewModel.ZapArchive));
                Title = fi.Name;
                _zapFileService.SaveArchive(_mainWindowViewModel.ZapArchive, Uri.UnescapeDataString(file.Path.AbsolutePath));
                _addFileService.RefreshFolder();
                await MessageBox.ShowAsync("Saving Complete");
            }
        }

        private void Return(object sender, RoutedEventArgs e)
        {
            if(_mainWindowViewModel.CurrentDirectory == null)
                return;
            _mainWindowViewModel.BreadCrumbsBar.Remove(_mainWindowViewModel.BreadCrumbsBar.Last());
            if(_mainWindowViewModel.BreadCrumbsBar.Count == 0)
            {
                _mainWindowViewModel.CurrentDirectory = null;
                return;
            }

            _mainWindowViewModel.CurrentDirectory = null;
            _mainWindowViewModel.CurrentDirectory = _mainWindowViewModel.BreadCrumbsBar.Last();
        }
        private void ChangeDirectory(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button.Tag is DirectoryItem)
            {
                _mainWindowViewModel.CurrentDirectory = (DirectoryItem)button.Tag;
                int directoryPos = _mainWindowViewModel.BreadCrumbsBar.IndexOf(_mainWindowViewModel.CurrentDirectory);
                if((DirectoryItem)button.Tag == _mainWindowViewModel.BreadCrumbsBar.Last())
                    return;
                for(int i = 0; i < _mainWindowViewModel.BreadCrumbsBar.Count - directoryPos; i++)
                {
                    _mainWindowViewModel.BreadCrumbsBar.Remove(_mainWindowViewModel.BreadCrumbsBar.Last());
                }
            }
            else
            {
                _mainWindowViewModel.CurrentDirectory = null;
                _mainWindowViewModel.BreadCrumbsBar.Clear();
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if(_mainWindowViewModel.CurrentDirectory == null)
                _mainWindowViewModel.ZapArchive.Items.Remove((Item)((MenuItem)sender).Tag);
            else
                _mainWindowViewModel.CurrentDirectory.Items.Remove((Item)((MenuItem)sender).Tag);
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
                SuggestedFileName = fileItem.Name,
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
                _zapFileService.ExportArchive(_mainWindowViewModel.ZapArchive, Uri.UnescapeDataString(folder.First().Path.AbsolutePath));
            }
        }


        private void OnItemMouseDoubleClick(object? sender, TappedEventArgs e)
        {
            if (!e.Pointer.IsPrimary)
                return;
            ListBox? listBox = sender as ListBox;
            if(listBox.SelectedItem is DirectoryItem)
            {
                _mainWindowViewModel.CurrentDirectory = (DirectoryItem)listBox.SelectedItem;
                _mainWindowViewModel.BreadCrumbsBar.Add(_mainWindowViewModel.CurrentDirectory);
            }
        }

        protected async override void OnClosing(WindowClosingEventArgs e)
        {
            if(_addFileService.UnsavedProgress)
            {
                e.Cancel = true;
                var result = await MessageBox.ShowAsync("There is unsaved progress. Are you sure you want to quit?", "Warning", MessageBoxIcon.Warning, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _addFileService.UnsavedProgress = false;
                    Close();
                }
            }
            
            _addFileService.PurgeFolder();
            base.OnClosing(e);
        }
        
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
}