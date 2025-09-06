using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.ApplicationLayer.ViewModels;

public class MainWindowViewModel: INotifyPropertyChanged
{
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

    public MainWindowViewModel()
    {
        BreadCrumbsBar = new ObservableCollection<DirectoryItem>();
    }
    
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}