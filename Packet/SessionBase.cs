using System.Net.Sockets;

namespace Packet
{
    public abstract class SessionBase
    {
        public Socket Socket;
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;

        public abstract void OnPacket(int packetId, PacketReader reader);

        public static V Recv<V>(PacketReader r) where V : PacketBase, new()
        {
            var packet = new V();
            packet.Decode(r);
            return packet;
        }

        public void Send(PacketBase packet)
        {
            var writer = new PacketWriter();
            packet.Encode(writer);
            writer.Close(packet.PacketId);
            Socket.Send(writer.Buffer, writer.Pos, SocketFlags.None);
        }
    }
}