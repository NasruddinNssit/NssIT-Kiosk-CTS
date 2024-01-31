using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Client.Base
{
    public partial class frmWaiting : Form
    {
        public frmWaiting()
        {
            InitializeComponent();
        }

        public frmWaiting(string msg)
        {
            InitializeComponent();

            lblMsg.Text = msg;
        }
    }
}
