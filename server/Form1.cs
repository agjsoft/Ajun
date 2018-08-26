using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using Packet;

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
            mServer.Init(10000);
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