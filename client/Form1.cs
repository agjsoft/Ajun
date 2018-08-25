using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Packet;

namespace client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Client> mClientList = new List<Client>();

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new Client();
            client.Init(OnPacket);

            var packet = new LoginReqPacket();
            packet.Id = "apple";
            packet.Pw = "wpvpxh12#$";
            client.Send(packet);

            mClientList.Add(client);
        }

        private void OnPacket(object sender, EventArgs e)
        {
            var pr = (PacketReader)sender;
            switch ((PacketId)pr.GetPacketId())
            {
                case PacketId.LoginAck:
                    {
                        var packet = new LoginAckPacket(pr);
                        int ret = packet.Result;
                        string msg = packet.Message;
                        long id = packet.AccountId;
                    }
                    break;
            }
        }
    }
}