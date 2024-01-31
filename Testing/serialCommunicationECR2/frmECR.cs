using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
using IM20Base = NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;

namespace serialCommunicationECR2
{
    public partial class frmECR : Form
    {
		public delegate void FormDelegateReceive(TrxCallBackEventArgs e);
		public delegate void PayWaveProgress(InProgressEventArgs e);

		public FormDelegateReceive formDelegate1;
		public PayWaveProgress payWaveProgress1;

		public String command = "";
		public string comport = "COM1";

		PayECRAccess _payECRAcc = null;

		public frmECR()
		{
			InitializeComponent();
		}

		#region -- Events --

		private void frmECR_Load(object sender, EventArgs e)
		{
			lbl_portname.Text = comport;
			//ecr = new ECR(comport, 120000, 1000, "C:\\ECR_LOG", true, true);
			txtSendMsg.Text = command;
			txtHostNo.Text = "00";
			txtAmount.Text = "100";
			formDelegate1 = new FormDelegateReceive(FormReceiveMsg);
			payWaveProgress1 = new PayWaveProgress(PayWaveProg);
		}

		private void btnSendData_Click(object sender, EventArgs e)
		{
			try
			{
				if (command.Equals(IM20Base.PayCommand.Norm_Sale))
				{
					_payECRAcc.Pay(Guid.NewGuid().ToString(), txtAmount.Text.Trim(), IM20Base.AccountType.CreditCard, txtDocs.Text.Trim(), txtQRNo.Text.Trim(), txtDocs.Text.Trim());
				}
				else if (command.Equals(IM20Base.PayCommand.Norm_Void))
				{
					_payECRAcc.VoidPayment(Guid.NewGuid().ToString(), txtAmount.Text.Trim(), txtHostNo.Text.Trim(), IM20Base.AccountType.CreditCard, txtQRId.Text.Trim(), txtQRNo.Text.Trim());
				}
				else if (command.Equals(IM20Base.PayCommand.Norm_Settlement))
				{
					_payECRAcc.SettlePayment(Guid.NewGuid().ToString(), txtHostNo.Text.Trim());
				}
				else if (command.Equals(IM20Base.PayCommand.Base_Echo))
				{
					_payECRAcc.Echo();
				}
				else if (command.Equals(IM20Base.PayCommand.Norm_Query))
				{
					_payECRAcc.QueryCardResponse(txtHostNo.Text.Trim(), txtDocs.Text.Trim());
				}
				else
				{
					Program.ShowMsg("Pay Command not supported.");
				}
			}
			catch (Exception ex)
			{
				Program.ShowMsg(ex.ToString());
			}

		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			HideForm();
		}

