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

        public abstract void OnPacket(PacketId packetId, PacketReader reader);

        public void Send(PacketBase packet)
        {
            PacketWriter pw;
            packet.Encode(out pw);
            pw.Close(packet.PacketId);
            Socket.Send(pw.Buffer, pw.Pos, SocketFlags.None);
        }
    }
}