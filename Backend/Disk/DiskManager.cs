using Backend.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Directory = Backend.Files.Directory;
using SystemDirectory = System.IO.Directory;

namespace Backend.Disk
{
    public class DiskManager
    {
        public string DiskName { get; private set; }

        public static short BlockNum = 128;
        public static short BlockSize = 64;
        private string ApplicationPath = "HBUOS";

        public static string DiskPath { get; private set; }
        public DiskManager(string name = "disk0")
        {
            DiskName = name;
            DiskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationPath);
            CreateDisk();
        }

        public void CreateDisk()
        {
            if (SystemDirectory.Exists(DiskPath))
            {
                Console.WriteLine("程序文件夹存在");
            }
            else
            {
                try
                {
                    SystemDirectory.CreateDirectory(DiskPath);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("创建程序文件夹时出错: " + ex.Message);
                }
            }

            DiskPath = Path.Combine(DiskPath, DiskName);

            if (File.Exists(DiskPath))
            {
                Console.WriteLine("磁盘文件存在");
            }
            else
            {
                try
                {
                    // 使用File类创建文件
                    using (FileStream fs = File.Create(DiskPath))
                    {
                        // 文件已创建，可以进行其他操作，比如写入内容
                    }
                    InitializeDisk();
                    Console.WriteLine("文件已创建： " + DiskPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("创建文件时出错: " + ex.Message);
                }
            }
        }

        public static void InitializeDisk()
        {
            byte nullData = 0;
            // 使用BinaryWriter来写入单个byte
            using (BinaryWriter writer = new(File.Open(DiskPath, FileMode.Open)))
            {
                // 写入单个byte到文件
                for (int i = 2*BlockSize; i < BlockNum * BlockSize; i++)
                {
                    writer.Write(nullData);
                }
            }
        }

        public void ReadData()
        {
            byte tmp;
            int counter = 0;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(DiskPath)))
            {
                for (int i = 0; reader.BaseStream.Position < reader.BaseStream.Length; i++)
                {
                    if (i == 2 * BlockSize || i == 3 * BlockSize)
                    {
                        Console.WriteLine(counter);
                    }
                    tmp = reader.ReadByte();
                    Console.Write(tmp + " ");
                    counter++;
                }
                Console.WriteLine();
                Console.WriteLine(counter);
            }
        }

        public FileAllocationTable ReadData2FAT()
        {
            FileAllocationTable fat = new();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(DiskPath)))
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < 2 * BlockSize; i++)
                {
                    byte tmp = reader.ReadByte();
                    fat.LinkTable[i] = tmp & 127;
                    fat.BitMap[i] = (tmp & 128) != 0;
                }
            }
            return fat;
        }

        public Directory ReadData2Directory(FileAllocationTable fat, int startBlock)
        {
            Directory directory = new Directory();
            int currentBlock = startBlock;
            int counter = 0;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(DiskPath)))
            {
                reader.BaseStream.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                while (currentBlock != 0)
                {
                    string fileName = Encoding.ASCII.GetString(reader.ReadBytes(3));

                    string extendedName = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    byte file_startBlock = reader.ReadByte();

                    byte fileSize = reader.ReadByte();

                    bool isDirectory = reader.ReadByte() != 0;

                    //检查数据是否有效
                    if (!(!isDirectory && fileSize == 0))
                    {
                        directory.AddFileObject(fileName,
                                      file_startBlock,
                                      isDirectory,
                                      extendedName);
                    }
                    counter++;
                    //读完一个块
                    if (counter % (BlockSize / 8) == 0)
                    {
                        currentBlock = fat.LinkTable[currentBlock];
                        reader.BaseStream.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                    }
                }
            }
            return directory;
        }

        public string ReadData2File(FileAllocationTable fat, int startBlock)
        {
            int currentBlock = startBlock;
            StringBuilder data = new StringBuilder(); // 使用 StringBuilder 来存储结果数据
            using (BinaryReader reader = new BinaryReader(File.OpenRead(DiskPath)))
            {
                // 循环读取 FAT 链中的每个块
                while (currentBlock != 0)
                {
                    reader.BaseStream.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                    byte[] blockData = reader.ReadBytes(BlockSize);

                    // 遍历块中的每个字节并追加到数据
                    for (int i = 0; i < blockData.Length; i++)
                    {
                        if (blockData[i] == 0) // 遇到单个字节为 0 时停止
                            return data.ToString();

                        data.Append((char)blockData[i]);
                    }
                    currentBlock = fat.LinkTable[currentBlock];
                }
            }

            return data.ToString();
        }

        public void WriteFAT2Disk(FileAllocationTable fat)
        {
            using (BinaryWriter writer = new(File.Open(DiskPath, FileMode.Open)))
            {
                writer.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < BlockNum; i++)
                {
                    byte binary = (byte)(fat.LinkTable[i] & 0x7F | (fat.BitMap[i] ? 0x80 : 0x00));
                    writer.Write(binary);
                }

            }
        }

        public void WriteDirectory2Disk(Directory directory, FileAllocationTable fat, int startBlock)
        {
            int currentBlock = startBlock;
            int counter = 0;
            using (BinaryWriter writer = new(File.Open(DiskPath, FileMode.Open), Encoding.ASCII))
            {
                writer.Seek(currentBlock * BlockSize, SeekOrigin.Begin);

                foreach (var i in directory.FileObjects)
                {
                    var a = writer.BaseStream.Position;
                    for (int j = 0; j < 3; j++)
                    {
                        writer.Write(i.FileObjectName[j]);
                    }
                    for (int j = 0; j < 2; j++)
                    {
                        writer.Write(i.ExtendedName[j]);
                    }
                    writer.Write((byte)i.StartBlock);
                    writer.Write((byte)i.FileSize);
                    writer.Write((byte)(i.IsDirectory ? 1 : 0));
                    counter++;
                    if (counter % (BlockSize / 8) == 0)
                    {
                        currentBlock = fat.LinkTable[currentBlock];
                        writer.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                    }
                }

                int remainingBytes = BlockSize - counter * 8;
                if (remainingBytes > 0)
                {
                    writer.Seek(currentBlock * BlockSize + counter * 8, SeekOrigin.Begin);
                    writer.Write(new byte[remainingBytes]); // 用空字节填充
                }
            }
        }
        public void WriteFile2Disk(string data, FileAllocationTable fat, int startBlock)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("数据不能为空", nameof(data));
            }

            int currentBlock = startBlock;
            int dataLength = data.Length;
            int dataIndex = 0;

            using (BinaryWriter writer = new(File.Open(DiskPath, FileMode.Open), Encoding.ASCII))
            {
                while (dataIndex < dataLength)
                {
                    int bytesToWrite = Math.Min(BlockSize, dataLength - dataIndex);
                    writer.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                    writer.Write(Encoding.ASCII.GetBytes(data.Substring(dataIndex, bytesToWrite)));
                    dataIndex += bytesToWrite;
                    if (dataIndex >= dataLength)
                        break;
                    currentBlock = fat.LinkTable[currentBlock];
                    if (currentBlock == 0)
                    {
                        throw new InvalidOperationException("文件所需磁盘块数量不足");
                    }
                }
                int remainingBytes = BlockSize - dataLength % BlockSize;
                if (remainingBytes > 0 && currentBlock != 0)
                {
                    writer.Seek(currentBlock * BlockSize + dataLength % BlockSize, SeekOrigin.Begin);
                    writer.Write(new byte[remainingBytes]); // 用空字节填充
                }
            }
        }

        public void DeleteDataFromFileObject(FileAllocationTable fat, int startBlock)
        {
            int currentBlock = startBlock;
            int counter = 0;
            using (BinaryWriter writer = new(File.Open(DiskPath, FileMode.Open), Encoding.ASCII))
            {
                writer.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
                while (currentBlock != 0)
                {
                    writer.Seek(currentBlock * BlockSize, SeekOrigin.Begin);

                    // 将整个块清零
                    for (int i = 0; i < BlockSize; i++)
                    {
                        writer.Write((byte)0);
                    }
                    currentBlock = fat.LinkTable[currentBlock];
                }
            }
        }
    }
}
