using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    [Serializable]
    public class Item
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public Item(string name)
        {
            Name = name;
        }
    }
}
