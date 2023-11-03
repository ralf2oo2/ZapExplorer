using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            InitializeComponent();
            DataContext = this;
            _zapFileService = new ZapFileService();
            BreadCrumbsBar = new ObservableCollection<DirectoryItem>();
        }
        
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select zap archive";
            openFileDialog.DefaultExt = "zap";
            openFileDialog.Filter = "zap archives (*.zap)|*zap";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            if(openFileDialog.ShowDialog() == true)
            {
                ZapArchive = _zapFileService.GetArchive(openFileDialog.FileName);
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

        private void OnItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
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
