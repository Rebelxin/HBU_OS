using System.Text;

namespace HBU_OS.Backend
{
    internal struct DirectoryEntry
    {
        private string _fileName;

        //名称3个字节
        public string FileObjectName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value.Length > 8)
                {
                    value = value.Substring(0, 8);
                }
                byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
                string asciiString = Encoding.ASCII.GetString(asciiBytes);
                _fileName = asciiString;
            }
        }

        private string _extendedName;

        //拓展名2个字节
        public string ExtendedName
        {
            get
            {
                return _extendedName;
            }
            set
            {
                if (value.Length > 2)
                {
                    value = value.Substring(0, 2);
                }
                if (value.Length == 1)
                {
                    value += "\0";
                }
                if (value.Length == 0)
                {
                    value += "\0\0";
                }
                byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
                string asciiString = Encoding.ASCII.GetString(asciiBytes);
                _extendedName = asciiString;
            }
        }

        //起始块号1个字节
        public int StartBlock { get; set; }
        //文件大小1个字节
        public int FileSize { get; set; }
        //文件属性1个字节
        public bool IsDirectory { get; set; }
    }

    internal class Directory
    {
        public List<DirectoryEntry> FileObjects = new List<DirectoryEntry>();
        public void AddFileObject(string fileName, int startBlock, bool isDirectory, string extendedName)
        {
            CheckName(fileName, extendedName, isDirectory);
            FileObjects.Add(new DirectoryEntry
            {
                FileObjectName = fileName,
                ExtendedName = extendedName,
                StartBlock = startBlock,
                FileSize = 1,
                IsDirectory = isDirectory
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>
        /// return the copy of directory entry.
        /// </returns>
        public DirectoryEntry FindFileObject(string fileName)
        {
            return FileObjects.FirstOrDefault(file => file.FileObjectName == fileName);
        }

        public DirectoryEntry FindFileObject(string fileName, string extendedName)
        {
            return FileObjects.FirstOrDefault(file => file.FileObjectName == fileName && file.ExtendedName == extendedName);
        }

        public void ModifyFileObject(string originFileName, string originExtendedName, string fileName, string extendedName, int fileSize)
        {

            for (int i = 0; i < FileObjects.Count; i++)
            {
                if (FileObjects[i].FileObjectName == originFileName && FileObjects[i].ExtendedName == originExtendedName)
                {
                    var fileObject = FileObjects[i];
                    CheckName(fileName, extendedName, fileObject.IsDirectory);
                    fileObject.FileObjectName = fileName;
                    fileObject.ExtendedName = extendedName;
                    fileObject.FileSize = fileSize;
                    FileObjects[i] = fileObject;
                }
            }
        }

        public void CheckName(string name, string extendedName, bool isDirectory)
        {
            if (!isDirectory)
            {
                if (extendedName == "dc")
                {
                    throw new Exception("非法命名");
                }
            }
            else
            {
                if (!(extendedName == "dc"))
                {
                    throw new Exception("非法命名");
                }
            }
        }

        public void DeleteFileObject(string fileName, string extendedName)
        {
            try
            {
                var a = FileObjects.RemoveAll(file => file.FileObjectName == fileName && file.ExtendedName == extendedName);
            }
            catch (Exception)
            {

                throw new Exception("从目录删除文件对象失败");
            }

        }

        //测试用
        public void T_ListFiles()
        {
            foreach (var entry in FileObjects)
            {
                if (entry.IsDirectory)
                {
                    Console.WriteLine($"目录名: {entry.FileObjectName}, 起始块: {entry.StartBlock}");
                }
                else
                {
                    Console.WriteLine($"文件名: {entry.FileObjectName}.{entry.ExtendedName} 起始块: {entry.StartBlock}, 文件大小: {entry.FileSize}");
                }
            }
        }
    }
}
