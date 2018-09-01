﻿namespace Packet
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
}