using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    public class ZapArchive
    {
        public string Origin { get; private set; }
        public int PaddingSize { get; set; }
        public int UnknownValue { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public void SortItems()
        {
            ItemComparer comparer = new ItemComparer();
            Items.Sort(comparer);
            foreach(Item item in Items)
            {
                Debug.WriteLine($"Name: {item.Name} Parent: {item.ParentDirectory} \n");
            }
        }

        public ZapArchive(string origin)
        {
            Origin = origin;
        }
    }

    public class ItemComparer : IComparer<Item>
    {
        public int Compare(Item? x, Item? y)
        {
            int parentCompare = string.Compare(x.ParentDirectory?.Name, y.ParentDirectory?.Name, StringComparison.Ordinal);

            if (parentCompare == 0)
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
            else
            {
                return parentCompare;
            }
        }
    }
}
