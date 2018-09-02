using System;
using Packet;

namespace server
{
    public class Session : SessionBase
    {
        public string Guid;

        public override void OnPacket(int packetId, PacketReader r)
        {
            switch (packetId)
            {
                case (int)ePacketId.LoginReq:
                    {
                        var packet = r.GetPacket<LoginReqPacket>();
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
                        SendAsync(sendPacket);
                    }
                    break;
                case (int)ePacketId.UpdateNameReq:
                    {
                        var packet = r.GetPacket<UpdateNameReqPacket>();
                    }
                    break;
            }
        }
    }
}