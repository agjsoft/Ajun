using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Packet;

namespace server
{
    public class Session
    {
        public Socket Socket;
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;

        public void Send(PacketBase packet)
        {
            PacketWriter pw;
            packet.Encode(out pw);
            pw.Close(packet.PacketId);
            Socket.Send(pw.Buffer, pw.Pos, SocketFlags.None);
        }
    }

    public class PacketEventArgs : EventArgs
    {
        public Session Session;
        public PacketReader Reader;
    }

    public class Server
    {
        private Socket mSocket;
        private Dictionary<Socket, Session> mSessionMap = new Dictionary<Socket, Session>();
        private event EventHandler mOnAccept;
        private event EventHandler mOnReceive;
        private event EventHandler<PacketEventArgs> mOnPacket;

        public void Init(int port, EventHandler accept, EventHandler receive, EventHandler<PacketEventArgs> packet)
        {
            mOnAccept += new EventHandler(accept);
            mOnReceive += new EventHandler(receive);
            mOnPacket += new EventHandler<PacketEventArgs>(packet);

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            mSocket.Listen(20);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            mSocket.AcceptAsync(args);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            var session = new Session()
            {
                Socket = e.AcceptSocket
            };
            mSessionMap.Add(session.Socket, session);

            mOnAccept(mSessionMap.Count, null);

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(session.Buffer, 0, session.Buffer.Length);
            args.UserToken = session;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            session.Socket.ReceiveAsync(args);

            e.AcceptSocket = null;
            mSocket.AcceptAsync(e);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            var session = e.UserToken as Session;
            var socket = sender as Socket;

            mOnReceive(Thread.CurrentThread.ManagedThreadId, null);

            if (socket.Connected && e.BytesTransferred > 0)
            {
                Array.Copy(e.Buffer, 0, session.PacketBuffer, session.Tail, e.BytesTransferred);
                session.Tail += e.BytesTransferred;

                while (true)
                {
                    int dataLen = session.Tail - session.Head;
                    if (dataLen < 8)
                        break;

                    int packetSize = BitConverter.ToInt32(session.PacketBuffer, session.Head);
                    if (dataLen < packetSize)
                        break;

                    int packetId = BitConverter.ToInt32(session.PacketBuffer, session.Head + 4);
                    mOnPacket(null, new PacketEventArgs()
                    {
                        Session = session,
                        Reader = new PacketReader(packetId, session.PacketBuffer, session.Head + 8)
                    });
                    session.Head += packetSize;
                }

                socket.ReceiveAsync(e);
            }
            else
            {
                socket.Disconnect(false);
                socket.Dispose();
                mSessionMap.Remove(socket);
            }
        }
    }
}