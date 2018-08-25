using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        private Socket mSocket = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
            mSocket.ConnectAsync(args);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length <= 0)
                return;

            var szData = Encoding.Unicode.GetBytes(textBox1.Text);

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(szData, 0, szData.Length);
            
            textBox1.Text = "";
            textBox1.Focus();

            Task.Run(() =>
            {
                while (true)
                {
                    mSocket.Send(szData);
                    Thread.Sleep(1);
                }
            });

            // mSocket.SendAsync(args);
        }
    }
}