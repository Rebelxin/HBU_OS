using Backend.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Memory
{
    internal class SystemArea
    {
        private List<ProcessControlBlock> PCBs;

        private int limit;

        public SystemArea(int limit)
        {
            this.limit = limit;
            PCBs = new List<ProcessControlBlock>();
        }

        public void AddPCB(ProcessControlBlock PCB,int BlockID)
        {
            PCB.BlockID = BlockID;
            if (PCBs.Count==limit)
            {
                throw new ThreadLimitException();
            }
            PCBs.Add(PCB);
        }

        public void RemovePCB(ProcessControlBlock PCB)
        {
            PCBs.Remove(PCB);
        }

        public bool Contains(int processID)
        {
            return PCBs.Any(pcb => pcb.ProcessID == processID);
        }
    }

    internal class UserArea
    {
        private List<PageTable> PageTables;

        public UserArea()
        {
            PageTables = new List<PageTable>();
        }

        public void AddPage(int processID,int BlockID)
        {
            PageTables.Add(new PageTable(processID,BlockID));
        }

        public PageTable FindPage(int processID)
        {

            return PageTables.FirstOrDefault(page => page.ProcessID == processID);
        }

        public void RemovePage(int processID)
        {
            PageTables.RemoveAll(page => page.ProcessID == processID);
        }
    }

    internal class MemoryManager
    {
        int MemorySize = 8;
        int SystemSize = 20;
        int UserSize;
        bool[] IsValid ;
        UserArea UserArea;
        SystemArea SystemArea;


        public MemoryManager() {
            IsValid = new bool[MemorySize];
            Array.Fill(IsValid, true);
            UserSize = MemorySize -SystemSize;
            UserArea = new UserArea();
            SystemArea = new SystemArea(SystemSize);
        }

        public void AllocateProcessMemory(ProcessControlBlock pcb)
        {
            try
            {
                if (!SystemArea.Contains(pcb.ProcessID))
                {
                    int PCBBlockID = SystemAreaAllocateBlock();
                    SystemArea.AddPCB(pcb, PCBBlockID);
                }

                int PageBlockID = UserAreaAllocateBlock();
                UserArea.AddPage(pcb.ProcessID, PageBlockID);

                var pageTable = UserArea.FindPage(pcb.ProcessID);
                if (pageTable == null)
                {
                    throw new Exception($"Page table for process {pcb.ProcessID} not found.");
                }

                for (int i = 0; i < pcb.RequiredFrame; i++)
                {
                    int BlockID = UserAreaAllocateBlock();
                    pageTable.AddFrame(i, BlockID);
                }
            }
            catch (MemoryFullException ex)
            {
                Console.WriteLine($"Memory allocation failed for process {pcb.ProcessID}: {ex.Message}");
            }
        }

        private int UserAreaAllocateBlock()
        {
            for (int i = SystemSize; i < MemorySize; i++)
            {
                if (IsValid[i])
                {
                    IsValid[i] = false;
                    return i;
                }
            }
            throw new MemoryFullException("用户内存区域已满");
        }

        private int SystemAreaAllocateBlock()
        {
            for (int i = 0; i < SystemSize; i++)
            {
                if (IsValid[i])
                {
                    IsValid[i] = false;
                    return i;
                }
            }
            throw new MemoryFullException("系统内存区域已满");
        }

        public void RemoveProcessMemory(ProcessControlBlock pcb)
        {
            var pageTable = UserArea.FindPage(pcb.ProcessID);
            IsValid[pageTable.BlockID] = true;
            if (pageTable != null)
            {
                foreach (var frame in pageTable.GetBlocks())
                {
                    IsValid[frame] = true; // 释放物理内存块
                }
            }
            IsValid[pcb.BlockID] = true;
            SystemArea.RemovePCB(pcb);
            
            UserArea.RemovePage(pcb.ProcessID);
        }
    }
}
