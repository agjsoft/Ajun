using System;
using System.Net;
using System.Net.Sockets;
using Packet;

namespace client
{
    public abstract class ClientBase
    {
        private Socket mSocket = null;
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;

        public abstract void OnConnect();
        public abstract void OnPacket(PacketId packetId, PacketReader reader);

        public void Init(string ip, int port)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
            mSocket.ConnectAsync(args);
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnConnect();

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(Buffer, 0, Buffer.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            mSocket.ReceiveAsync(args);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (mSocket.Connected && e.BytesTransferred > 0)
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

                    OnPacket(
                        (PacketId)BitConverter.ToInt32(PacketBuffer, Head + 4),
                        new PacketReader(PacketBuffer, Head + 8));
                    Head += packetSize;
                }

                mSocket.ReceiveAsync(e);
            }
            else
            {
                mSocket.Disconnect(false);
                mSocket.Dispose();
            }
        }

        public void Send(PacketBase packet)
        {
            PacketWriter pw;
            packet.Encode(out pw);
            pw.Close(packet.PacketId);
            mSocket.Send(pw.Buffer, pw.Pos, SocketFlags.None);
        }
    }
}