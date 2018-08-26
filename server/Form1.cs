﻿using System;
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

        public class Session : SessionBase
        {
            public string Guid;
        }

        public class Server : ServerBase<Session>
        {
            public ConcurrentDictionary<string, Session> UserMap = new ConcurrentDictionary<string, Session>();

            public override void OnAccept(Session session)
            {
                session.Guid = Guid.NewGuid().ToString("N");
                UserMap.TryAdd(session.Guid, session);
            }

            public override void OnDisconnect(Session session)
            {
                Session tmp;
                UserMap.TryRemove(session.Guid, out tmp);
            }

            public override void OnPacket(Session session, PacketId packetId, PacketReader reader)
            {
                switch (packetId)
                {
                    case PacketId.LoginReq:
                        {
                            var packet = new LoginReqPacket(reader);
                            string id = packet.Id;
                            string pw = packet.Pw;

                            var sendPacket = new LoginAckPacket();
                            sendPacket.Result = 0;
                            sendPacket.Message = "Success";
                            sendPacket.AccountId = 1982;
                            for (int i = 100; i < 200; i++)
                            {
                                sendPacket.Inven.Add(new Item()
                                {
                                    Id = i,
                                    Expired = DateTime.Now,
                                    Count = i % 5
                                });
                            }
                            session.Send(sendPacket);
                        }
                        break;
                    case PacketId.UpdateNameReq:
                        {
                            var packet = new UpdateNameReqPacket(reader);
                        }
                        break;
                }
            }
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