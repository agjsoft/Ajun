using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace client
{
    public class Client
    {
        private Socket mSocket = null;

        public void Init()
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
            mSocket.ConnectAsync(args);
        }

        public void Work()
        {
            //var args = new SocketAsyncEventArgs();
            //args.SetBuffer(szData, 0, szData.Length);

            Task.Run(() =>
            {
                while (true)
                {
                    if (mSocket.Connected)
                        break;
                    Thread.Sleep(1);
                }

                while (true)
                {
                    var packet = new LoginReqPacket();
                    packet.Result = 33;
                    SendPacket(7700, packet);
                    Thread.Sleep(200);
                }
            });
        }

        public void SendPacket(int packetId, IPacket packet)
        {
            PacketWriter pw;
            packet.Encode(out pw);
            pw.Close(packetId);
            mSocket.Send(pw.Buffer, pw.Pos, SocketFlags.None);
        }
    }

    public interface IPacket
    {
        void Encode(out PacketWriter writer);
    }

    public class LoginReqPacket : IPacket
    {
        public int Result;
        public string Message;
        public long AccountId;

        public void Encode(out PacketWriter writer)
        {
            writer = new PacketWriter();
            writer.SetInt(Result);
            if (0 != Result)
                return;

            writer.SetString(Message);
            writer.SetLong(AccountId);
        }

        public LoginReqPacket()
        {
        }
    }

    public class PacketWriter
    {
        public byte[] Buffer = new byte[1024];
        public int Pos = 8;

        public void Close(int packetId)
        {
            Array.Copy(BitConverter.GetBytes(Pos), 0, Buffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(packetId), 0, Buffer, 4, sizeof(int));
        }

        public void SetShort(short val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(int));
            Pos += sizeof(int);
        }

        public void SetInt(int val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(int));
            Pos += sizeof(int);
        }

        public void SetLong(long val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, Buffer, Pos, sizeof(long));
            Pos += sizeof(long);
        }

        public void SetString(string val)
        {
            var bytes = Encoding.UTF8.GetBytes(val);
            SetInt(bytes.Length);
            Array.Copy(bytes, 0, Buffer, Pos, bytes.Length);
            Pos += bytes.Length;
        }
    }
}