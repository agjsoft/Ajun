using System;
using Packet;

namespace server
{
    public class Session : SessionBase
    {
        public string Guid;

        public override void OnPacket(PacketId packetId, PacketReader reader)
        {
            switch (packetId)
            {
                case PacketId.LoginReq:
                    {
                        var packet = new LoginReqPacket(reader);
                        string id = packet.Id;
                        string pw = packet.Pw;

                        var sendPacket = new LoginAckPacket();
                        sendPacket.Result = 0;
                        sendPacket.Message = "Success";
                        sendPacket.AccountId = 1982;
                        for (int i = 100; i < 200; i++)
                        {
                            sendPacket.Inven.Add(new Item()
                            {
                                Id = i,
                                Expired = DateTime.Now,
                                Count = i % 5
                            });
                        }
                        Send(sendPacket);
                    }
                    break;
                case PacketId.UpdateNameReq:
                    {
                        var packet = new UpdateNameReqPacket(reader);
                    }
                    break;
            }
        }
    }
}