using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend
{
    internal class FileSystem
    {
        public class FileNode
        {
            public string Name { get; set; }
            public bool IsDirectory { get; set; }
            public int StartBlock { get; set; }
            public long FileSize { get; set; }
            public List<FileNode> Children { get; set; } = new();
        }

        public FileAllocationTable FAT;
        public Directory RootDirectory;
        private readonly DiskManager DiskManager0;
        private FileNode RootNode;

        public FileSystem()
        {
            DiskManager0 = new DiskManager();
            FAT = DiskManager0.ReadData2FAT();
            RootDirectory = DiskManager0.ReadData2Directory(FAT, 2);
        }

        public void ResetDisk()
        {
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
                throw new FileObjectPathNotExistException();
            }
            if (superDirectoryName.Length != 1)
            {
                superDirectoryName += '\\';
            }
            return (superDirectoryName, fileName);
        }

        private (string, string) SplitName(string fullName)
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
            return (fileName, extendedName);

        }
        /// <summary>
        /// Navigate parentDirectoryEntry from path
        /// </summary>
        /// <param name="fullDirectoryPath"></param>
        /// <returns>(Directory entry(from parent), Directory)</returns>
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
                    throw new FileObjectPathNotExistException();
                }
                if (!fileObject.IsDirectory)
                {
                    throw new FileObjectPathNotExistException();
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


            return (fileObject, dir);
        }
        /// <summary>
        /// Navigate parentDirectoryEntry from path
        /// </summary>
        /// <param name="fullDirectoryPath"></param>
        /// <returns>Directory entry(from parent)</returns>
        /// <exception cref="Exception"></exception>
        private DirectoryEntry NavigateFile(string fullFilePath)
        {
            string[] parts = fullFilePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            string extendedName, fileName;

            (fileName, extendedName) = SplitName(parts[parts.Length - 1]);
            parts[parts.Length - 1] = fileName;

            Directory dir = RootDirectory;
            DirectoryEntry fileObject = default;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                DirectoryEntry directoryEntry = dir.FindFileObject(parts[i]);
                if (EqualityComparer<DirectoryEntry>.Default.Equals(directoryEntry, default))
                {
                    throw new FileObjectPathNotExistException();
                }
                //不是文件则是目录继续向下寻找
                dir = DiskManager0.ReadData2Directory(FAT, directoryEntry.StartBlock);
            }
            //路径末尾文件搜寻
            fileObject = dir.FindFileObject(fileName, extendedName);
            if (EqualityComparer<DirectoryEntry>.Default.Equals(fileObject, default))
            {
                throw new FileObjectPathNotExistException();
            }

            return fileObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileObjectPath"></param>
        /// <param name="isDirectory">
        /// Is the file object a Directory
        /// </param>
        /// <returns>Directory entry(from parent)</returns>
        public DirectoryEntry NavigateFileObject(string fileObjectPath)
        {
            DirectoryEntry fileObject = default;

            try
            {
                fileObject = NavigateFile(fileObjectPath);
            }
            catch (Exception)
            {
                try
                {
                    (fileObject, _) = NavigateDirectory(fileObjectPath);
                }
                catch (Exception)
                {

                    throw new FileObjectPathNotExistException();
                }
            }
            return fileObject;
        }
        public void CreateFileObject(string fullFileName, bool isDirectory)
        {
            (string superDirectoryName, string fileName) = SplitPath(fullFileName);


            string extendedName;
            if (isDirectory)
            {
                extendedName = "dc";
            }
            else
            {
                (fileName, extendedName) = SplitName(fileName);
                fileName = fileName.CheckFileName();
                extendedName = extendedName.CheckExtendedName();


            }

            Directory parentDirectory;
            DirectoryEntry parentDirectoryEntry;

            (parentDirectoryEntry, parentDirectory) = NavigateDirectory(superDirectoryName);
            if (parentDirectoryEntry.FileObjectName == "rot" && parentDirectory.FileObjects.Count >= DiskManager.BlockSize / 8)
            {
                throw new RootDirectoryLimitExceededException();
            }
            foreach (var subFile in parentDirectory.FileObjects)
            {
                if (subFile.FileObjectName == fileName
                    && subFile.ExtendedName == extendedName)
                {
                    throw new FileNameConflictException(fileName);
                }
            }
            if ((parentDirectory.FileObjects.Count + 1) % (DiskManager.BlockSize / 8) >= 0)
            {
                int extendBlock = FAT.AllocateBlock();
                FAT.LinkBlocks(FAT.GetEndBlock(parentDirectoryEntry.StartBlock), extendBlock);
            }

            int entryStartBlock;
            entryStartBlock = parentDirectoryEntry.StartBlock;

            int startBlock = FAT.AllocateBlock();


            parentDirectory.AddFileObject(fileName, startBlock, isDirectory, extendedName);



            DiskManager0.WriteFAT2Disk(FAT);
            DiskManager0.WriteDirectory2Disk(parentDirectory, FAT, entryStartBlock);
            Console.WriteLine($"文件 \"{fileName}\" 创建成功，起始块：{startBlock}");
        }

        public void CopyFileObject(string sourceFileObjectPath, string destinationFileObjectPath)
        {
            DirectoryEntry sourceFileObjectEntry, destinationFileObjectEntry;
            sourceFileObjectEntry = NavigateFileObject(sourceFileObjectPath);
            CreateFileObject(destinationFileObjectPath, sourceFileObjectEntry.IsDirectory);
            destinationFileObjectEntry = NavigateFileObject(destinationFileObjectPath);

            if (sourceFileObjectEntry.IsDirectory)
            {
                Directory sourceDirectory = DiskManager0.ReadData2Directory(FAT, sourceFileObjectEntry.StartBlock);
                foreach (var item in sourceDirectory.FileObjects)
                {
                    string subSourceFileObjectPath = sourceFileObjectPath + "\\" + item.FileObjectName;
                    string subDestinationFileObjectPath = destinationFileObjectPath + "\\" + item.FileObjectName;
                    CopyFileObject(subSourceFileObjectPath, subDestinationFileObjectPath);
                }
                DiskManager0.WriteDirectory2Disk(sourceDirectory, FAT, destinationFileObjectEntry.StartBlock);
            }
            else
            {
                string data = ReadFile(sourceFileObjectPath);
                if (!string.IsNullOrEmpty(data))
                {
                    WriteData2File(destinationFileObjectPath, data);
                }

            }
        }

        public void MoveFileObject(string sourceFileObjectPath, string destinationFileObjectPath)
        {
            CopyFileObject(sourceFileObjectPath, destinationFileObjectPath);

            DeleteFileObject(sourceFileObjectPath);
        }

        public void ModifyFileObject(string fileObjectPath, string fileName)
        {
            DirectoryEntry fileObject;

            fileObject = NavigateFileObject(fileObjectPath);

            (string parentDirectoryPath, string unchangedFileName) = SplitPath(fileObjectPath);
            string extendedName;

            (fileName, extendedName) = SplitName(fileName);
            if (fileObject.IsDirectory)
            {
                extendedName = "dc";
            }

            (DirectoryEntry parentFileObject, Directory parentDirectory) = NavigateDirectory(parentDirectoryPath);
            int parentEntryStartBlock = parentFileObject.StartBlock;

            parentDirectory.ModifyFileObject(fileObject.FileObjectName, fileObject.ExtendedName, fileName, extendedName, fileObject.FileSize);

            DiskManager0.WriteDirectory2Disk(parentDirectory, FAT, parentEntryStartBlock);

            if (fileObject.IsDirectory)
            {
                Console.WriteLine($"目录 \"{fileObjectPath}\" 已经重命名为 \"{parentDirectoryPath + fileName}\"");
            }
            else
            {
                Console.WriteLine($"文件 \"{fileObjectPath}\" 已经重命名为 \"{parentDirectoryPath + fileName}.{extendedName}\"");
            }

        }

        public void WriteData2File(string fullFilePath, string data)
        {
            Directory parentDirectory;
            DirectoryEntry parentDirectoryEntry, file;

            (string parentDirectoryPath, _) = SplitPath(fullFilePath);
            file = NavigateFile(fullFilePath);

            (parentDirectoryEntry, parentDirectory) = NavigateDirectory(parentDirectoryPath);

            int startBlock = file.StartBlock;
            int parentStartBlock = parentDirectoryEntry.StartBlock;

            int BlockNum = (data.Length + DiskManager.BlockSize - 1) / DiskManager.BlockSize;
            int currentBlock = startBlock;
            for (int i = 0; i < BlockNum - 1; i++)
            {
                int nextBlock = FAT.AllocateBlock();
                FAT.LinkBlocks(currentBlock, nextBlock);
                currentBlock = nextBlock;
            }

            parentDirectory.ModifyFileObject(file.FileObjectName, file.ExtendedName,
                                             file.FileObjectName, file.ExtendedName, BlockNum);

            DiskManager0.WriteDirectory2Disk(parentDirectory, FAT, parentStartBlock);

            DiskManager0.WriteFile2Disk(data, FAT, startBlock);
            Console.WriteLine($"文件 \"{fullFilePath}\" 写入成功");
        }

        public string ReadFile(string fullFilePath)
        {
            DirectoryEntry fileObject;

            fileObject = NavigateFile(fullFilePath);

            int startBlock = fileObject.StartBlock;

            string data = DiskManager0.ReadData2File(FAT, startBlock);
            return data;
        }

        public void DeleteFileObject(string fullFileObjectPath)
        {
            DirectoryEntry fileObject = NavigateFileObject(fullFileObjectPath);

            (string parentDirectoryPath, string fileObjectName) = SplitPath(fullFileObjectPath);
            (fileObjectName, string fileObjectExtendedName) = SplitName(fileObjectName);


            Directory parentDirectory, directory;
            DirectoryEntry parentDirectoryEntry, fileObjectEntry;
            (parentDirectoryEntry, parentDirectory) = NavigateDirectory(parentDirectoryPath);

            if (fileObject.IsDirectory)
            {
                (fileObjectEntry, directory) = NavigateDirectory(fullFileObjectPath);
                foreach (var item in directory.FileObjects)
                {
                    string tmp = fullFileObjectPath + "\\" + item.FileObjectName;
                    DeleteFileObject(tmp);
                }
                fileObjectExtendedName = fileObjectEntry.ExtendedName;
            }
            else
            {
                fileObjectEntry = NavigateFile(fullFileObjectPath);
            }

            int startBlock = fileObjectEntry.StartBlock;
            int parentStartBlock = parentDirectoryEntry.StartBlock;

            DiskManager0.DeleteDataFromFileObject(FAT, startBlock);

            FAT.FreeBlockChain(startBlock);
            DiskManager0.WriteFAT2Disk(FAT);
            //从父目录删除
            parentDirectory.DeleteFileObject(fileObjectName, fileObjectExtendedName);

            DiskManager0.WriteDirectory2Disk(parentDirectory, FAT, parentStartBlock);
            Console.WriteLine($"文件 \"{fullFileObjectPath}\" 删除成功");
        }

        public void ShowFile(string fullFileObjectPath)
        {
            var fileObject = NavigateFileObject(fullFileObjectPath);
            Console.WriteLine("文件对象信息: ");
            if (fileObject.IsDirectory)
            {
                Console.WriteLine($"目录名称: {fileObject.FileObjectName}");
            }
            else
            {
                Console.WriteLine($"文件名称: {fileObject.FileObjectName}.{fileObject.ExtendedName}");
            }
            Console.WriteLine($"文件对象大小： {fileObject.FileSize}");
        }

        public void ListRootFiles()
        {
            RootDirectory.T_ListFiles();
        }

        public void ListFilesFromDirectory(Directory dir, int incident)
        {
            string incidents = " | ".Repeat(incident);
            foreach (var fileObject in dir.FileObjects)
            {
                if (fileObject.IsDirectory)
                {

                    Console.WriteLine(incidents + $"{fileObject.FileObjectName}   " +
                                                $" : {fileObject.StartBlock} : {fileObject.FileSize}");
                    Directory subdir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);
                    incident++;
                    ListFilesFromDirectory(subdir, incident);
                }
                else
                {
                    Console.WriteLine(incidents + $"{fileObject.FileObjectName}.{fileObject.ExtendedName}" +
                                                $" : {fileObject.StartBlock} : {fileObject.FileSize}");
                }
            }
        }

        public void ListAllFiles()
        {
            Directory dir = RootDirectory;

            ListFilesFromDirectory(dir, 0);

        }

        public void DisplayDisk()
        {
            DiskManager0.ReadData();
        }

        public void ListFAT(int limit)
        {
            FAT.T_ListFat(limit);
        }

        public FileNode TraverseFileTree()
        {
            FileNode rootNode = new FileNode
            {
                Name = "rot",
                IsDirectory = true,
                FileSize = 1,
                StartBlock = 2
            };


            BuildFileTree(RootDirectory, rootNode);

            string json = JsonSerializer.Serialize(rootNode, new JsonSerializerOptions
            {
                WriteIndented = true // 美化 JSON 输出
            });

            ListAllFiles();

            return rootNode;
        }

        private void BuildFileTree(Directory dir, FileNode parent)
        {
            if (parent.Name == "rot")
            {
                Console.WriteLine("root num: " + dir.FileObjects.Count);
            }


            foreach (var fileObject in dir.FileObjects)
            {
                FileNode childFileNode = new FileNode();

                childFileNode.IsDirectory = fileObject.IsDirectory;
                childFileNode.StartBlock = fileObject.StartBlock;
                childFileNode.FileSize = fileObject.FileSize;



                if (fileObject.IsDirectory)
                {
                    childFileNode.Name = fileObject.FileObjectName;

                    Directory subdir = DiskManager0.ReadData2Directory(FAT, fileObject.StartBlock);

                    BuildFileTree(subdir, childFileNode);

                    parent.Children.Add(childFileNode);
                }
                else
                {
                    childFileNode.Name = fileObject.FileObjectName + "." + fileObject.ExtendedName.Replace("\u0000", "");
                    parent.Children.Add(childFileNode);
                }
            }
        }
    }

}
