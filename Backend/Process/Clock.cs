using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Process
{
    internal class Clock
    {
        public int AbsoluteTime { get; private set; }
        public int RelativeTime { get; private set; }
        public bool TimeSliceExpired { get; private set; }
        public bool IOInterrupt { get; private set; }

        private int timeSliceLimit;

        public Clock(int timeSliceLimit = 5)
        {
            AbsoluteTime = 0;
            RelativeTime = 0;
            this.timeSliceLimit = timeSliceLimit;
            TimeSliceExpired = false;
            IOInterrupt = false;
        }

        // 每次Tick调用
        public void Tick(bool Pause)
        {
            AbsoluteTime++;

            if (!Pause)
            { RelativeTime++; }

            if (RelativeTime >= timeSliceLimit)
            {
                TimeSliceExpired = true;
            }

            // 模拟I/O中断，每10个时钟周期触发一次
            if (AbsoluteTime % 10 == 0)
            {
                IOInterrupt = true;
            }
        }

        public void ResetTimeSlice()
        {
            RelativeTime = 0;
            TimeSliceExpired = false;
        }

        public void ResetIOInterrupt()
        {
            IOInterrupt = false;
        }

        public override string ToString()
        {
            return $"AbsoluteTime: {AbsoluteTime}, RelativeTime: {RelativeTime}";
        }
    }
}
