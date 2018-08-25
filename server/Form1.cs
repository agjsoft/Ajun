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
        public enum optype
        {
            label1,
            listBox1,
        }

        public class op
        {
            public optype type;
            public object data;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private ConcurrentQueue<op> mQueue = new ConcurrentQueue<op>();
        private List<int> ThreadIdList = new List<int>();
        private Server mServer = new Server();

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            mServer.Init(10000, OnAccept, OnReceive, OnPacket);
        }

        private void OnAccept(object sender, EventArgs e)
        {
            mQueue.Enqueue(new op()
            {
                type = optype.label1,
                data = (int)sender
            });
        }

        private void OnPacket(object sender, EventArgs e)
        {
            var pr = (PacketReader)sender;
            switch ((PacketId)pr.GetPacketId())
            {
                case PacketId.LoginReq:
                    {
                        var packet = new LoginReqPacket(pr);
                    }
                    break;
                case PacketId.UpdateNameReq:
                    {
                        var packet = new UpdateNameReqPacket(pr);
                    }
                    break;
            }
        }

        private void OnReceive(object sender, EventArgs e)
        {
            mQueue.Enqueue(new op()
            {
                type = optype.listBox1,
                data = (int)sender
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 128; i++)
            {
                op todo;
                if (false == mQueue.TryDequeue(out todo))
                    return;

                switch (todo.type)
                {
                    case optype.label1:
                        {
                            label1.Text = ((int)todo.data).ToString();
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
}