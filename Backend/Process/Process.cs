using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal enum ProcessState
    {
        Ready,
        Running,
        Blocked,
        Terminated
    }

    // 进程控制块类
    internal class ProcessControlBlock
    {
        public int ProcessID { get; set; }
        public int PC { get; set; } // 程序计数器
        public int PSW { get; set; } // 程序状态字
        public ProcessState State { get; set; }
        public string WaitReason { get; set; }
        public int PageTable { get; set; }
        public string IntermediateResult { get; set; }
        public int PC_LIMIT { get; set; }

        public ProcessControlBlock(int pid,int limit)
        {
            ProcessID = pid;
            PC = 0;
            PSW = 0;
            State = ProcessState.Ready;
            WaitReason = "";
            PageTable = -1;
            IntermediateResult = "";
            PC_LIMIT = limit;
        }

        public override string ToString()
        {
            return $"PID: {ProcessID}, PC: {PC}, PSW: {PSW}, State: {State}, WaitReason: {WaitReason}";
        }
    }
}
