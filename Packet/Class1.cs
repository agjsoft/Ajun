﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Packet
{
    public abstract class PacketBase
    {
        public int PacketId;
        public abstract void Decode(PacketReader r);
        public abstract void Encode(PacketWriter w);
    }

    public interface ITrans
    {
        void Decode(PacketReader r);
        void Encode(PacketWriter w);
    }

    public enum ePacketId
    {
        LoginReq = 7700,
        LoginAck,
        UpdateNameReq,
        UpdateNameAck,
    }

    public class PacketReader
    {
        private byte[] Buffer;
        private int Pos;

        public PacketReader(byte[] buffer, int pos)
        {
            Buffer = buffer;
            Pos = pos;
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