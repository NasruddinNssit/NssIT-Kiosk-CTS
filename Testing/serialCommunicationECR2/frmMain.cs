using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IM20Base = NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;

namespace serialCommunicationECR2
{
    public partial class frmMain : Form
    {
		frmECR _frmEcr = null;
		public string _comPort = "COM1";

		public frmMain()
		{
			InitializeComponent();
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			_frmEcr = new frmECR();
			string[] ports = SerialPort.GetPortNames();
			cboComPort.Items.AddRange(ports);

			if (cboComPort.Items.Count > 0)
			{
				cboComPort.Text = (string)cboComPort.Items[cboComPort.Items.Count - 1];
			}
		}

		private void btnSale_Click(object sender, EventArgs e)
		{
			this.Hide();
			this.Refresh();
			////////////// ECRip f1 = new ECRip();

			_frmEcr.txtTipAmount.Hide();
			_frmEcr.label7.Hide();
			_frmEcr.label10.Hide();
			_frmEcr.txtQRNo.Hide();
			_frmEcr.command = IM20Base.PayCommand.Norm_Sale;
			_frmEcr.label6.Text = "SALE";
			_frmEcr.label3.Text = "Additional Data";
			_frmEcr.comport = cboComPort.Text;

			_frmEcr.lblDocs.Show();
			_frmEcr.txtDocs.Show();
			_frmEcr.label1.Show();
			_frmEcr.rtxtReceiveMsg.Text = "";
			_frmEcr.txtSendMsg.Show();
			_frmEcr.txtSendMsg.Text = IM20Base.PayCommand.Norm_Sale;
			_frmEcr.label2.Show();
			_frmEcr.label3.Show();
			_frmEcr.txtQRId.Hide();
			_frmEcr.txtQRId.Text = "";
			_frmEcr.txtHostNo.Show();
			_frmEcr.txtHostNo.Text = "00";
			_frmEcr.txtAmount.Show();
			_frmEcr.txtAmount.Text = "100";

			_frmEcr.ReInitForm();
			_frmEcr.Show(this);
		}

		private void btnVoid_Click(object sender, EventArgs e)
		{
			this.Hide();
			this.Refresh();

			_frmEcr.lblDocs.Hide();
			_frmEcr.txtDocs.Hide();
			_frmEcr.label7.Hide();
			_frmEcr.label10.Hide();
			_frmEcr.txtQRNo.Hide();
			_frmEcr.txtTipAmount.Hide();
			_frmEcr.label3.Text = "TRACE NUMBER";
			_frmEcr.command = IM20Base.PayCommand.Norm_Void;
			_frmEcr.label6.Text = "VOID";
			_frmEcr.comport = cboComPort.Text;

			_frmEcr.label1.Show();
			_frmEcr.rtxtReceiveMsg.Text = "";
			_frmEcr.txtSendMsg.Show();
			_frmEcr.txtSendMsg.Text = IM20Base.PayCommand.Norm_Void;
			_frmEcr.label2.Show();
			_frmEcr.label3.Show();
			_frmEcr.txtQRId.Show();
			_frmEcr.txtQRId.Text = "";
			_frmEcr.txtHostNo.Show();
			_frmEcr.txtHostNo.Text = "00";
			_frmEcr.txtAmount.Show();
			_frmEcr.txtAmount.Text = "100";

			_frmEcr.ReInitForm();
			_frmEcr.Show(this);
		}

		private void btnSettlement_Click(object sender, EventArgs e)
		{
			this.Hide();
			this.Refresh();

			_frmEcr.command = IM20Base.PayCommand.Norm_Settlement;

			_frmEcr.lblDocs.Hide();
			_frmEcr.txtDocs.Hide();
			_frmEcr.txtAmount.Hide();
			_frmEcr.txtQRId.Hide();
			_frmEcr.txtTipAmount.Hide();
			_frmEcr.label2.Hide();
			_frmEcr.label3.Hide();
			_frmEcr.label7.Hide();
			_frmEcr.label10.Hide();
			_frmEcr.txtQRNo.Hide();
			_frmEcr.txtAmount.Text = "";
			_frmEcr.label6.Text = "SETTLEMENT";
			_frmEcr.comport = cboComPort.Text;

			_frmEcr.label1.Show();
			_frmEcr.rtxtReceiveMsg.Text = "";
			_frmEcr.txtSendMsg.Show();
			_frmEcr.txtSendMsg.Text = IM20Base.PayCommand.Norm_Settlement;
			_frmEcr.txtHostNo.Show();
			_frmEcr.txtHostNo.Text = "00";

			_frmEcr.ReInitForm();
			_frmEcr.Show(this);
		}

