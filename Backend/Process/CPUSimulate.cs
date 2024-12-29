using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal class CPUSimulate
    {


        private Scheduler scheduler;
        private Clock clock;
        private bool isRunning;
        private int PC_LIMIT;
        private ProcessControlBlock currentProcess;
        private bool Tickpause;

        public CPUSimulate(Scheduler scheduler, Clock clock)
        {
            this.scheduler = scheduler;
            this.clock = clock;
            isRunning = true;
            PC_LIMIT = 0;
            currentProcess = null;
            Registers.PSW = 2;
            Tickpause = false;
        }

        // 模拟中央处理器的执行
        public void Execute()
        {
            while (isRunning)
            {
                if (Registers.PSW!=0)
                {
                    currentProcess = scheduler.GetNextProcess();
                    if (currentProcess == null)
                    {
                        // 没有可运行的进程，等待一段时间后继续检查
                        Console.WriteLine("没有可运行的进程，CPU 正在等待...");
                        Thread.Sleep(300); 
                        clock.Tick(Tickpause);

                        continue;
                    }
                    else
                    {
                        Registers.PC = currentProcess.PC;
                        Registers.PSW = currentProcess.PSW;
                        PC_LIMIT = currentProcess.PC_COUNT;
                        currentProcess.State = ProcessState.Running;
                    }
                }

                // 模拟执行指令
                CPU();
                Console.WriteLine($"执行进程: {currentProcess.ProcessID}, PC: {Registers.PC},PSW: {Registers.PSW}");
            tick:
                    // 增加时钟
                    clock.Tick(Tickpause);

                    // 检查中断
                    HandleInterrupts(currentProcess);   
            }
        }

        // 中央处理器函数，解释并执行指令
        private void CPU()
        {
            Thread.Sleep(300);
            // 简单模拟：每执行一次，程序计数器加1
            Registers.PC += 1;

            // 模拟程序结束指令
            if (Registers.PC >= PC_LIMIT)
            {
                Registers.PSW = 1; // 设置程序结束中断
            }
        }

        // 处理中断
        private void HandleInterrupts(ProcessControlBlock pcb)
        {
            if (Registers.PSW == 1)
            {
                Console.WriteLine($"进程 {pcb.ProcessID} 程序结束。");

                pcb.PSW = Registers.PSW;
                pcb.PC = Registers.PC;

                scheduler.TerminateProcess(pcb);
                clock.ResetTimeSlice();
                Registers.PSW = 1;
            }
            else if (clock.TimeSliceExpired)
            {
                Console.WriteLine($"进程 {pcb.ProcessID} 时间片到。");

                pcb.PSW = Registers.PSW;
                pcb.PC = Registers.PC;
                pcb.State = ProcessState.Ready;
                scheduler.PreemptProcess(pcb);
                clock.ResetTimeSlice();
                Registers.PSW = 2;
            }
            else if (clock.IOInterrupt)
            {
                Console.WriteLine($"进程 {pcb.ProcessID} 发生I/O中断。");
                clock.ResetIOInterrupt();
                scheduler.BlockProcess(pcb, "I/O");

                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.In))
                {
                    Console.WriteLine("子进程正在尝试连接...");
                    pipeClient.Connect();
                    Console.WriteLine("连接成功。");

                    byte[] buffer = new byte[256];


                    int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Substring(1);
                    Console.WriteLine($"子进程收到消息: {message}");

                }
            }
        }

        // 停止CPU执行
        public void Stop()
        {
            isRunning = false;
        }
    }
}
