using System.Diagnostics;
using System.Drawing;
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
            byte[] header = CreateHeader(archive);
            using (var fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(header, 0, header.Length);
            }
        }
        private byte[] CreateHeader(ZapArchive archive)
        {
            ZapArchive clonedArchive = (ZapArchive)archive.Clone();

            List<byte> headerBytes = new List<byte>();
            clonedArchive.SortItems();

            List<Item> items = new List<Item>(clonedArchive.Items);

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
        private List<Item> FlattenDirectory(List<Item> archiveItems)
        {
            List<Item> items = new List<Item>();
            foreach (Item item in archiveItems)
            {
                items.Add(item);
                if (item is DirectoryItem)
                    items.AddRange(FlattenDirectory(((DirectoryItem)item).Items));
            }
            return items;
        }
        private void AddDirectoryEnds(List<Item> archiveItems)
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
                foreach(Item item in archive.Items)
                {
                    if (item.Size == 0 || item is DirectoryItem)
                        continue;
                    ((FileItem)item).StartPos = GetPadding(prevFileEnd, paddingSize);
                    prevFileEnd = ((FileItem)item).StartPos + item.Size;
                    ((FileItem)item).EndPos = prevFileEnd;
                    Debug.WriteLine("Size: " + item.Size);
                    Debug.WriteLine("StartPos: " + ((FileItem)item).StartPos + " EndPos: " + ((FileItem)item).EndPos);
                }

            }
            return archive;
        }
    }
}