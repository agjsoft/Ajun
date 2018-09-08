using System;
using System.Net;
using System.Net.Sockets;

namespace AEngine
{
    public abstract class ServerBase<T> where T : SessionBase, new()
    {
        private Socket mSocket;

        public abstract void OnAccept(T session);
        public abstract void OnDisconnect(T session);

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

                    session.OnPacket(
                        BitConverter.ToInt32(session.PacketBuffer, session.Head + 4),
                        new PacketReader(session.PacketBuffer, session.Head + 8));
                    session.Head += packetSize;
                }

                socket.ReceiveAsync(e);
            }
            else
            {
                OnDisconnect(session);
                mSocket.Disconnect(false);
                mSocket.Dispose();
            }
        }
    }
}