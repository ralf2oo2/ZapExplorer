using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace ZapExplorer.ApplicationLayer.Windows;

public partial class NewFileWindow : Window
{
    public int PaddingSize { get; set; }

    public bool Confirmed { get; set; }
    public NewFileWindow()
    {
        InitializeComponent();
    }
    
    private async void Create(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        if (!int.TryParse(tbxOffset.Text, out int paddingSize) || !IsTextAllowed(tbxOffset.Text))
        {
            await MessageBox.ShowAsync("Non numeric value entered.");
            return;
        }
        PaddingSize = paddingSize;
        Close();
    }
    
    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private static bool IsTextAllowed(string text)
    {
        Regex regex = new Regex("^[0-9]+$");
        return regex.IsMatch(text);
    }
}