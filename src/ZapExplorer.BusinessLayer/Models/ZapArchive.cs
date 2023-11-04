using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    public class ZapArchive : ICloneable
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

        public object Clone()
        {
            ZapArchive archive = (ZapArchive) MemberwiseClone();
            for (int i = 0; i < archive.Items.Count; i++)
            {
                if (archive.Items[i] is DirectoryItem)
                {
                    archive.Items[i] = (DirectoryItem)archive.Items[i].Clone();
                    ((DirectoryItem)archive.Items[i]).Items = new List<Item>(((DirectoryItem)archive.Items[i]).Items);
                    ReplaceLists(((DirectoryItem)archive.Items[i]).Items);
                }
                else
                {
                    archive.Items[i] = (FileItem)archive.Items[i].Clone();
                }
            }
            return archive;
        }

        private void ReplaceLists(List<Item> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if(items[i] is DirectoryItem)
                {
                    items[i] = (DirectoryItem)items[i].Clone();
                    ((DirectoryItem)items[i]).Items = new List<Item>(((DirectoryItem)items[i]).Items);
                    ReplaceLists(((DirectoryItem)items[i]).Items);
                }
                else
                {
                    items[i] = (FileItem)items[i].Clone();
                }
            }
        }
    }
}
