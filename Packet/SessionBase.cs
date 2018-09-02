using System.Net.Sockets;
using System.Threading.Tasks;

namespace Packet
{
    public abstract class SessionBase
    {
        public Socket Socket;
        public object SendLock = new object();
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;

        public abstract void OnPacket(int packetId, PacketReader reader);

        private void SendCommon(PacketBase packet)
        {
            var writer = new PacketWriter();
            packet.Encode(writer);
            writer.Close(packet.PacketId);
            lock (SendLock)
            {
                Socket.Send(writer.Buffer, writer.Pos, SocketFlags.None);
            }
        }

        public void SendSync(PacketBase packet)
        {
            SendCommon(packet);
        }

        public void SendAsync(PacketBase packet)
        {
            Task.Run(() => SendCommon(packet));
        }
    }
}