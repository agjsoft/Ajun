using System;
using System.Net;
using System.Net.Sockets;
using Packet;

namespace client
{
    public class PacketEventArgs : EventArgs
    {
        public PacketId PacketId;
        public PacketReader Reader;
    }

    public class Client
    {
        private Socket mSocket = null;
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;
        private event EventHandler<PacketEventArgs> mOnPacket;

        public void Init(EventHandler<PacketEventArgs> packet)
        {
            mOnPacket += new EventHandler<PacketEventArgs>(packet);

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
            mSocket.ConnectAsync(args);
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
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

                    mOnPacket(null, new PacketEventArgs()
                    {
                        PacketId = (PacketId)BitConverter.ToInt32(PacketBuffer, Head + 4),
                        Reader = new PacketReader(PacketBuffer, Head + 8)
                    });
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