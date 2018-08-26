using System;
using System.Collections.Generic;

namespace Packet
{
    public class LoginReqPacket : PacketBase
    {
        public string Id;
        public string Pw;

        public override void Encode(PacketWriter writer)
        {
            writer.SetString(Id);
            writer.SetString(Pw);
        }

        public LoginReqPacket()
        {
            PacketId = (int)ePacketId.LoginReq;
        }

        public LoginReqPacket(PacketReader reader)
        {
            Id = reader.GetString();
            Pw = reader.GetString();
        }
    }

    public class Item
    {
        public int Id;
        public DateTime Expired;
        public int Count;

        public void Encode(PacketWriter writer)
        {
            writer.SetInt(Id);
            writer.SetDateTime(Expired);
            writer.SetInt(Count);
        }

        public Item()
        {
        }

        public Item(PacketReader reader)
        {
            Id = reader.GetInt();
            Expired = reader.GetDateTime();
            Count = reader.GetInt();
        }
    }

    public class LoginAckPacket : PacketBase
    {
        public int Result;
        public string Message;
        public long AccountId;
        public List<Item> Inven = new List<Item>();

        public override void Encode(PacketWriter writer)
        {
            writer.SetInt(Result);
            if (0 != Result)
                return;

            writer.SetString(Message);
            writer.SetLong(AccountId);
            writer.SetInt(Inven.Count);
            foreach (var i in Inven)
            {
                i.Encode(writer);
            }
        }

        public LoginAckPacket()
        {
            PacketId = (int)ePacketId.LoginAck;
        }

        public LoginAckPacket(PacketReader reader)
        {
            Result = reader.GetInt();
            if (0 != Result)
                return;

            Message = reader.GetString();
            AccountId = reader.GetLong();
            int count = reader.GetInt();
            for (int i = 0; i < count; i++)
            {
                Inven.Add(new Item(reader));
            }
        }
    }

    public class UpdateNameReqPacket : PacketBase
    {
        public string Name;

        public override void Encode(PacketWriter writer)
        {
            writer.SetString(Name);
        }

        public UpdateNameReqPacket()
        {
            PacketId = (int)ePacketId.UpdateNameReq;
        }

        public UpdateNameReqPacket(PacketReader reader)
        {
            Name = reader.GetString();
        }
    }
}