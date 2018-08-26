using System;
using System.Net;
using System.Net.Sockets;

namespace Packet
{
    public class SessionBase
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

    public abstract class ServerBase<T> where T : SessionBase, new()
    {
        private Socket mSocket;

        public abstract void OnAccept(T session);
        public abstract void OnDisconnect(T session);
        public abstract void OnPacket(T session, PacketId packetId, PacketReader reader);

        public void Init(int port)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            mSocket.Listen(20);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            mSocket.AcceptAsync(args);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            var session = new T()
            {
                Socket = e.AcceptSocket
            };

            OnAccept(session);

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
            var session = e.UserToken as T;
            var socket = sender as Socket;

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

                    OnPacket(session,
                        (PacketId)BitConverter.ToInt32(session.PacketBuffer, session.Head + 4),
                        new PacketReader(session.PacketBuffer, session.Head + 8));
                    session.Head += packetSize;
                }

                socket.ReceiveAsync(e);
            }
            else
            {
                OnDisconnect(session);
                socket.Disconnect(false);
                socket.Dispose();
            }
        }
    }
}