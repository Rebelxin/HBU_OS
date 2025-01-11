using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Memory
{

    internal class PageTable
    {
        private struct PageTableEntry
        {
            public int FrameID;
            public int BlockID;
        }

        List<PageTableEntry> entries;

        public int BlockID;

        public int ProcessID;

        public PageTable(int processID,int blockID)
        {
            entries = new List<PageTableEntry>();
            ProcessID = processID;
            BlockID = blockID;
        }

        public void AddFrame(int FrameID,int BlockID)
        { 
            entries.Add(new PageTableEntry { FrameID = FrameID, BlockID = BlockID });
        }

        public int[] GetBlocks()
        {
            int[] Blocks = new int[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                Blocks[i] = entries[i].BlockID;
            }

            return Blocks;
        }

    }
}
