using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFmCoinTest2
{
	public partial class Form1 : Form
	{
		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\WFmCoinTest2\Log\NssITKioskLog01.db;Version=3";

		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
			_sysSetting.LogDbConnectionStr = _logDBConnStr;
		}

		private void ShowMsg(string msg)
		{
			this.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(msg))
					txtMsg.AppendText("");
				else
					txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff_fff_f")} - {msg}{"\r\n"}");

				if (txtMsg.Text.Length > 5)
				{
					txtMsg.SelectionStart = txtMsg.Text.Length - 2;
					txtMsg.SelectionLength = 1;
					txtMsg.ScrollToCaret();
					Application.DoEvents();
				}
				
			}));
		}

		private NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK.CX2D3Access _cx2d3 = null;
		private void btnStartCoinMach_Click(object sender, EventArgs e)
		{
			try
			{
				_cx2d3 = new NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK.CX2D3Access();

				ShowMsg($@"Start Coin Machine - Done");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void btnEndCoinMach_Click(object sender, EventArgs e)
		{
			try
			{
				_cx2d3.Dispose();

				ShowMsg($@"End Coin Machine - Done");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void btnDispenseCoin_Click(object sender, EventArgs e)
		{
			try
			{
				bool isMachReady = _cx2d3.CheckMachineIsReady(out bool isMachineQuitProcess, out bool isAppShuttingDown, out bool isLowCoin, 
					out bool isAccessSDKBusy, out bool isRecoveryInProgressAfterDispenseCoin, 
					out string errorMessage);

				if (!isMachReady)
				{
					if (isAppShuttingDown)
						ShowMsg("Coin Machine Application shutting down.");
					else if (isMachineQuitProcess)
						ShowMsg("Coin Machine already quit process");
					else
						ShowMsg("Coin Machine IS NOT ready");
				}
				else
				{
					decimal dispenseAmt = decimal.Parse(txtDispenseAmount.Text);

					if (_cx2d3.GetDispensePossibility(txtDocNo.Text, dispenseAmt, out string lowCoinMsg, out string machOutOfSvc))
					{
						_cx2d3.Dispense(txtDocNo.Text, dispenseAmt);
					}
					else
					{
						ShowMsg($@"lowCoinMsg : {lowCoinMsg} ; machOutOfSvc : {machOutOfSvc}");
					}
				}
					
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}
	}
}
