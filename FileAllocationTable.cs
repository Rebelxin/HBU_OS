using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBU_OS
{
    
    internal class FileAllocationTable
    {
        //一个块用一个字节表示
        //存储中4个bit代表下一个块号,4个bit代表是否分配
        public readonly int BlockNum;
        public int[] LinkTable { get; private set; }
        public bool[] BitMap { get; private set; }

        public FileAllocationTable()
        {
            BlockNum = DiskManager.BlockNum;
            LinkTable = new int[BlockNum];
            for (int i = 0; i < BlockNum; i++)
            {
                LinkTable[i] = 0; // 0 表示末尾磁盘块
            }
            LinkTable[0] = 1;
            LinkTable[1] = 0;
            LinkTable[2] = 0;

            BitMap = new bool[BlockNum];
            BitMap[0] = true;
            BitMap[1] = true;
            BitMap[2] = true;
        }
        public int AllocateBlock()
        {
            for (int i = 0; i < BlockNum; i++)
            {

                if (!BitMap[i]) // 如果未使用
                {
                    BitMap[i] = true; // 标记为已分配
                    return i; // 返回块号
                }

            }
            throw new Exception("磁盘空间已满");
        }
        public void FreeBlock(int startBlock)
        {
            if (startBlock >= 0 && startBlock < BlockNum)
            {
                BitMap[startBlock] = false; // 标记为未使用
                LinkTable[startBlock] = 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException("块索引或偏移超出范围");
            }
        }

        public void DisplayBitmap()
        {
            for (int i = 0; i < BlockNum; i++)
            {
                Console.Write(BitMap[i] ? "1" : "0");
            }
        }

        public void LinkBlocks(int currentBlock, int nextBlock)
        {
            LinkTable[currentBlock] = nextBlock;
        }

        public int GetNextBlock(int currentBlock)
        {
            return LinkTable[currentBlock];
        }

        public int GetEndBlock(int startBlock)
        {
            int currentBlock = startBlock;

            while (GetNextBlock(currentBlock) != 0) 
            {
                currentBlock = GetNextBlock(currentBlock); 
            }
            return currentBlock;
        }

        public void FreeBlockChain(int startBlock)
        {
            int current = startBlock;
            while (current != 0)
            {
                int next = LinkTable[current];
                FreeBlock(current);
                current = next;
            }
        }

        public void T_ListFat() 
        {
            for (int i=0;i< BlockNum;i++) {
                Console.WriteLine(" "+LinkTable[i]+" : " + BitMap[i]);
            }
        }
        public void T_ListFat(int limit)
        {
            for (int i = 0; i < BlockNum && i<limit ; i++)
            {
                Console.WriteLine(i+" : " + LinkTable[i] + " : " + BitMap[i]);
            }
        }
    }

}
