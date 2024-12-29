using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    internal class FileNameConflictException : Exception
    {
        public FileNameConflictException(string fileName)
            : base($"文件 \"{fileName}\" 创建失败，文件名重复！")
        {
        }
    }

    internal class RootDirectoryLimitExceededException : Exception
    {
        public RootDirectoryLimitExceededException()
            : base($"根目录无法创建更多文件对象！")
        {
        }
    }

    internal class FileObjectPathNotExistException : Exception
    {
        public FileObjectPathNotExistException()
            : base($"没有找到文件对象")
        { 
        }
    }

    internal class MemoryFullException : Exception
    {
        public MemoryFullException(string a)
            :base(a)
        {}
    }

    internal class ThreadLimitException : Exception
    {
        public ThreadLimitException()
            : base("进程数量已满")
        { }
    }
}
