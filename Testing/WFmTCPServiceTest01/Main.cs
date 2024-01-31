using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WFmTCPServiceTest01
{
	public partial class Main : Form
	{
		LocalTcpService _tcpService = null;

		public Main()
		{
			InitializeComponent();
		}

		private void Main_Load(object sender, EventArgs e)
		{ }

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				_tcpService?.ShutdownService();
			}
			catch 
			{ }
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				int serverPort = Convert.ToInt32(txtLocSvcPort.Text);

				_tcpService = new LocalTcpService(serverPort);

				_tcpService.OnDataReceived += _tcpService_OnDataReceived;

				ShowMsg($@"Start TCP Service. Local Service Port: {_tcpService.LocalServicePort}");

				btnStart.Visible = false;
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void btnEnd_Click(object sender, EventArgs e)
		{
			try
			{
				_tcpService?.ShutdownService();
				ShowMsg("End TCP Service.");
				btnStart.Visible = true;
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void _tcpService_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if ((e != null) && (e.ReceivedData != null) && (e.ReceivedData.MsgObject != null))
			{
				int originPort = e.ReceivedData.OriginalServicePort;

				if (e.ReceivedData.MsgObject.GetType().Equals(typeof(UINewPayment)))
				{
					UINewPayment nPay = (UINewPayment)e.ReceivedData.MsgObject;
					ShowMsg($@"Origin Port: {originPort}; Customer Message: {nPay.CustmerMsg}; Processing Message: {nPay.ProcessMsg}; Price: {nPay.Price}; Log Time: {nPay.TimeStamp.ToString("HH:mm:ss.fff_fff_f")}");

					bool isPortFound = false;
					foreach (object tt1 in cboClientPortList.Items)
					{
						int xPort = (int)tt1;

						if (originPort == (int)tt1)
						{
							isPortFound = true;
							break;
						}
					}
					
					if (!isPortFound)
					{
						this.Invoke(new Action(() => {
							cboClientPortList.Items.Add(originPort);
						}));						
					}
				}
			}
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(cboClientPortList.Text))
					throw new Exception("Please select a Destination Port");
				
				int destPort = Convert.ToInt32(cboClientPortList.Text);

				if (destPort <= 0)
					throw new Exception("Invalid Destination Port specification");

				Guid netProcessId = Guid.NewGuid();
				_tcpService.SendMsgPack(new NetMessagePack(netProcessId)
				{
					DestinationPort = destPort,
					MsgObject = new UINewPayment(netProcessId, "--", DateTime.Now)
					{
						CustmerMsg = "Try Customer Message",
						ProcessMsg = txtSendMsg.Text,
						//ProcessMsg = GetLargeString2(),
						Price = 45.20M
					}
				});
				ShowMsg("End Send Data.");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void ShowMsg(string msg)
		{
			msg = (string.IsNullOrWhiteSpace(msg)) ? null : msg;

			this.Invoke(new Action(() => {
				if (msg == null)
					txtMsg.AppendText("\r\n");
				else
				{
					txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff_fff_f")} - {msg}{"\r\n"}");

					if (txtMsg.TextLength > 5)
					{
						txtMsg.SelectionStart = txtMsg.TextLength - 2;
						txtMsg.SelectionLength = 1;
						txtMsg.ScrollToCaret();
					}
					
				}
			}));
		}

		private string GetLargeString2()
		{
			string retStr = File.ReadAllText(@"C:\temp\tt5.txt");

			return retStr;
		}

		private string GetLargeString()
		{
			string retStr = @"
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
3200-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_

320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
320-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
6400-abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_abcdefghijklmnopqrstuvwxyz123456_
";
			for (int inx = 0; inx <= 192; inx++)
				retStr += retStr;

			return retStr;
		}
	}
}
