using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using ZapExplorer.BusinessLayer.Models;

namespace ZapExplorer.BusinessLayer
{
    public class ZapFileService
    {
        public long GetPadding(long value, long padding)
        {
            long rem = value % padding;
            long result = value - rem;
            if (rem != 0)
                result += padding;
            return result;
        }
        public void SaveArchive(ZapArchive archive, string path)
        {
            ZapArchive clonedArchive = Utility.DeepClone(archive);
            byte[] header = CreateHeader(clonedArchive);
            clonedArchive.Items = FlattenDirectory(clonedArchive.Items);

            string savePath = path;
            if(File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                savePath = $"{AddFileService.FILES_PATH}/{fi.Name}";
            }

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                fs.Write(header, 0, header.Length);
                foreach(var item in clonedArchive.Items.Where(x => x is FileItem && x.Size > 0))
                {
                    using (var originFs = new FileStream(((FileItem)item).Origin, FileMode.Open))
                    {
                        byte[] fileBytes = new byte[((FileItem)item).Size];
                        originFs.Seek((int)((FileItem)item).StartPos, SeekOrigin.Begin);
                        originFs.Read(fileBytes, 0, fileBytes.Length);
                        fs.Write(fileBytes, 0, fileBytes.Length);
                        int remainingPadding = (int)GetPadding(((FileItem)item).Size, clonedArchive.PaddingSize) - ((FileItem)item).Size;
                        for(int i = 0; i < remainingPadding; i++)
                        {
                            fs.WriteByte(0x0);
                        }
                        Debug.WriteLine(fs.Position);
                    }
                }
            }

