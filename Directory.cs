using System.Text;

namespace HBU_OS
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

        private string _extendedName ;

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
        public void AddFileObject(string fileName, int startBlock,bool isDirectory,string extendedName = "nf")
        {
            FileObjects.Add(new DirectoryEntry
            { 
                FileObjectName = fileName,
                ExtendedName = extendedName,
                StartBlock = startBlock,
                FileSize = 0,
                IsDirectory = isDirectory
            });
        }

        public DirectoryEntry FindFileObject(string fileName)
        {
            return FileObjects.FirstOrDefault(file => file.FileObjectName == fileName);
        }

        public void DeleteFile(string fileName)
        {
            FileObjects.RemoveAll(file => file.FileObjectName == fileName);
        }


        //测试用
        public void T_ListFiles()
        {
            foreach (var entry in FileObjects)
            {
                Console.WriteLine($"文件名: {entry.FileObjectName}, 起始块: {entry.StartBlock}, 文件大小: {entry.FileSize}");
            }
        }
    }
}
