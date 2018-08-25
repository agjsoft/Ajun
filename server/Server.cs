using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace server
{
    public class Server
    {
        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket = new List<Socket>();
        private byte[] szData;
        public event EventHandler OnAccept;
        public event EventHandler OnReceive;

        public void Init()
        {
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 10000));
            m_ServerSocket.Listen(20);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            m_ServerSocket.AcceptAsync(args);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            var clientSocket = e.AcceptSocket;
            m_ClientSocket.Add(clientSocket);

            OnAccept(m_ClientSocket.Count, null);

            if (m_ClientSocket != null)
            {
                var args = new SocketAsyncEventArgs();

                szData = new byte[1024];
                args.SetBuffer(szData, 0, szData.Length);
                args.UserToken = m_ClientSocket;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                clientSocket.ReceiveAsync(args);
            }

            e.AcceptSocket = null;
            m_ServerSocket.AcceptAsync(e);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            var ClientSocket = sender as Socket;

            OnReceive(Thread.CurrentThread.ManagedThreadId, null);

            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {
                byte[] szData = e.Buffer;
                var sData = Encoding.Unicode.GetString(szData);

                sData = sData.Replace("\0", "").Trim();
                e.SetBuffer(szData, 0, 1024);
                Thread.Sleep(500);
                ClientSocket.ReceiveAsync(e);
            }
            else
            {
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                m_ClientSocket.Remove(ClientSocket);
            }
        }
    }
}
