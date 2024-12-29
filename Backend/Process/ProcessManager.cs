using Backend.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal class ProcessManager
    {
        private Scheduler scheduler;
        private MemoryManager memoryManager;


        public ProcessManager(Scheduler scheduler,MemoryManager memoryManager)
        {
            this.scheduler = scheduler;
            this.memoryManager = memoryManager;
        }

        // 创建新进程
        public void CreateProcess()
        {
            var pcb =  scheduler.CreateProcess();
            memoryManager.AllocateProcessMemory(pcb);
        }

        public void CreateProcess(string data)
        {
            int limit = data.Length;

            var pcb = scheduler.CreateProcess(limit);
            memoryManager.AllocateProcessMemory(pcb);
        }

        public void CreateProcess(int limit)
        {
            scheduler.CreateProcess(limit);
        }

        // 终止进程
        public void TerminateProcess(int pid)
        {
            // 查找进程
            var pcb = scheduler.FindProcess(pid);
            if (pcb != null)
            {
                scheduler.TerminateProcess(pcb);
                memoryManager.RemoveProcessMemory(pcb);
            }
            else
            {
                Console.WriteLine($"无法找到进程 {pid} 以终止。");
            }
        }

        // 阻塞进程
        public void BlockProcess(int pid, string reason)
        {
            var pcb = scheduler.FindProcess(pid);
            if (pcb != null)
            {
                scheduler.BlockProcess(pcb, reason);
            }
            else
            {
                Console.WriteLine($"无法找到进程 {pid} 以阻塞。");
            }
        }

        // 唤醒进程
        public void WakeUpProcess(int pid)
        {
            scheduler.WakeUpProcess(pid);
        }
    }
}
