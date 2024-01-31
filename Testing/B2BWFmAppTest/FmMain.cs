using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace B2BWFmAppTest
{
	public partial class FmMain : Form
	{
		FmB2BUI _payUi = null;
		
		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\B2BWFmAppTest\LogDB\NssITKioskLog01.db;Version=3";

		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		public FmMain()
		{
			InitializeComponent();
		}

		private void FmMain_Load(object sender, EventArgs e)
		{
			_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
			_sysSetting.LogDbConnectionStr = _logDBConnStr;

			_payUi = new FmB2BUI();
		}

		private void btnCashMachineStatus_Click(object sender, EventArgs e)
		{
			try
			{
				if (_payUi.IsMachineReady())
					ShowMsg("Cash Machine Is Ready");
				else
					ShowMsg("Cash Machine IS NOT Ready");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void btnStartPayment_Click(object sender, EventArgs e)
		{
			try
			{
				if (_payUi.IsMachineReady() == false)
				{
					throw new Exception("Cash Machine is not ready..");
				}

				decimal amount = 0.00M;

				ShowMsg(); ShowMsg(); ShowMsg();

				if (decimal.TryParse(txtAmount.Text, out amount) == false)
					throw new Exception("Invalid Amount value.");

				if (amount <= 0.999999999999M)
					throw new Exception("Amount must greated or equal 1.00.");

				if (string.IsNullOrWhiteSpace(txtDocNo.Text))
					throw new Exception("Invalid Document No.");

				amount = (Math.Floor((amount * 100.00M))) / 100M;
				txtAmount.Text = amount.ToString();

				_payUi.Show();

				Task<bool> resTask = _payUi.StartPayment(txtDocNo.Text.Trim(), amount);

				resTask.Wait();

				if (resTask.Result == false)
					throw new Exception("Unable to start new Transaction; System busy;");

			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void btnClearMsg_Click(object sender, EventArgs e)
		{

		}

		private void ShowMsg(string msg = null)
		{
			DateTime tm = DateTime.Now;
			msg = (msg ?? "").Trim();

			this.Invoke(new Action(() => {
				if (msg.Length > 0)
					txtMsg.AppendText($@"{tm.ToString("HH:mm:ss.fff")}-{msg}{"\r\n"}");

				else if (txtMsg.Text.Length > 1)
					txtMsg.AppendText("\r\n");

				if (txtMsg.TextLength > 5)
				{
					txtMsg.SelectionStart = txtMsg.TextLength - 2;
					txtMsg.SelectionLength = 1;
					txtMsg.ScrollToCaret();
				}
			}));
		}

		private void FmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			_payUi.Close();
		}
	}
}