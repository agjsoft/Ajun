using System;
using System.Collections.Generic;
using System.Text;

namespace AEngine
{
    public class PacketWriter
    {
        public byte[] Buffer = new byte[2048];
        public int Pos = 8;

        public void Close(int packetId)
        {
            Array.Copy(BitConverter.GetBytes(Pos), 0, Buffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(packetId), 0, Buffer, 4, sizeof(int));
        }

        public void SetShort(short val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(int));
            Pos += sizeof(int);
        }

        public void SetInt(int val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(int));
            Pos += sizeof(int);
        }

        public void SetLong(long val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(long));
            Pos += sizeof(long);
        }

        public void SetString(string val)
        {
            var bytes = Encoding.UTF8.GetBytes(val);
            SetInt(bytes.Length);
            Array.Copy(bytes, 0, Buffer, Pos, bytes.Length);
            Pos += bytes.Length;
        }

        public void SetDateTime(DateTime val)
        {
            SetLong(val.Ticks);
        }

        public void SetList<T>(List<T> list) where T : ITrans
        {
            SetInt(list.Count);
            foreach (T item in list)
            {
                item.Encode(this);
            }
        }
    }
}