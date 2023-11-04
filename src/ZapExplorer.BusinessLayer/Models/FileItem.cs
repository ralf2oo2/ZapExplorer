using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapExplorer.BusinessLayer.Models
{
    [Serializable]
    public class FileItem : Item
    {
        public string Origin { get; set; }
        public long StartPos { get; set; }
        public long EndPos { get; set; }
        public FileItem(string name) : base(name) { }
    }
}