            if(savePath != path)
            {
                File.Delete(path);
                File.Move(savePath, path);
            }
        }
        private byte[] CreateHeader(ZapArchive clonedArchive)
        {
            //ZapArchive clonedArchive = Utility.DeepClone(archive);

            List<byte> headerBytes = new List<byte>();
            clonedArchive.SortItems();

            ObservableCollection<Item> items = new ObservableCollection<Item>(clonedArchive.Items);

            foreach (Item item in items)
            {
                if (item is DirectoryItem)
                    AddDirectoryEnds(((DirectoryItem)item).Items);
            }

            items = FlattenDirectory(items);

            // Adding ZAP
            headerBytes.AddRange(new byte[] { (byte)'Z', (byte)'A', (byte)'P' , 0x0});

            // Adding padding size
            headerBytes.AddRange(BitConverter.GetBytes(clonedArchive.PaddingSize));

            // Adding temporary value for count
            headerBytes.AddRange(BitConverter.GetBytes(items.Count));

            // Adding unknown value;
            headerBytes.AddRange(BitConverter.GetBytes(clonedArchive.UnknownValue));
            foreach(Item item in items)
            {
                // Adding unknown bytes, maybe it marks the start of an item?
                headerBytes.AddRange(new byte[] { 0x0d, 0x0a });
                headerBytes.AddRange(BitConverter.GetBytes(item.Size));

                string name = item.Name.Trim();
                if(item is DirectoryItem)
                {
                    if (item.Name != "..")
                        name += "/";
                }

                headerBytes.AddRange(new byte[] { (byte)(name.Length + 1), 0x0 });

                if(name == "")
                {
                    headerBytes.AddRange(new byte[] { 0x2e, 0x2e });
                }
                else
                {
                    headerBytes.AddRange(Encoding.ASCII.GetBytes(name));
                }
                headerBytes.Add(0x0);
            }
            headerBytes.AddRange(new byte[] { 0x0d, 0x0a });
            int totalPadding = (int)GetPadding(headerBytes.Count, clonedArchive.PaddingSize);

            while(headerBytes.Count < totalPadding)
            {
                headerBytes.Add(0x0);
            }


            return headerBytes.ToArray();
        }
        private ObservableCollection<Item> FlattenDirectory(ObservableCollection<Item> archiveItems)
        {
            ObservableCollection<Item> items = new ObservableCollection<Item>();
            foreach (Item item in archiveItems)
            {
                items.Add(item);
                if (item is DirectoryItem)
                {
                    foreach(Item flattenedDirItem in FlattenDirectory(((DirectoryItem)item).Items))
                    {
                        items.Add(flattenedDirItem);
                    }
                }
            }
            return items;
        }
        private void AddDirectoryEnds(ObservableCollection<Item> archiveItems)
        {
            foreach(Item item in archiveItems)
            {
                if (item is DirectoryItem)
                    AddDirectoryEnds(((DirectoryItem)item).Items);
            }
            archiveItems.Add(new DirectoryItem(".."));
        }
        public ZapArchive? GetArchive(string path)
        {
            ZapArchive? archive = new ZapArchive(path);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                // Check if header is ZAP
                byte[] header = new byte[16];
                fs.Read(header, 0, header.Length);
                if (!(header[0] == (byte)'Z' && header[1] == (byte)'A' && header[2] == (byte)'P'))
                {
                    return null;
                }

                // Getting all directory and filenames
                int paddingSize = BitConverter.ToInt32(new byte[] { header[4], header[5], header[6], header[7] });

                archive.PaddingSize = paddingSize;
                archive.UnknownValue = BitConverter.ToInt32(new byte[] { header[12], header[13], header[14], header[15] });

                int itemCount = BitConverter.ToInt32(new byte[] { header[8], header[9], header[10], header[11] });
                List<DirectoryItem> directoryStack = new List<DirectoryItem>();
                while(itemCount > 0)
                {
                    byte[] itemInfo = new byte[8];
                    fs.Read(itemInfo, 0, itemInfo.Length);
                    byte[] nameBytes = new byte[itemInfo[6]];
                    fs.Read(nameBytes, 0, nameBytes.Length);

                    if(nameBytes[0] == 0x2E && nameBytes[1] == 0x2E)
                    {
                        directoryStack.Remove(directoryStack.Last());
                        //currentDirectory = null;
                        itemCount--;
                        continue;
                    }

                    Item currentItem;
                    if (nameBytes[nameBytes.Length -2] == (byte)'/')
                    {
                        nameBytes[nameBytes.Length -2] = 0x0;
                        currentItem = new DirectoryItem(System.Text.Encoding.Default.GetString(nameBytes).TrimEnd('\0'));
                    }
                    else
                    {
                        currentItem = new FileItem(System.Text.Encoding.Default.GetString(nameBytes).TrimEnd('\0'));
                        ((FileItem)currentItem).Origin = path;
                    }

                    currentItem.Size = BitConverter.ToInt32(new byte[] { itemInfo[2], itemInfo[3], itemInfo[4], itemInfo[5] });

                    if (directoryStack.Count > 0)
                    {
                        directoryStack.Last().Items.Add(currentItem);
                    }
                    else
                    {
                        archive.Items.Add(currentItem);
                    }

                    if (currentItem is DirectoryItem)
                    {
                        directoryStack.Add((DirectoryItem)currentItem);
                    }

                    itemCount--;
                }
                long prevFileEnd = fs.Position + 2;
                GetPositions(archive.Items, prevFileEnd, paddingSize);
            }
            return archive;
        }
        private long GetPositions(ObservableCollection<Item> items, long startPos, int paddingSize)
        {
            long prevFileEnd = startPos;
            foreach(Item item in items)
            {
                if(item is DirectoryItem)
                {
                    prevFileEnd = GetPositions(((DirectoryItem)item).Items, prevFileEnd, paddingSize);
                }
                if (item.Size == 0)
                    continue;
                ((FileItem)item).StartPos = GetPadding(prevFileEnd, paddingSize);
                prevFileEnd = ((FileItem)item).StartPos + item.Size;
                ((FileItem)item).EndPos = prevFileEnd;

            }
            return prevFileEnd;
        }
    }
}