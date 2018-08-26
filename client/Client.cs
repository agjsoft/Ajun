﻿using Packet;

namespace client
{
    public class Client : ClientBase
    {
        public override void OnConnect()
        {
        }

        public override void OnPacket(PacketId packetId, PacketReader reader)
        {
            switch (packetId)
            {
                case PacketId.LoginAck:
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