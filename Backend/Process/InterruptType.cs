using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal enum InterruptType
    {
        ProgramEnd,
        TimeSliceExpired,
        IOInterrupt
    }

    internal class Interrupt
    {
        public InterruptType Type { get; set; }
        public int ProcessID { get; set; }

        public Interrupt(InterruptType type, int pid)
        {
            Type = type;
            ProcessID = pid;
        }
    }
}
