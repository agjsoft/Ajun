using System;
using System.Collections.Generic;

namespace Packet
{
    public class LoginReqPacket : PacketBase
    {
        public string Id;
        public string Pw;

        public override void Decode(PacketReader r)
        {
            Id = r.GetString();
            Pw = r.GetString();
        }

        public override void Encode(PacketWriter writer)
        {
            writer.SetString(Id);
            writer.SetString(Pw);
        }

        public LoginReqPacket()
        {
            PacketId = (int)ePacketId.LoginReq;
        }
    }

    public class Item : ITrans
    {
        public int Id;
        public DateTime Expired;
        public int Count;

        public void Encode(PacketWriter w)
        {
            w.SetInt(Id);
            w.SetDateTime(Expired);
            w.SetInt(Count);
        }

        public void Decode(PacketReader r)
        {
            Id = r.GetInt();
            Expired = r.GetDateTime();
            Count = r.GetInt();
        }
    }

    public class LoginAckPacket : PacketBase
    {
        public int Result;
        public string Message;
        public long AccountId;
        public List<Item> Inven = new List<Item>();

        public override void Decode(PacketReader r)
        {
            Result = r.GetInt();
            if (0 != Result)
                return;

            Message = r.GetString();
            AccountId = r.GetLong();
            int count = r.GetInt();
            Inven = r.GetList<Item>();
        }

        public override void Encode(PacketWriter w)
        {
            w.SetInt(Result);
            if (0 != Result)
                return;

            w.SetString(Message);
            w.SetLong(AccountId);
            w.SetInt(Inven.Count);
            w.SetList(Inven);
        }

        public LoginAckPacket()
        {
            PacketId = (int)ePacketId.LoginAck;
        }
    }

    public class UpdateNameReqPacket : PacketBase
    {
        public string Name;

        public override void Decode(PacketReader r)
        {
            Name = r.GetString();
        }

        public override void Encode(PacketWriter w)
        {
            w.SetString(Name);
        }

        public UpdateNameReqPacket()
        {
            PacketId = (int)ePacketId.UpdateNameReq;
        }
    }
}