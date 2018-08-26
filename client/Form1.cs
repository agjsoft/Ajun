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
            client.Init("127.0.0.1", 10000);

            var packet = new LoginReqPacket();
            packet.Id = "apple";
            packet.Pw = "wpvpxh12#$";
            client.Send(packet);

            mClientList.Add(client);
        }
    }

    public class Client : ClientBase
    {
        public override void OnConnect()
        {
        }

        public override void OnPacket(PacketId packetId, PacketReader reader)
        {
            switch (packetId)
            {
                case PacketId.LoginAck:
                    {
                        var packet = new LoginAckPacket(reader);
                        int ret = packet.Result;
                        string msg = packet.Message;
                        long id = packet.AccountId;
                    }
                    break;
            }
        }
    }
}