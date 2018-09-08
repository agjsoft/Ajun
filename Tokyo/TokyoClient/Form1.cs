using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using TokyoPacket;

namespace TokyoClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Client> ClientList = new List<Client>();

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new Client();
            client.Init("127.0.0.1", 12500);
            ClientList.Add(client);

            Task.Run(() =>
            {
                while (true)
                {
                    client.SendSync(new LoginReqPacket()
                    {
                        Id = "apple",
                        Pw = "banana"
                    });
                    Thread.Sleep(1);
                }
            });
        }
    }
}