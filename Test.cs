using System;
using System.Collections.Generic;
using System.IO;
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

                FileSystem fileSystem = new FileSystem(true);
                fileSystem.DisplayDisk();
                fileSystem.ListFAT(5);
                fileSystem.RootDirectory.T_ListFiles();
                fileSystem.CreateFileObject("\\tf1",false);
                fileSystem.CreateFileObject("\\td1", true);
                fileSystem.CreateFileObject("\\td1\\tf2", false);
                fileSystem.CreateFileObject("\\td1\\tf3", false);
                fileSystem.CreateFileObject("\\td1\\tf4", false);
                fileSystem.CreateFileObject("\\td1\\td2", true);
                fileSystem.CreateFileObject("\\td1\\td2\\tf5", false);
                fileSystem.CreateFileObject("\\tf6", false);
                fileSystem.CreateFileObject("\\tf6", false);
                string data = "aaaaaaaa\naa";
                fileSystem.WriteFile("\\tf1",data);
                fileSystem.DeleteFile("\\tf1");
                fileSystem = new FileSystem();
                fileSystem.DisplayDisk();
                fileSystem.ListFAT(5);
                fileSystem.ListAllFiles();
                string data1;
                data1 = fileSystem.ReadFile("\\tf1");
                Console.WriteLine("数据为："+data1);

                //fileSystem.CreateFileObject();

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
            TestFileSystem();
        }
    }
}
