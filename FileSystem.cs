using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HBU_OS
{
    internal class FileSystem
    {
        public FileAllocationTable FAT;
        public Directory RootDirectory;
        private readonly DiskManager DiskManager;

        public FileSystem()
        {
            DiskManager = new DiskManager();
            FAT = DiskManager.ReadData2FAT();
            RootDirectory = DiskManager.ReadData2Directory(FAT,2);
        }

        public void T_DisplayDisk() 
        {
            DiskManager.ReadData();
        }

        public (string, string) SplitPath(string path)
        {
            string superDirectoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            if (fileName == null || superDirectoryName == null)
            {
                throw new Exception("路径错误");
            }
            return (superDirectoryName,fileName);
        }

        public (DirectoryEntry, Directory) NavigateSuperDirectory(string directoryName)
        {
            string[] parts = directoryName.Split('\\');
            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 1; i < parts.Length-1; i++)
            {
                fileObject = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
                {
                    throw new Exception("路径错误");
                }
                if (!fileObject.IsDirectory)
                {
                    throw new Exception("路径错误");
                }
                dir = DiskManager.ReadData2Directory(FAT, fileObject.StartBlock);
            }
            return (fileObject,dir);
        }

        public DirectoryEntry NvigateFileObject(string FileName)
        {
            string[] parts = FileName.Split('\\');
            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 1; i < parts.Length; i++)
            {
                fileObject = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
                {
                    throw new Exception("路径错误");
                }
                if (!fileObject.IsDirectory && fileObject.FileObjectName == parts[parts.Length-1])
                {
                    break;
                }
                //不是文件则是目录继续向下寻找
                dir = DiskManager.ReadData2Directory(FAT, fileObject.StartBlock);
            }
            return fileObject;
        }

        public void CreateFileObject(string fullFileName)
        {
            (string superDirectoryName, string fileName) = SplitPath(fullFileName);

            Directory dir;
            DirectoryEntry fileObject;

            (fileObject, dir) = NavigateSuperDirectory(superDirectoryName);
            //判断是否是根目录
            int entryStartBlock;
            if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
            {
                entryStartBlock = fileObject.StartBlock;
            }
            else
            {
                entryStartBlock = 2;
            }

            int startBlock = FAT.AllocateBlock();
            dir.AddFileObject(fileName, startBlock, true);
            //向目录写入
            DiskManager.WriteDirectoryEntries2Disk(dir, FAT, entryStartBlock);
            Console.WriteLine($"目录 \"{fileName}\" 创建成功，起始块：{startBlock}");
        }
        
        public void CreateFileObject(string fullFileName, string extendedName)
        {
            (string superDirectoryName, string fileName) = SplitPath(fullFileName);

            Directory dir;
            DirectoryEntry fileObject;

            (fileObject, dir) = NavigateSuperDirectory(superDirectoryName);
            //判断是否是根目录
            int entryStartBlock;
            if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
            {
                entryStartBlock = fileObject.StartBlock;
            }
            else 
            {
                entryStartBlock = 2;
            }

            int startBlock = FAT.AllocateBlock();
            dir.AddFileObject(fileName, startBlock, false,extendedName);

            DiskManager.WriteDirectoryEntries2Disk(dir, FAT, entryStartBlock);
            Console.WriteLine($"文件 \"{fileName}\" 创建成功，起始块：{startBlock}");
        }

        public void WriteFile(string fullFileName,string data)
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFileObject(fileName);

            int startBlock = fileObject.StartBlock;

            int BlockNum = (data.Length + DiskManager.BlockSize - 1) / DiskManager.BlockSize;
            int currentBlock = startBlock;
            for (int i = 0; i < BlockNum - 1; i++)
            {
                int nextBlock = FAT.AllocateBlock();
                FAT.LinkBlocks(currentBlock, nextBlock); 
                currentBlock = nextBlock; 
            }

            DiskManager.WriteFile2Disk(data, FAT, startBlock);
        }

        public string ReadFile(string fullFileName) 
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFileObject(fileName);

            int startBlock = fileObject.StartBlock;

            string data = DiskManager.ReadData2File(FAT,startBlock);
            return data;
        }

        public void DeleteFile(string fullFileName)
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFileObject(fileName);
            int startBlock = fileObject.StartBlock;

            DiskManager.DeleteDataFromFile(FAT, startBlock);

            FAT.FreeBlockChain(startBlock);
        }

        public void ListRootFiles()
        {
            RootDirectory.T_ListFiles();
        }
    }

}
