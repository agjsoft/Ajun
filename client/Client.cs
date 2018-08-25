using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Packet;

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
                    packet.Result = 0;
                    packet.Message = "하하하";
                    packet.AccountId = 19820514;
                    SendPacket(PacketId.LoginReq, packet);
                    Thread.Sleep(200);
                }
            });
        }

        public void SendPacket(PacketId packetId, IPacket packet)
        {
            PacketWriter pw;
            packet.Encode(out pw);
            pw.Close(packetId);
            mSocket.Send(pw.Buffer, pw.Pos, SocketFlags.None);
        }
    }
}