		private void btnEcho_Click(object sender, EventArgs e)
		{
			this.Hide();
			this.Refresh();

			// ECRip f1 = new ECRip();
			_frmEcr.txtHostNo.Hide();
			_frmEcr.txtAmount.Text = "";
			_frmEcr.txtAmount.Hide();
			_frmEcr.txtQRId.Hide();
			_frmEcr.txtTipAmount.Hide();
			_frmEcr.label1.Hide();
			_frmEcr.label2.Hide();
			_frmEcr.label3.Hide();
			_frmEcr.label7.Hide();
			_frmEcr.label10.Hide();
			_frmEcr.txtQRNo.Hide();
			_frmEcr.command = "C902";
			_frmEcr.label6.Text = "ECHO";
			_frmEcr.comport = cboComPort.Text;

			_frmEcr.ReInitForm();
			_frmEcr.Show(this);
		}

		private void btnReadCard_Click(object sender, EventArgs e)
		{

		}

		private void txtExit_Click(object sender, EventArgs e)
		{
			_frmEcr.Close();
			this.Close();
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			PayECRAccess.GetExistingPayECRAccess()?.Dispose();
			_frmEcr.Close();
			Application.Exit();
		}

		//private bool _salesKtmbTestBusy = false;
		private async void btnSalesKtmbTest_Click(object sender, EventArgs e)
        {
			this.Hide();
			this.Refresh();

			string COMPort = cboComPort.Text;
			ResponseInfo cardResponseInfo = null;

			await Task.Run(new Action(() => {

				frmPayWave _paywave = null; 

                try
                {
					_paywave = new frmPayWave(1d, $@"TIDOCX{DateTime.Now.ToString("MMddHHmmss")}", COMPort);
					_paywave.ShowDialog();

					if (_paywave.IsSaleSuccess)
                    {
						cardResponseInfo = _paywave.SaleResult;
						Program.ShowMsg("Success Payment");
					}
                    else
                    {
						Program.ShowMsg("Fail Payment");
					}
				}
				catch (Exception ex)
                {
					Program.ShowMsg(ex.ToString());
                }                
			}));

			this.Show();
			this.Refresh();
		}

        private void btnQuery_Click(object sender, EventArgs e)
        {
			this.Hide();
			this.Refresh();

			_frmEcr.lblDocs.Show();
			_frmEcr.txtDocs.Show();
			_frmEcr.label7.Hide();
			_frmEcr.label10.Hide();
			_frmEcr.txtQRNo.Hide();
			_frmEcr.txtTipAmount.Hide();
			_frmEcr.label3.Text = "TRACE NUMBER";
			_frmEcr.command = IM20Base.PayCommand.Norm_Query;
			_frmEcr.label6.Text = "VOID";
			_frmEcr.comport = cboComPort.Text;

			_frmEcr.label1.Show();
			_frmEcr.rtxtReceiveMsg.Text = "";
			_frmEcr.txtSendMsg.Show();
			_frmEcr.txtSendMsg.Text = IM20Base.PayCommand.Norm_Query;
			_frmEcr.label2.Hide();
			_frmEcr.label3.Hide();
			_frmEcr.txtQRId.Hide();
			_frmEcr.txtQRId.Text = "";
			_frmEcr.txtHostNo.Show();
			_frmEcr.txtHostNo.Text = "00";
			_frmEcr.txtAmount.Hide();
			_frmEcr.txtAmount.Text = "100";

			_frmEcr.ReInitForm();
			_frmEcr.Show(this);
		}
    }
}
