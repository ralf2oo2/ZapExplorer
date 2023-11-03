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

        public ZapArchive(string origin)
        {
            Origin = origin;
        }
    }
}
