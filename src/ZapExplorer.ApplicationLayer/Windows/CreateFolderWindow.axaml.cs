using System.IO;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace ZapExplorer.ApplicationLayer.Windows;

public partial class CreateFolderWindow : Window
{
    public string FolderName { get; set; }

    public bool Confirmed { get; set; }
    
    public CreateFolderWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    private async void Create(object sender, RoutedEventArgs e)
    {
        if(!IsTextAllowed(tbxFolderName.Text))
        {
            await MessageBox.ShowAsync("Illegal folder name");
            return;
        }
        if(tbxFolderName.Text.Length > 255)
        {
            await MessageBox.ShowAsync("Folder name too long. (Max 255)");
            return;
        }
        if (tbxFolderName.Text.Length == 0)
        {
            await MessageBox.ShowAsync("Folder name too short");
            return;
        }
        if (tbxFolderName.Text == "..")
        {
            await MessageBox.ShowAsync("Folder name reserved");
            return;
        }

        Confirmed = true;
        FolderName = tbxFolderName.Text;
        Close();
    }
    
    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private static bool IsTextAllowed(string text)
    {
        return !Regex.IsMatch(text, string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
    }
}