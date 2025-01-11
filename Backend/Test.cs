using Backend.Disk;
using Backend.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend
{
    internal class Test
    {
        
        static void CMD()
        {
            
            // 初始化磁盘管理器和文件系统
            FileSystem fileSystem = new FileSystem();

            // 提示用户输入命令
            Console.WriteLine("文件系统管理器已启动。输入命令以执行操作，输入 'help' 查看所有命令，输入 'exit' 退出。");

            bool running = true;
            while (running)
            {
                Console.Write("\n> ");
                string input = Console.ReadLine();
                input = input.Replace('/','\\');
                string[] parts = input.Split(' ', 2); // 分割命令和参数
                string command = parts[0];
                string argument = parts.Length > 1 ? parts[1] : "";

                try
                {
                    switch (command.ToLower())
                    {
                        case "help":
                            Console.WriteLine("可用命令：");
                            Console.WriteLine("display_disk - 显示磁盘状态");
                            Console.WriteLine("list_root_files - 列出根目录文件");
                            Console.WriteLine("create_file <路径> <是否目录> - 创建文件或目录");
                            Console.WriteLine("write_file <路径> <数据> - 写入数据到文件");
                            Console.WriteLine("modify_file <旧路径> <新路径> - 修改文件或目录名称");
                            Console.WriteLine("delete_file <路径> - 删除文件或目录");
                            Console.WriteLine("list_files - 列出所有文件");
                            Console.WriteLine("copy_file <源路径> <目标路径> - 复制文件");
                            Console.WriteLine("move_file <源路径> <目标路径> - 移动文件");
                            Console.WriteLine("read_file <路径> - 读取文件内容");
                            Console.WriteLine("show_file <路径> - 显示文件内容");
                            Console.WriteLine("exit - 退出程序");
                            break;

                        case "reset_disk":
                            fileSystem.ResetDisk();
                            Console.WriteLine("磁盘已重置。");
                            break;

                        case "display_disk":
                            Console.OutputEncoding = System.Text.Encoding.UTF8;
                            fileSystem.ConsoleDisplayFAT(16);

                            //fileSystem.DisplayDisk();
                            break;

                        case "list_root_files":
                            fileSystem.ListRootFiles();
                            break;

                        case "create_file":
                            var args = argument.Split(' ');
                            if (args.Length == 2 && bool.TryParse(args[1], out bool isDirectory))
                            {
                                try
                                {
                                    fileSystem.CreateFileObject(args[0], isDirectory);
                                }
                                catch (Exception e)
                                {

                                   Console.WriteLine(e);
                                    continue;
                                }
                                
                                Console.WriteLine($"已创建{(isDirectory ? "目录" : "文件")}：{args[0]}");
                            }
                            else
                            {
                                Console.WriteLine("用法：create_file <路径> <是否目录(true/false)>");
                            }
                            break;

                        case "write_file":
                            var writeArgs = argument.Split(' ', 2);
                            if (writeArgs.Length == 2)
                            {
                                fileSystem.WriteData2File(writeArgs[0], writeArgs[1]);
                                Console.WriteLine($"已写入数据到文件：{writeArgs[0]}");
                            }
                            else
                            {
                                Console.WriteLine("用法：write_file <路径> <数据>");
                            }
                            break;

                        case "modify_file":
                            var modifyArgs = argument.Split(' ');
                            if (modifyArgs.Length == 2)
                            {
                                fileSystem.ModifyFileObject(modifyArgs[0], modifyArgs[1]);
                                Console.WriteLine($"文件/目录已重命名：{modifyArgs[0]} -> {modifyArgs[1]}");
                            }
                            else
                            {
                                Console.WriteLine("用法：modify_file <旧路径> <新路径>");
                            }
                            break;

                        case "delete_file":
                            fileSystem.DeleteFileObject(argument);
                            break;

                        case "list_files":
                            fileSystem.ListAllFiles();
                            break;

                        case "copy_file":
                            var copyArgs = argument.Split(' ');
                            if (copyArgs.Length == 2)
                            {
                                fileSystem.CopyFileObject(copyArgs[0], copyArgs[1]);
                                Console.WriteLine($"文件已复制：{copyArgs[0]} -> {copyArgs[1]}");
                            }
                            else
                            {
                                Console.WriteLine("用法：copy_file <源路径> <目标路径>");
                            }
                            break;

                        case "move_file":
                            var moveArgs = argument.Split(' ');
                            if (moveArgs.Length == 2)
                            {
                                fileSystem.MoveFileObject(moveArgs[0], moveArgs[1]);
                                Console.WriteLine($"文件已移动：{moveArgs[0]} -> {moveArgs[1]}");
                            }
                            else
                            {
                                Console.WriteLine("用法：move_file <源路径> <目标路径>");
                            }
                            break;

                        case "read_file":
                            try
                            {
                                string data = fileSystem.ReadFile(argument);
                                Console.WriteLine($"文件内容：\n{data}");
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("读取文件失败，文件可能不存在。");
                            }
                            break;

                        case "show_file":
                            fileSystem.ConsoleShowFile(argument);
                            break;

                        case "exit":
                            running = false;
                            Console.WriteLine("退出文件系统管理器。");
                            break;

                        default:
                            Console.WriteLine("未知命令，输入 'help' 查看所有命令。");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行命令时发生错误：{ex.Message}");
                }
            }
        }

        static void Main(string[] args)
        {
            CMD();
            //TestFileSystem();
            //Server.ListenPort();
        }
    }
}
