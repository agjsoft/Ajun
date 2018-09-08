using System;
using System.Windows.Forms;

namespace TokyoServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Server mServer = new Server();

        private void Form1_Load(object sender, EventArgs e)
        {
            mServer.Init(12500);
        }
    }
}