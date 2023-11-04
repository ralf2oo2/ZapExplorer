using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    [Serializable]
    public class ZapArchive
    {
        public string Origin { get; set; }
        public int PaddingSize { get; set; }
        public int UnknownValue { get; set; }
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public ZapArchive(string origin)
        {
            Origin = origin;
        }
        public ZapArchive()
        {

        }
        public void SortItems()
        {
            SortItems(Items);
        }
        private void SortItems(ObservableCollection<Item> items)
        {
            items.OrderBy(x => x.Name);
            foreach(Item item in items)
            {
                if (item is DirectoryItem)
                    SortItems(((DirectoryItem)item).Items);
            }
        }
    }
}
