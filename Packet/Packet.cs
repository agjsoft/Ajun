namespace Packet
{
    public class LoginReqPacket : PacketBase
    {
        public string Id;
        public string Pw;

        public override void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
            writer.SetString(Id);
            writer.SetString(Pw);
        }

        public LoginReqPacket()
        {
            PacketId = PacketId.LoginReq;
        }

        public LoginReqPacket(PacketReader reader)
        {
            Id = reader.GetString();
            Pw = reader.GetString();
        }
    }

    public class LoginAckPacket : PacketBase
    {
        public int Result;
        public string Message;
        public long AccountId;

        public override void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
            writer.SetInt(Result);
            if (0 != Result)
                return;

            writer.SetString(Message);
            writer.SetLong(AccountId);
        }

        public LoginAckPacket()
        {
            PacketId = PacketId.LoginAck;
        }

        public LoginAckPacket(PacketReader reader)
        {
            Result = reader.GetInt();
            if (0 != Result)
                return;

            Message = reader.GetString();
            AccountId = reader.GetLong();
        }
    }

    public class UpdateNameReqPacket : PacketBase
    {
        public string Name;

        public override void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
            writer.SetString(Name);
        }

        public UpdateNameReqPacket()
        {
            PacketId = PacketId.UpdateNameReq;
        }

        public UpdateNameReqPacket(PacketReader reader)
        {
            Name = reader.GetString();
        }
    }
}
