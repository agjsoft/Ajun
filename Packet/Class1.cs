using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet
{
    public class UpdateNameReqPacket : IPacket
    {
        public void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
        }

        public UpdateNameReqPacket()
        {
        }

        public UpdateNameReqPacket(PacketReader reader)
        {

        }
    }

    public class PacketWriter
    {
        public byte[] Buffer = new byte[1024];
        public int Pos = 8;

        public void Close(PacketId packetId)
        {
            Array.Copy(BitConverter.GetBytes(Pos), 0, Buffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)packetId), 0, Buffer, 4, sizeof(int));
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
    }

    public interface IPacket
    {
        void Encode(out PacketWriter writer);
    }

    public class LoginReqPacket : IPacket
    {
        public int Result;
        public string Message;
        public long AccountId;

        public void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
            writer.SetInt(Result);
            if (0 != Result)
                return;

            writer.SetString(Message);
            writer.SetLong(AccountId);
        }

        public LoginReqPacket()
        {
        }

        public LoginReqPacket(PacketReader reader)
        {
            Result = reader.GetInt();
            if (0 != Result)
                return;

            Message = reader.GetString();
            AccountId = reader.GetLong();
        }
    }

    public enum PacketId
    {
        LoginReq = 7700,
        LoginAck,
        UpdateNameReq,
        UpdateNameAck,
    }

    public class PacketReader
    {
        private int PacketId;
        private byte[] Buffer;
        private int Pos;

        public PacketReader(int packetId, byte[] buffer, int pos)
        {
            PacketId = packetId;
            Buffer = buffer;
            Pos = pos;
        }

        public int GetPacketId()
        {
            return PacketId;
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
    }
}