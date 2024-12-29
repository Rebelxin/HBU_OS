using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal class Scheduler
    {
        private Queue<ProcessControlBlock> readyQueue;
        private List<ProcessControlBlock> allProcesses;
        private int maxProcesses;
        private int nextPID;
        private object queueLock = new object(); // 锁对象

        public Scheduler(int maxProcesses = 10)
        {
            readyQueue = new Queue<ProcessControlBlock>();
            allProcesses = new List<ProcessControlBlock>();
            this.maxProcesses = maxProcesses;
            nextPID = 1;
        }

        // 获取下一个要执行的进程（时间片轮转）
        public ProcessControlBlock GetNextProcess()
        {
            lock (queueLock)
            {
                if (readyQueue.Count == 0)
                    return null;

                ProcessControlBlock pcb = readyQueue.Dequeue();
                if (pcb.State == ProcessState.Ready)
                {
                    return pcb;
                }
                return null;
            }
        }
        public ProcessControlBlock CreateProcess(int limit=10)
        {
            lock (queueLock)
            {
                if (allProcesses.Count >= maxProcesses)
                {
                    Console.WriteLine("进程数量已达上限。");
                    return null;
                }

                ProcessControlBlock pcb = new ProcessControlBlock(nextPID++,limit);
                allProcesses.Add(pcb);
                readyQueue.Enqueue(pcb);
                Console.WriteLine($"创建进程: {pcb.ProcessID}");
                return pcb;
            }
        }

        public ProcessControlBlock FindProcess(int pid)
        {
            lock (queueLock)
            {
                return allProcesses.Find(p => p.ProcessID == pid);
            }
        }

        // 终止进程
        public void TerminateProcess(ProcessControlBlock pcb)
        {
            lock (queueLock)
            {
                pcb.State = ProcessState.Terminated;
                allProcesses.Remove(pcb);
                Console.WriteLine($"终止进程: {pcb.ProcessID}");
            }
        }

        // 预占当前进程
        public void PreemptProcess(ProcessControlBlock pcb)
        {
            lock (queueLock)
            {
                pcb.State = ProcessState.Ready;
                readyQueue.Enqueue(pcb);
                Console.WriteLine($"预占进程: {pcb.ProcessID}");
            }
        }

        // 阻塞进程
        public void BlockProcess(ProcessControlBlock pcb, string reason)
        {
            lock (queueLock)
            {
                pcb.State = ProcessState.Blocked;
                pcb.WaitReason = reason;
                Console.WriteLine($"阻塞进程: {pcb.ProcessID}, 原因: {reason}");
            }
        }

        // 唤醒进程
        public void WakeUpProcess(int pid)
        {
            lock (queueLock)
            {
                ProcessControlBlock pcb = allProcesses.Find(p => p.ProcessID == pid);
                if (pcb != null && pcb.State == ProcessState.Blocked)
                {
                    pcb.State = ProcessState.Ready;
                    pcb.WaitReason = "";
                    readyQueue.Enqueue(pcb);
                    Console.WriteLine($"唤醒进程: {pcb.ProcessID}");
                }
                else
                {
                    Console.WriteLine($"未找到阻塞中的进程: {pid}");
                }
            }
        }

        // 打印所有进程状态
        public void PrintAllProcesses()
        {
            lock (queueLock)
            {
                Console.WriteLine("当前所有进程状态:");
                foreach (var pcb in allProcesses)
                {
                    Console.WriteLine(pcb);
                }
            }
        }
    }
}
