using System;
using System.Collections.Concurrent;
using Packet;

namespace server
{
    public class Server : ServerBase<Session>
    {
        public ConcurrentDictionary<string, Session> UserMap = new ConcurrentDictionary<string, Session>();

        public override void OnAccept(Session session)
        {
            session.Guid = Guid.NewGuid().ToString("N");
            UserMap.TryAdd(session.Guid, session);
        }

        public override void OnDisconnect(Session session)
        {
            Session tmp;
            UserMap.TryRemove(session.Guid, out tmp);
        }

        public override void OnPacket(Session session, PacketId packetId, PacketReader reader)
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
                        session.Send(sendPacket);
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