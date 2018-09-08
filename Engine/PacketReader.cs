using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class PacketReader
    {
        private byte[] Buffer;
        private int Pos;

        public PacketReader(byte[] buffer, int pos)
        {
            Buffer = buffer;
            Pos = pos;
        }

        public V GetPacket<V>() where V : PacketBase, new()
        {
            var packet = new V();
            packet.Decode(this);
            return packet;
        }

        public short GetShort()
        {
            short val = BitConverter.ToInt16(Buffer, Pos);
            Pos += sizeof(short);
            return val;
        }

        public int GetInt()
        {
            int val = BitConverter.ToInt32(Buffer, Pos);
            Pos += sizeof(int);
            return val;
        }

        public long GetLong()
        {
            long val = BitConverter.ToInt64(Buffer, Pos);
            Pos += sizeof(long);
            return val;
        }

        public float GetFloat()
        {
            float val = BitConverter.ToSingle(Buffer, Pos);
            Pos += sizeof(float);
            return val;
        }

        public double GetDouble()
        {
            double val = BitConverter.ToDouble(Buffer, Pos);
            Pos += sizeof(double);
            return val;
        }

        public string GetString()
        {
            int len = GetInt();
            string val = Encoding.UTF8.GetString(Buffer, Pos, len);
            Pos += len;
            return val;
        }

        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }

        public List<T> GetList<T>() where T : ITrans, new()
        {
            var list = new List<T>();
            int count = GetInt();
            for (int i = 0; i < count; i++)
            {
                var t = new T();
                t.Decode(this);
                list.Add(t);
            }
            return list;
        }
    }
}