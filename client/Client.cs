using Packet;

namespace client
{
    public class Client : ClientBase
    {
        public override void OnConnect()
        {
        }

        public override void OnPacket(int packetId, PacketReader reader)
        {
            switch (packetId)
            {
                case (int)ePacketId.LoginAck:
                    {
                        var packet = new LoginAckPacket(reader);
                        int ret = packet.Result;
                        string msg = packet.Message;
                        long id = packet.AccountId;
                    }
                    break;
            }
        }
    }
}