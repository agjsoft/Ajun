using System.Net.Sockets;
using AEngine;

namespace TokyoClient
{
    public class Client : ClientBase
    {
        public bool bConnected = false;

        public override void OnConnect(SocketError result)
        {
            bConnected = (result == SocketError.Success);
        }

        public override void OnPacket(int packetId, PacketReader r)
        {
        }

        public void NoLockSend(PacketBase packet)
        {
            var writer = new PacketWriter();
            packet.Encode(writer);
            writer.Close(packet.PacketId);
            Socket.Send(writer.Buffer, writer.Pos, SocketFlags.None);
        }
    }
}