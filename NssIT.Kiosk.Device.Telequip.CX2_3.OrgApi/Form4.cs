using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public partial class Form4 : Form
	{
		private AxCX2_3 _tq0100X = null;
		private bool _tq0100Connected = false;

		public Form4()
		{
			InitializeComponent();

			_tq0100X = new AxCX2_3();
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
				_tq0100Connected = _tq0100X.ConnectDevice();

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
				//if (_tq0100X.DispenseCoin(txtCoinAmount.Text))
				//{
				//	ShowMsg($@"Coin Dispense Done.");
				//}
				//else
				//{
				//	ShowMsg($@"Coin Dispense Failed.");
				//}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void Form4_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				_tq0100X.CloseDevice();
			}
			catch
			{ }
		}

		private void btnReleaseCOMObj_Click(object sender, EventArgs e)
		{
			try
			{
				_tq0100X.CloseDevice();
				ShowMsg($@"CX2-3 has disposed.");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}
	}
}
