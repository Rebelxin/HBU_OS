using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HBU_OS
{
    internal class FileSystem
    {
        public FileAllocationTable FAT;
        public Directory RootDirectory;
        private readonly DiskManager DiskManager0;

        public FileSystem(bool isReset)
        {
            DiskManager0 = new DiskManager();
            if (isReset) 
            {
                Reset();
            }
            FAT = DiskManager0.ReadData2FAT();
            RootDirectory = DiskManager0.ReadData2Directory(FAT,2);
        }

        public FileSystem()
        {
            DiskManager0 = new DiskManager();
            FAT = DiskManager0.ReadData2FAT();
            RootDirectory = DiskManager0.ReadData2Directory(FAT, 2);
        }

        public void Reset() {
            DiskManager0.InitializeDisk();
            FAT = new FileAllocationTable();
            DiskManager0.WriteFAT2Disk(FAT);
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

        public (DirectoryEntry, Directory) NavigateDirectory(string directoryPath)
        {
            string[] parts = directoryPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 0; i < parts.Length; i++)
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
                dir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);
            }
            return (fileObject,dir);
        }

        public DirectoryEntry NvigateFile(string FilePath)
        {
            string[] parts = FilePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            string extendedName = "  ";

            if (parts[parts.Length - 1].Contains("."))
            {
                extendedName = parts[parts.Length - 1].Split(".")[1];
            }

            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 0; i < parts.Length; i++)
            {
                fileObject = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
                {
                    throw new Exception("路径错误");
                }
                if (!fileObject.IsDirectory && fileObject.FileObjectName == parts[parts.Length-1] && fileObject.ExtendedName == extendedName)
                {
                    break;
                }
                //不是文件则是目录继续向下寻找
                dir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);
            }
            return fileObject;
        }    
        public void CreateFileObject(string fullFileName,bool isDirectory, string extendedName="  ")
        {
            (string superDirectoryName, string fileName) = SplitPath(fullFileName);
            if (isDirectory) 
            {
                extendedName = "dc";
            }

            Directory dir;
            DirectoryEntry fileObject;

            (fileObject, dir) = NavigateDirectory(superDirectoryName);
            foreach (var subFile in dir.FileObjects) 
            {
                if (subFile.FileObjectName== fileName
                    && subFile.ExtendedName== extendedName) 
                {
                    Console.WriteLine($"文件 \"{fileName}\" 创建失败，文件名重复！");
                    return;
                }

            }
            //判断是否是根目录
            int entryStartBlock;
            if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
            {
                entryStartBlock = 2;
            }
            else 
            {
                entryStartBlock = fileObject.StartBlock;
            }

            int startBlock = FAT.AllocateBlock();
            dir.AddFileObject(fileName, startBlock, isDirectory, extendedName);

            DiskManager0.WriteFAT2Disk(FAT);
            DiskManager0.WriteDirectoryEntries2Disk(dir, FAT, entryStartBlock);
            Console.WriteLine($"文件 \"{fileName}\" 创建成功，起始块：{startBlock}");
        }

        public void WriteFile(string fullFileName,string data)
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFile(fileName);

            int startBlock = fileObject.StartBlock;

            int BlockNum = (data.Length + DiskManager.BlockSize - 1) / DiskManager.BlockSize;
            int currentBlock = startBlock;
            for (int i = 0; i < BlockNum - 1; i++)
            {
                int nextBlock = FAT.AllocateBlock();
                FAT.LinkBlocks(currentBlock, nextBlock); 
                currentBlock = nextBlock; 
            }
            ListFAT(15);

            DiskManager0.WriteFile2Disk(data, FAT, startBlock);
        }

        public string ReadFile(string fullFileName) 
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFile(fileName);

            int startBlock = fileObject.StartBlock;

            string data = DiskManager0.ReadData2File(FAT,startBlock);
            return data;
        }

        public void DeleteFile(string fullFileName)
        {
            (_, string fileName) = SplitPath(fullFileName);

            DirectoryEntry fileObject;

            fileObject = NvigateFile(fileName);
            int startBlock = fileObject.StartBlock;

            DiskManager0.DeleteDataFromFile(FAT, startBlock);

            FAT.FreeBlockChain(startBlock);
        }

        public void ListRootFiles()
        {
            RootDirectory.T_ListFiles();
        }

        public void ListFilesFromDirectory(Directory dir,int incident)
        {
            string incidents = " | ".Repeat(incident);
            foreach (var fileObject in dir.FileObjects)
            {
                if (fileObject.IsDirectory)
                {

                    Console.WriteLine(incidents+$"{fileObject.FileObjectName}   " +
                                                $" : {fileObject.StartBlock} : {fileObject.FileSize}");
                    Directory subdir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);
                    incident++;
                    ListFilesFromDirectory(subdir, incident);
                }
                else 
                {
                    Console.WriteLine(incidents+$"{fileObject.FileObjectName}.{fileObject.ExtendedName}" +
                                                $" : {fileObject.StartBlock} : {fileObject.FileSize}");
                }
            }
        }

        public void ListAllFiles()
        { 
            Directory dir = RootDirectory;

            ListFilesFromDirectory(dir,0);
            
        }

        public void DisplayDisk()
        {
            DiskManager0.ReadData();
        }

        public void ListFAT(int limit) { 
            FAT.T_ListFat(limit);
        }
    }

}
