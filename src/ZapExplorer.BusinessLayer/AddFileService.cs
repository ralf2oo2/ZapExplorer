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
        public static string FILES_PATH = "Temp";
        public string Uuid;
        public bool UnsavedProgress { get; set; } = false;
        public AddFileService()
        {
            GenUuid();
            
            string path = Path.Combine(FILES_PATH, Uuid);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void GenUuid()
        {
            Uuid = Guid.NewGuid().ToString();
        }

        public FileItem AddFile(string path)
        {
            UnsavedProgress = true;
            FileInfo fi = new FileInfo(path);
            File.Copy(path, Path.Combine(FILES_PATH, Uuid, fi.Name), true);
            FileItem fileItem = new FileItem(Path.GetFileName(path));
            fileItem.Origin = path;
            fileItem.StartPos = 0;
            fileItem.EndPos = fi.Length;
            fileItem.Size = (int)fi.Length;
            return fileItem;

        }

        public void RefreshFolder()
        {
            PurgeFolder();
            GenUuid();
            UnsavedProgress = false;
            string path = Path.Combine(FILES_PATH, Uuid);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void RemoveFile(FileItem file)
        {

        }

        public void PurgeFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FILES_PATH, Uuid));
            if(dir.Exists)
            {
                dir.Delete(true);
            }
        }
    }
}
