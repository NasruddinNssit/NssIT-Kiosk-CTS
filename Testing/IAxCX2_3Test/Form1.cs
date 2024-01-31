using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IAxCX2_3Test
{
	public partial class Form1 : Form
	{
		NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.IAxCX2_3 _cx2_3 = null;
		private bool _tq0100Connected = false;

		public Form1()
		{
			InitializeComponent();

			//_cx2_3 = NssIT.Kiosk.Telequip.CX2_3.TheHelper.NewAxCX2_3();

			_cx2_3 = new NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.AxCX2_3();
		}
		
		private void ShowMsg(string msg)
		{
			msg = msg ?? "--";
			txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {msg}{"\r\n"}");
			Application.DoEvents();
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			ShowMsg("");
			ShowMsg("Connecting Telequip CX2-3 Coin Machine .. ");

			try
			{
				_tq0100Connected = _cx2_3.ConnectDevice();

				if (_tq0100Connected)
				{ ShowMsg($@"tq0100 connected."); }
				else
				{ ShowMsg($@"tq0100 not connected."); }
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}
		}

		private void btnDispense01_Click(object sender, EventArgs e)
		{
			try
			{
				int totalDispenseCents = Convert.ToInt32(Math.Floor(decimal.Parse(txtCoinAmount.Text) * 100M));

				if (_cx2_3.DispenseCoin(totalDispenseCents, out string notEnoughCoinMsg))
				{
					ShowMsg($@"Coin Dispense Done.");
				}
				else
				{
					ShowMsg($@"Coin Dispense Failed.{notEnoughCoinMsg}");
				}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				_cx2_3.CloseDevice();
			}
			catch
			{ }
		}

		private void btnReleaseCOMObj_Click(object sender, EventArgs e)
		{
			try
			{
				_cx2_3.CloseDevice();
				ShowMsg($@"CX2-3 has disposed.");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}
	}
}
