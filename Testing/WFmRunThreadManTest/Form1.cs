using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFmRunThreadManTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTest1_Click(object sender, EventArgs e)
        {
            TestingController.Start();
        }

        private void btnShowNoOfRunThreadMan_Click(object sender, EventArgs e)
        {
            TestingController.ShowAllThreadManStates();
        }

        private void btnClearRunThreadManList_Click(object sender, EventArgs e)
        {
            TestingController.ClearAllThreadManStates();
        }

        private void btnAbortTesting_Click(object sender, EventArgs e)
        {
            TestingController.AbortThreadManStates();
        }

        private void btnWaitToEnd_Click(object sender, EventArgs e)
        {
            TestingController.WaitToEndThreadMan();
        }
    }
}
