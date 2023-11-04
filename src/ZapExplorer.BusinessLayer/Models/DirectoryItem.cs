using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    [Serializable]
    public class DirectoryItem : Item
    {
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public DirectoryItem(string name) : base(name) { }
    }
}
