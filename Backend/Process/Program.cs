//using Backend.Memory;
//using System;
//using System.Collections.Generic;
//using System.IO.Pipes;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Backend.Process
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            Scheduler scheduler = new Scheduler();
//            Clock clock = new Clock();
//            CPUSimulate cpu = new CPUSimulate(scheduler, clock);
//            MemoryManager memoryManager = new MemoryManager();
//            ProcessManager processManager = new ProcessManager(scheduler, memoryManager);

//            // 创建几个初始进程
//            processManager.CreateProcess(4);
//            processManager.CreateProcess(1);
//            processManager.CreateProcess(10);

//            // 在单独的线程中执行CPU
//            Thread cpuThread = new Thread(new ThreadStart(cpu.Execute));
//            cpuThread.Start();

//            // 主线程用于接受用户输入
//            while (true)
//            {
//                Console.WriteLine("\n请输入命令：create, terminate <pid>, block <pid>, wakeup <pid>, list, exit");


//                skipInput:
//                string input = Console.ReadLine();

//                if (input.StartsWith("#"))
//                {
//                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe",PipeDirection.Out))
//                    {
//                        Console.WriteLine("主进程等待连接");
//                        pipeServer.WaitForConnection();
//                        Console.WriteLine("主进程连接成功");

//                        byte[] buffer = Encoding.UTF8.GetBytes(input);
//                        pipeServer.Write(buffer, 0, buffer.Length);
//                        pipeServer.WaitForPipeDrain();
//                        goto skipInput;
//                    }
//                }
                
                
//                string[] parts = input.Split(' ');



//                if (parts[0] == "create")
//                {
//                    if (parts.Length > 1) 
//                    {
//                        try
//                        {
//                            var limit = int.Parse(parts[1]); 
//                            processManager.CreateProcess(limit);
//                        }
//                        catch (Exception)
//                        {
//                            Console.WriteLine("格式错误，请输入有效的数字。");
//                        }
//                    }
//                    else
//                    {
//                        processManager.CreateProcess();
//                    }
//                }
//                else if (parts[0] == "terminate" && parts.Length > 1)
//                {
//                    if (int.TryParse(parts[1], out int pid))
//                    {
//                        processManager.TerminateProcess(pid);
//                    }
//                    else
//                    {
//                        Console.WriteLine("无效的PID。");
//                    }
//                }
//                else if (parts[0] == "block" && parts.Length > 1)
//                {
//                    if (int.TryParse(parts[1], out int pid))
//                    {
//                        processManager.BlockProcess(pid, "用户请求");
//                    }
//                    else
//                    {
//                        Console.WriteLine("无效的PID。");
//                    }
//                }
//                else if (parts[0] == "wakeup" && parts.Length > 1)
//                {
//                    if (int.TryParse(parts[1], out int pid))
//                    {
//                        processManager.WakeUpProcess(pid);
//                    }
//                    else
//                    {
//                        Console.WriteLine("无效的PID。");
//                    }
//                }
//                else if (parts[0] == "list")
//                {
//                    scheduler.PrintAllProcesses();
//                }
//                else if (parts[0] == "exit")
//                {
//                    cpu.Stop();
//                    cpuThread.Join();
//                    break;
//                }
//                else
//                {
//                    Console.WriteLine("未知命令。");
//                }
//            }
//        }
//    }
//}
