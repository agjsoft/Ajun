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
                var szData = Encoding.Unicode.GetBytes("apple_banana_candy_desert_eagle__" + mSocket.LocalEndPoint);
                while (true)
                {
                    mSocket.Send(szData);
                    Thread.Sleep(200);
                }
            });
        }
    }
}
