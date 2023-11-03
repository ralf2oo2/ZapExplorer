using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    public class FileItem : Item
    {
        public long StartPos { get; set; }
        public long EndPos { get; set; }
        public FileItem(string name) : base(name) { }
    }
}
