using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.BusinessLayer
{
    public class AddFileService
    {
        private static string FILES_PATH = "Temp";
        public bool UnsavedProgress { get; set; } = false;
        public AddFileService()
        {
            if(!Directory.Exists(FILES_PATH))
            {
                Directory.CreateDirectory(FILES_PATH);
            }
        }

        public FileItem AddFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            File.Copy(path, $"{FILES_PATH}/{fi.Name}", true);
            FileItem fileItem = new FileItem(Path.GetFileName(path));
            fileItem.Origin = path;
            fileItem.StartPos = 0;
            fileItem.EndPos = fi.Length;
            return fileItem;

        }
    }
}
