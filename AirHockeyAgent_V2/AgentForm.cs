using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirHockeyAgent
{
    public partial class AgentForm : Form
    {
        GameManager GM;
        int mPort = 1234;
        string mIP = @"127.0.0.1";
        short mID = 1;
        string mLogFileName = @"c:\temp\log.txt";
        
        public AgentForm()
        {
            InitializeComponent();
        }

        private void AgentForm_Load(object sender, EventArgs e)
        {
            GM = new GameManager();
            GM.Init();

            IPTxt.Text = mIP;
            IDTxt.Text = mID.ToString();
            LogTxt.Text = mLogFileName;

            LoginBtn.Enabled = true;
            LogoutBtn.Enabled = false;
        }

        private void AgentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GM.Clean();
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            if (GM.Connect(mPort, mIP, mID, mLogFileName) == false)
                Application.Exit();

            LoginBtn.Enabled = false;
            IPTxt.Enabled = false;
            LogoutBtn.Enabled = true;
            IDTxt.Enabled = false;
            LogTxt.Enabled = false;
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            GM.Disconnect();

            LoginBtn.Enabled = true;
            IPTxt.Enabled = true;
            LogoutBtn.Enabled = false;
            IDTxt.Enabled = true;
            LogTxt.Enabled = true;
        }

        private void IPTxt_TextChanged(object sender, EventArgs e)
        {
            mIP = IPTxt.Text;
            if (IPTxt.Text != string.Empty)
                LoginBtn.Enabled = true;
            else
                LoginBtn.Enabled = false;
        }

        private void IDTxt_TextChanged(object sender, EventArgs e)
        {
            if (IDTxt.Text != string.Empty)
            {
                LoginBtn.Enabled = true;
                mID = Convert.ToInt16(IDTxt.Text);
            }
            else
                LoginBtn.Enabled = false;
        }

        private void LogTxt_TextChanged(object sender, EventArgs e)
        {
            mLogFileName = LogTxt.Text;
            if (LogTxt.Text != string.Empty)
                LoginBtn.Enabled = true;
            else
                LoginBtn.Enabled = false;
        }
    }
}
