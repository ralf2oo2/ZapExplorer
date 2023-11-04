using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    [Serializable]
    public class ZapArchive
    {
        public string Origin { get; private set; }
        public int PaddingSize { get; set; }
        public int UnknownValue { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public ZapArchive(string origin)
        {
            Origin = origin;
        }
        public void SortItems()
        {
            SortItems(Items);
        }
        private void SortItems(List<Item> items)
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
