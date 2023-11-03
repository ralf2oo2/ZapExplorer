using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    public class DirectoryItem : Item
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public DirectoryItem(string name) : base(name) { }
    }
}
