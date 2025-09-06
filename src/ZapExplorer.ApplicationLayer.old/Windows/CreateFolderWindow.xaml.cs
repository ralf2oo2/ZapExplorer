using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
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
using System.Text.RegularExpressions;

namespace ZapExplorer.ApplicationLayer.Windows
{
    /// <summary>
    /// Interaction logic for CreateFolderWindow.xaml
    /// </summary>
    public partial class CreateFolderWindow : Window
    {
        public string FolderName { get; set; }

        public bool Confirmed { get; set; }
        public CreateFolderWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            if(tbxFolderName.Text.Length > 255)
            {
                MessageBox.Show("Folder name too long. (Max 255)");
                return;
            }
            if (tbxFolderName.Text.Length == 0)
            {
                MessageBox.Show("Folder name too short");
                return;
            }
            if (tbxFolderName.Text == "..")
            {
                MessageBox.Show("Folder name reserved");
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

        private void tbxFolderName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return !Regex.IsMatch(text, string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
        }

        private void tbxFolderName_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
