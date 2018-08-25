using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace server
{
    public class Session
    {
        public Socket Socket;
        public byte[] Buffer = new byte[1024];
        public byte[] PacketBuffer = new byte[8192];
        public int Head = 0;
        public int Tail = 0;
    }

    public class Server
    {
        private Socket mSocket;
        private Dictionary<Socket, Session> mSessionMap = new Dictionary<Socket, Session>();
        private event EventHandler mOnAccept;
        private event EventHandler mOnReceive;
        private event EventHandler mOnPacket;

        public void Init(int port, EventHandler accept, EventHandler receive, EventHandler packet)
        {
            mOnAccept += new EventHandler(accept);
            mOnReceive += new EventHandler(receive);
            mOnPacket += new EventHandler(packet);

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
                    if (dataLen - 4 < packetSize)
                        break;

                    int packetId = BitConverter.ToInt32(session.PacketBuffer, session.Head + 4);
                    mOnPacket(new PacketReader(packetId, session.PacketBuffer, session.Head + 8), null);
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

    public class UpdateNameReqPacket
    {
        public UpdateNameReqPacket()
        {
        }

        public UpdateNameReqPacket(PacketReader reader)
        {

        }
    }

    public class LoginReqPacket
    {
        public int Result;
        public string Message;
        public long AccountId;

        public LoginReqPacket()
        {
        }

        public LoginReqPacket(PacketReader reader)
        {
            Result = reader.GetInt();
            if (0 != Result)
                return;

            Message = reader.GetString();
            AccountId = reader.GetLong();
        }
    }

    public enum PacketId
    {
        LoginReq = 7700,
        LoginAck,
        UpdateNameReq,
        UpdateNameAck,
    }

    public class PacketReader
    {
        private int PacketId;
        private byte[] Buffer;
        private int Pos;

        public PacketReader(int packetId, byte[] buffer, int pos)
        {
            PacketId = packetId;
            Buffer = buffer;
            Pos = pos;
        }

        public int GetPacketId()
        {
            return PacketId;
        }

        public short GetShort()
        {
            short val = BitConverter.ToInt16(Buffer, Pos);
            Pos += sizeof(short);
            return val;
        }

        public int GetInt()
        {
            int val = BitConverter.ToInt32(Buffer, Pos);
            Pos += sizeof(int);
            return val;
        }

        public long GetLong()
        {
            long val = BitConverter.ToInt64(Buffer, Pos);
            Pos += sizeof(long);
            return val;
        }

        public float GetFloat()
        {
            float val = BitConverter.ToSingle(Buffer, Pos);
            Pos += sizeof(float);
            return val;
        }

        public double GetDouble()
        {
            double val = BitConverter.ToDouble(Buffer, Pos);
            Pos += sizeof(double);
            return val;
        }

        public string GetString()
        {
            int len = GetInt();
            string val = Encoding.UTF8.GetString(Buffer, Pos, len);
            Pos += len;
            return val;
        }
    }
}
