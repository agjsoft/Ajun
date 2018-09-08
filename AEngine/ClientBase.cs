using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AEngine
{
    public abstract class ClientBase
    {
        public Socket Socket = null;
        public object SendLock = new object();
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;

        public abstract void OnConnect(SocketError result);
        public abstract void OnPacket(int packetId, PacketReader reader);

        public void Init(string ip, int port)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
            Socket.ConnectAsync(args);
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnConnect(e.SocketError);
            if (e.SocketError != SocketError.Success)
                return;

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(Buffer, 0, Buffer.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            Socket.ReceiveAsync(args);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (Socket.Connected && e.BytesTransferred > 0)
            {
                Array.Copy(e.Buffer, 0, PacketBuffer, Tail, e.BytesTransferred);
                Tail += e.BytesTransferred;

                while (true)
                {
                    int dataLen = Tail - Head;
                    if (dataLen < 8)
                        break;

                    int packetSize = BitConverter.ToInt32(PacketBuffer, Head);
                    if (dataLen < packetSize)
                        break;

                    OnPacket(BitConverter.ToInt32(PacketBuffer, Head + 4), new PacketReader(PacketBuffer, Head + 8));
                    Head += packetSize;
                }

                Socket.ReceiveAsync(e);
            }
            else
            {
                Socket.Disconnect(false);
                Socket.Dispose();
            }
        }

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