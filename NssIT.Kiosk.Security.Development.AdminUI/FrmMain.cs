using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Security.Development.AdminUI
{
    public partial class FrmMain : Form
    {
        private DevelopmentRegistrySetting _devRegSett = new DevelopmentRegistrySetting();
        //private StagingRegistrySetting _stageRegSett = new StagingRegistrySetting();
        private LocalHostRegistrySetting _localHostRegSett = new LocalHostRegistrySetting();

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnDevVerify_Click(object sender, EventArgs e)
        {
            try
            {
                _devRegSett.VerifyRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Dev.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnDevWrite_Click(object sender, EventArgs e)
        {
            try
            {
                _devRegSett.WriteRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Dev.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnLocalHostVerify_Click(object sender, EventArgs e)
        {
            try
            {
                _localHostRegSett.VerifyRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Local_Host.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnLocalHostWrite_Click(object sender, EventArgs e)
        {
            try
            {
                _localHostRegSett.WriteRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Local_Host.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
