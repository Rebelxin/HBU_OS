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

        public FileSystem()
        {
            DiskManager0 = new DiskManager();
            FAT = DiskManager0.ReadData2FAT();
            RootDirectory = DiskManager0.ReadData2Directory(FAT, 2);
        }

        public void ResetDisk() {
            DiskManager0.InitializeDisk();
            FAT = new FileAllocationTable();
            DiskManager0.WriteFAT2Disk(FAT);
            RootDirectory = DiskManager0.ReadData2Directory(FAT, 2);
        }
        /// <summary>
        /// Split path
        /// </summary>
        /// <param name="path">Full path</param>
        /// <returns>(superDirectoryName, fullName)</returns>
        /// <exception cref="Exception"></exception>
        private (string, string) SplitPath(string path)
        {
            string superDirectoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            if (fileName == null || superDirectoryName == null)
            {
                throw new Exception("文件不存在");
            }
            return (superDirectoryName,fileName);
        }

        private (string,string) SplitName(string fullName)
        {
            string extendedName;
            string fileName = fullName;
            if (fullName.Contains("."))
            {
                string[] tmp = fullName.Split(".");
                fileName = tmp[0];
                extendedName = tmp[1];
                if (extendedName.Length == 1)
                {
                    extendedName += '\0';
                }
            }
            else
            { 
                extendedName = "\0\0";
            }
            return (fileName,extendedName);

        }
        /// <summary>
        /// Navigate directoryEntry from path
        /// </summary>
        /// <param name="fullDirectoryPath"></param>
        /// <returns>(Directory entry, Directory)</returns>
        /// <exception cref="Exception"></exception>
        private (DirectoryEntry, Directory) NavigateDirectory(string fullDirectoryPath)
        {
            string[] parts = fullDirectoryPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 0; i < parts.Length; i++)
            {
                fileObject = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
                {
                    throw new Exception("文件不存在");
                }
                if (!fileObject.IsDirectory)
                {
                    throw new Exception("文件不存在");
                }
                dir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);
            }

            //构建根目录目录项
            if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
            {
                fileObject.FileObjectName = "rot";
                fileObject.ExtendedName = "  ";
                fileObject.StartBlock = 2;
                fileObject.FileSize = 1;
                fileObject.IsDirectory = true;
            }


            return (fileObject,dir);
        }

        private DirectoryEntry NavigateFile(string FileName)
        {
            string[] parts = FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            string extendedName;

            (parts[parts.Length - 1],extendedName) = SplitName(parts[parts.Length - 1]);

            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 0; i < parts.Length; i++)
            {
                fileObject = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
                {
                    throw new Exception("文件不存在");
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
        public void CreateFileObject(string fullFileName,bool isDirectory)
        {
            (string superDirectoryName, string fileName) = SplitPath(fullFileName);
            string extendedName;
            if (isDirectory)
            {
                extendedName = "dc";
            }
            else 
            {
                (fileName,extendedName) = SplitName(fileName);
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
            
            int entryStartBlock;
            entryStartBlock = fileObject.StartBlock;

            int startBlock = FAT.AllocateBlock();
            dir.AddFileObject(fileName, startBlock, isDirectory, extendedName);

            DiskManager0.WriteFAT2Disk(FAT);
            DiskManager0.WriteDirectory2Disk(dir, FAT, entryStartBlock);
            Console.WriteLine($"文件 \"{fileName}\" 创建成功，起始块：{startBlock}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileObjectPath"></param>
        /// <param name="isDirectory">
        /// Is the file object a Directory
        /// </param>
        /// <returns></returns>
        public DirectoryEntry FindFileObject(string fileObjectPath,bool isDirectory)
        {
            DirectoryEntry fileObject = default;
            if (!isDirectory)
            {
                fileObject = NavigateFile(fileObjectPath);
            }
            else
            {
                (fileObject, _) = NavigateDirectory(fileObjectPath);
            }
            return fileObject;
        }

        public void ModifyFileObject(string fileObjectPath,string fileName) {
            DirectoryEntry fileObject;
            try
            {
                fileObject = FindFileObject(fileObjectPath, true);
            }
            catch (Exception)
            {
                try
                {
                    fileObject = FindFileObject(fileObjectPath, false);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            (string parentDirectoryPath,string unchangedFileName) = SplitPath(fileObjectPath);
            string extendedName;

            (fileName,extendedName) = SplitName(fileName);
            if (fileObject.IsDirectory)
            {
                extendedName = "dc";
            }

            (DirectoryEntry parentFileObject, Directory parentDirectory) = NavigateDirectory(parentDirectoryPath);
            int parentEntryStartBlock = parentFileObject.StartBlock;

            Console.WriteLine(fileName);
            parentDirectory.ModifyFileObjectName(fileObject.FileObjectName, fileObject.ExtendedName, fileName, extendedName);
            
            DiskManager0.WriteDirectory2Disk(parentDirectory,FAT, parentEntryStartBlock);

        }

        public void WriteData2File(string fullFileName,string data)
        {
            DirectoryEntry fileObject;

            fileObject = NavigateFile(fullFileName);

            int startBlock = fileObject.StartBlock;

            int BlockNum = (data.Length + DiskManager.BlockSize - 1) / DiskManager.BlockSize;
            int currentBlock = startBlock;
            for (int i = 0; i < BlockNum - 1; i++)
            {
                int nextBlock = FAT.AllocateBlock();
                FAT.LinkBlocks(currentBlock, nextBlock); 
                currentBlock = nextBlock; 
            }
                DiskManager0.WriteFile2Disk(data, FAT, startBlock);
        }

        public string ReadFile(string fullFileName) 
        {
            DirectoryEntry fileObject;

            fileObject = NavigateFile(fullFileName);

            int startBlock = fileObject.StartBlock;

            string data = DiskManager0.ReadData2File(FAT,startBlock);
            return data;
        }

        public void DeleteFile(string fullFilePath)
        {
            (string superDirectoryPath, string fileName) = SplitPath(fullFilePath);

            Directory directory;
            DirectoryEntry file,directoryEntry;
            
            (directoryEntry, directory) = NavigateDirectory(superDirectoryPath);
            file = NavigateFile(fullFilePath);
            int startBlock = file.StartBlock;

            DiskManager0.DeleteDataFromFile(FAT, startBlock);

            FAT.FreeBlockChain(startBlock);
            DiskManager0.WriteFAT2Disk(FAT);

            directory.DeleteFile(fileName);
            DiskManager0.WriteDirectory2Disk(directory,FAT, directoryEntry.StartBlock);
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
