using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBU_OS
{
    internal class Test
    {
        static bool TestFileSystem()
        {
            try
            {
                FileSystem fileSystem = new FileSystem(); // 模拟 100 个磁盘块

                //fileSystem.CreateFileObject("/test1.txt", 5); // 创建一个目录
                //fileSystem.CreateFileObject("test2.txt", 3); // 创建一个 3 个块的文件
                //fileSystem.ListRootFiles();               // 列出所有文件
                //fileSystem.DeleteFile("test1.txt");   // 删除文件
                //fileSystem.ListRootFiles();               // 再次列出文件
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static bool TestDisk()
        {
            try
            {

                FileSystem fileSystem = new FileSystem();
                fileSystem.T_DisplayDisk();
                fileSystem.FAT.T_ListFat(5);
                fileSystem.RootDirectory.T_ListFiles();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }



        static void Main(string[] args)
        {
            Console.WriteLine(123);
            for (int i = 1; i < 1; i++) {
                Console.WriteLine(123);
            }
        }
    }
}
