using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Backend.Server;

namespace Backend
{
    internal class Test
    {

        static bool TestFileSystem()
        {
            try
            {
                DiskManager disk0 = new();
                disk0.CreateDisk();
                disk0.InitializeDisk();
                FileSystem fileSystem = new FileSystem();
                fileSystem.ResetDisk();
                fileSystem.DisplayDisk();
                fileSystem.ListFAT(5);
                fileSystem.ListRootFiles();
                fileSystem.CreateFileObject("\\tf1.t", false);
                fileSystem.CreateFileObject("\\td1", true);
                fileSystem.CreateFileObject("\\td1\\tf2", false);
                fileSystem.CreateFileObject("\\td1\\tf3", false);
                fileSystem.CreateFileObject("\\td1\\tf4", false);
                fileSystem.CreateFileObject("\\td1\\td2", true);
                fileSystem.CreateFileObject("\\td1\\td2\\tf5", false);
                fileSystem.CreateFileObject("\\tf6", false);
                fileSystem.CreateFileObject("\\tf6", false);
                fileSystem.CreateFileObject("\\tf7", false);
                fileSystem.ListAllFiles();
                string data = "aabbbaaa\naa";
                fileSystem.WriteData2File("\\tf1.t", data);
                fileSystem.WriteData2File("\\td1\\tf2", data);
                fileSystem.ModifyFileObject("\\td1", "td8");
                fileSystem.DeleteFileObject("\\tf1.t");
                fileSystem.DeleteFileObject("\\td8\\td2");
                fileSystem.ListAllFiles();
                fileSystem.ModifyFileObject("\\tf7", "tf1.k");
                fileSystem.ListAllFiles();
                fileSystem.ListFAT(15);
                fileSystem.CopyFileObject("\\tf6", "\\td8\\tf6");
                fileSystem.MoveFileObject("\\td8\\tf2", "\\tf2");
                fileSystem = new FileSystem();
                fileSystem.DisplayDisk();
                fileSystem.ListFAT(5);
                fileSystem.TraverseFileTree();
                fileSystem.ShowFile("\\tf2");
                string data1 = "";
                try
                {
                    data1 = fileSystem.ReadFile("\\td8\\tf2");
                }
                catch (Exception)
                {
                    Console.WriteLine("no file");
                }
                Console.WriteLine("数据为：" + data1);

                DirectoryEntry fileObject = default;

                //Console.WriteLine("StartBlock: "+fileObject.StartBlock);


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
            Server.ListenPort();
        }
    }
}