		private void frmECR_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.Visible)
			{
				HideForm();
				e.Cancel = true;
			}
		}

		private void btnClsMsg_Click(object sender, EventArgs e)
		{
			rtxtReceiveMsg.Text = "";
		}

		private void btnCancelTrans_Click(object sender, EventArgs e)
		{
			try
			{
				_payECRAcc.CancelRequest();
			}
			catch (Exception ex)
			{
				Program.ShowMsg(ex.ToString());
			}
		}

		#endregion ~.~*~.~

		#region -- Private --

		public void ReInitForm()
		{
			if (_payECRAcc == null)
			{
				_payECRAcc = PayECRAccess.GetPayECRAccess(comport, 300000); /* , @"C:\ECR_Receipts\", @"C:\ECR_LOG", true, true); */
				_payECRAcc.OnCompleteCallback += PayECRCallBack;
				_payECRAcc.OnInProgressCall += PayECRInProgressCall;
			}
			lblCheck.Text = DateTime.Now.ToString("HH:mm:ss fff");

			txtSendMsg.Text = command;
		}

		private void PayECRInProgressCall(object sender, InProgressEventArgs e)
		{
			if (payWaveProgress1 != null)
				this.Invoke(payWaveProgress1, e);
		}


		private void PayECRCallBack(object sender, TrxCallBackEventArgs e)
		{
			if (formDelegate1 != null)
				this.Invoke(formDelegate1, e);
		}

		private void PayWaveProg(InProgressEventArgs e)
		{
			lblProgressMsg.Text = e.Message ?? "";
		}

		public void FormReceiveMsg(TrxCallBackEventArgs e)
		{
			if (e.Error != null)
			{
				rtxtReceiveMsg.Text += "Time : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n"
					+ "Error\r\n"
					+ e.Error.ToString();
			}

			if (e.Result != null)
			{
				if (e.IsSuccess)
				{
					rtxtReceiveMsg.Text += "SUCCESS" + "\r\n" + "\r\n";
				}
				else
				{
					rtxtReceiveMsg.Text += "ERROR" + "\r\n" + "\r\n";
				}

				rtxtReceiveMsg.Text += "Host : \t" + (e.Result.HostNo ?? "") + "\r\n"
				+ "TID : \t" + (e.Result.TID ?? "") + "\r\n"
				+ "MID : \t" + (e.Result.MID ?? "") + "\r\n"
				+ "ResponseMsg : \t" + (e.Result.ResponseMsg ?? "") + "\r\n"
				+ "Additional Data : \t" + (e.Result.AdditionalData ?? "") + "\r\n"
				+ "Document Numbers : \t" + (e.Result.DocumentNumbers ?? "") + "\r\n"
				+ "Card No : \t" + (e.Result.CardNo ?? "") + "\r\n"
				+ "Card Type : \t" + (e.Result.CardType ?? "") + "\r\n"
				+ "Batch No : \t" + (e.Result.BatchNumber ?? "") + "\r\n"
				+ "Transaction Trace No : \t" + (e.Result.TransactionTrace ?? "") + "\r\n"
				+ "Time : \t" + e.Result.ReportTime.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n"
				+ "RRN : \t" + (e.Result.RRN ?? "") + "\r\n"
				+ "Approval Code : \t" + (e.Result.ApprovalCode ?? "") + "\r\n"
				+ "TC : \t" + (e.Result.TC ?? "") + "\r\n"
				+ "CardholderName : \t" + (e.Result.CardholderName ?? "") + "\r\n"
				+ "Card Application Label : \t" + (e.Result.CardAppLabel ?? "") + "\r\n"
				+ "ExpiryDate : \t" + (e.Result.ExpiryDate ?? "") + "\r\n"
				+ "StatusCode : \t" + (e.Result.StatusCode ?? "") + "\r\n"
				+ "Enter Screen Amount : \t" + txtAmount.Text.Trim() + "\r\n"

				+ "\r\n--VOID--\r\n"
				+ "Partner TrxID : \t" + (e.Result.PartnerTrxID ?? "") + "\r\n"
				+ "AlipayTrxID : \t" + (e.Result.AlipayTrxID ?? "") + "\r\n"
				+ "Customer ID : \t" + (e.Result.CustomerID ?? "") + "\r\n"
				+ "Void Amount : \t" + (e.Result.VoidAmount ?? "") + "\r\n"
				+ "Void Currency Amount : \t" + (e.Result.VoidCurrencyAmount.ToString() ?? "") + "\r\n"

				+ "\r\n--Settlement--\r\n"
				+ "Batch Count : \t" + (e.Result.BatchCount ?? "") + "\r\n"
				+ "Batch Amount : \t" + (e.Result.BatchAmount ?? "") + "\r\n"
				+ "Batch Currency Amount : \t" + (e.Result.BatchCurrencyAmount.ToString() ?? "") + "\r\n"

				+ "================================================================================================================================\r\n\r\n"
				;
			}
			else
			{
				rtxtReceiveMsg.Text += "Time : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n"
					+ "..No Result found..\r\n";
			}

			if (rtxtReceiveMsg.Text.Length > 1)
			{
				rtxtReceiveMsg.Select(rtxtReceiveMsg.Text.Length - 1, 1);
			}
			rtxtReceiveMsg.ScrollToCaret();
		}

		private void HideForm()
		{
			//if (_payECRAcc != null) _payECRAcc.Dispose();
			//_payECRAcc = null;

			((Form)this.Owner).Show();
			this.Hide();
		}


        #endregion
    }
}
