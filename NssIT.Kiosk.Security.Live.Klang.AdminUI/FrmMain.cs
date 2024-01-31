using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Security.Live.Klang.AdminUI
{
    public partial class FrmMain : Form
    {
        private LiveKlangRegistrySetting _liveRegSett = new LiveKlangRegistrySetting();

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnLiveVerify_Click(object sender, EventArgs e)
        {
            try
            {
                _liveRegSett.VerifyRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Live.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnLiveWrite_Click(object sender, EventArgs e)
        {
            try
            {
                _liveRegSett.WriteRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Live.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
