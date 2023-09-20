

namespace RosH
{
    [System.Serializable]
    public class Image
    {
        public const string RosMessageName = "sensor_msgs/Image";

        public Header header;
        public uint height;
        public uint width;
        public string encoding;
        public byte is_bigendian;
        public uint step;
        public byte[] data;

        public Image()
        {
            header = new Header();
            height = 0;
            width = 0;
            encoding = "";
            is_bigendian = 0;
            step = 0;
            data = new byte[0];
        }

        public Image(Header header, uint height, uint width, string encoding, byte is_bigendian, uint step, byte[] data)
        {
            this.header = header;
            this.height = height;
            this.width = width;
            this.encoding = encoding;
            this.is_bigendian = is_bigendian;
            this.step = step;
            this.data = data;
        }
    }
}
