using System;

namespace RosH
{
    [Serializable]
    public class Time
    {
        public uint secs;   // Seconds since epoch
        public uint nsecs;  // Nanoseconds within the current second

        public Time()
        {
            secs = 0;
            nsecs = 0;
        }

        public Time(uint secs, uint nsecs)
        {
            this.secs = secs;
            this.nsecs = nsecs;
        }
    }
}