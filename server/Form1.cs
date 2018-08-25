using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public enum optype
        {
            label1Text,
            listBox1,
        }

        public class op
        {
            public optype type;
            public object data;
        }

        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket;
        private byte[] szData;
        private ConcurrentQueue<op> mQueue = new ConcurrentQueue<op>();
        private List<int> ThreadIdList = new List<int>();

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            m_ClientSocket = new List<Socket>();

            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipep = new IPEndPoint(IPAddress.Any, 10000);
            m_ServerSocket.Bind(ipep);
            m_ServerSocket.Listen(20);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            m_ServerSocket.AcceptAsync(args);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            var clientSocket = e.AcceptSocket;
            m_ClientSocket.Add(clientSocket);

            mQueue.Enqueue(new op()
            {
                type = optype.label1Text,
                data = m_ClientSocket.Count.ToString()
            });

            try
            {
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
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("SocketException : {0}", se.Message));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception : {0}", ex.Message));
            }
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            var ClientSocket = sender as Socket;

            mQueue.Enqueue(new op()
            {
                type = optype.listBox1,
                data = Thread.CurrentThread.ManagedThreadId
            });

            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {
                byte[] szData = e.Buffer;
                var sData = Encoding.Unicode.GetString(szData);

                sData = sData.Replace("\0", "").Trim();
                SetText(sData);
                e.SetBuffer(szData, 0, 1024);
                //Thread.Sleep(500);
                ClientSocket.ReceiveAsync(e);
            }
            else
            {
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                m_ClientSocket.Remove(ClientSocket);
            }

            mQueue.Enqueue(new op()
            {
                type = optype.label1Text,
                data = m_ClientSocket.Count.ToString()
            });
        }

        private delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (richTextBox1.TextLength > 0)
                {
                    richTextBox1.AppendText("\n");
                }

                richTextBox1.AppendText(text);
                richTextBox1.ScrollToCaret();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            op todo;
            if (false == mQueue.TryDequeue(out todo))
                return;

            switch (todo.type)
            {
                case optype.label1Text:
                    {
                        label1.Text = (string)todo.data;
                    }
                    break;
                case optype.listBox1:
                    {
                        int threadId = (int)todo.data;
                        if (ThreadIdList.Contains(threadId))
                            break;

                        ThreadIdList.Add(threadId);
                        listBox1.Items.Add(threadId.ToString());
                    }
                    break;
            }
        }
    }
}