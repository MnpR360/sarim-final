using System;

namespace RosH
{
    [Serializable]
    public class Header
    {
        // Standard metadata for higher-level stamped data types.
        // This is generally used to communicate timestamped data
        // in a particular coordinate frame.

        // Sequence ID: consecutively increasing ID
        public uint seq;

        // Two-integer timestamp that is expressed as:
        // - stamp.sec: seconds (stamp_secs) since epoch
        // - stamp.nsec: nanoseconds since stamp_secs
        // Time-handling sugar is provided by the client library
        public Time stamp;

        // Frame this data is associated with
        // - 0: no frame
        // - 1: global frame
        public string frame_id;

        public Header()
        {
            seq = 0;
            stamp = new Time();
            frame_id = "";
        }

        public Header(uint seq, Time stamp, string frame_id)
        {
            this.seq = seq;
            this.stamp = stamp;
            this.frame_id = frame_id;
        }
    }
}
