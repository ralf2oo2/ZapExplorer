using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    public class Item : ICloneable
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public Item(string name)
        {
            Name = name;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
