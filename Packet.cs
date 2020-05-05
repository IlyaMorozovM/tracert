using System;

namespace LR2_CSaN
{
    class ICMPPack
    {
        const int IP_HEADER_SIZE = 20;
        const int ICMP_HEADER_SIZE = 4;
        const short MESSAGE_MAX_SIZE = 1024;

        private byte code;
        private UInt16 checksum;
        private int messageSize;
        private byte[] message = new byte[MESSAGE_MAX_SIZE];

        public byte Type { get; set; }

        public int PackSize { get; set; }

        public ICMPPack(byte type, byte[] data)
        {
            Type = type;
            code = 0;
            Buffer.BlockCopy(data, 0, message, ICMP_HEADER_SIZE, data.Length);
            messageSize = data.Length + 4;  // Add size of ICMP Identifier and ICMP Sequence Number 
            PackSize = messageSize + ICMP_HEADER_SIZE;
            checksum = getChecksum();
        }

        public ICMPPack(byte[] data, int size)
        {
            Type = data[IP_HEADER_SIZE];    //ICMP_TYPE_OFFSET
            code = data[IP_HEADER_SIZE + 1];    //ICMP_CODE_OFFSET
            checksum = BitConverter.ToUInt16(data, IP_HEADER_SIZE + 2);    //ICMP_CHECKSUM_OFFSET
            PackSize = size - IP_HEADER_SIZE;
            messageSize = size - IP_HEADER_SIZE - ICMP_HEADER_SIZE;
            Buffer.BlockCopy(data, IP_HEADER_SIZE + ICMP_HEADER_SIZE, message, 0, messageSize);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[PackSize + 1];
            data[0] = Type;
            data[1] = code;
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, data, 2, 2);
            Buffer.BlockCopy(message, 0, data, ICMP_HEADER_SIZE, messageSize);
            return data;
        }

        public UInt16 getChecksum()
        {
            UInt32 checksum = 0;
            byte[] data = getBytes();
            int index = 0;

            while (index < PackSize)
            {
                checksum += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            checksum = (checksum >> 16) + (checksum & 0xffff);
            checksum += (checksum >> 16);
            return (UInt16)(~checksum);
        }
    }
}
