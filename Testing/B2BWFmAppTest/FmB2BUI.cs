using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Device.B2B.B2BApp;

namespace B2BWFmAppTest
{
	public partial class FmB2BUI : Form
	{
		private string _logChannel = "B2BPaymentUI";

		public B2BPaymentApplication _b2bapp;

		private string _processId = null;
		private decimal _amount = 0.00M;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		public FmB2BUI()
		{
			InitializeComponent();

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			_b2bapp = new B2BPaymentApplication();
			//_b2bapp.OnSetCancellingPermission += _b2bapp_OnSetCancellingPermission;
			//_b2bapp.OnShowCountDownMessage += _b2bapp_OnShowCountDownMessage;
			//_b2bapp.OnShowCustomerMessage += _b2bapp_OnShowCustomerMessage;
			//_b2bapp.OnShowErrorMessage += _b2bapp_OnShowErrorMessage;
			//_b2bapp.OnShowOutstandingPaymentMessage += _b2bapp_OnShowOutstandingPaymentMessage;
			_b2bapp.OnShowProcessingMessage += _b2bapp_OnShowProcessingMessage;
			//_b2bapp.OnShowPaymantPage += _b2bapp_OnShowPaymantPage;
			//_b2bapp.OnHidePaymantPage += _b2bapp_OnHidePaymantPage;
		}

		private void FmB2BUI_Load(object sender, EventArgs e)
		{

		}

		private void FmB2BUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			_b2bapp.ShutDown();
		}

		private void btnCancelSales_Click(object sender, EventArgs e)
		{
			_b2bapp.CancelTransaction();
		}

		//private void _b2bapp_OnHidePaymantPage(object sender, EventArgs e)
		//{
		//	this.Invoke(new Action(() => {
		//		this.Hide();
		//	}));
		//}

		//private void _b2bapp_OnShowPaymantPage(object sender, EventArgs e)
		//{
		//	this.Invoke(new Action(() => {
		//		this.Show();
		//	}));
		//}

		private void _b2bapp_OnShowProcessingMessage(object sender, UIMessageEventArgs e)
		{
			try
			{
				this.Invoke(new Action(() => {

					if (e == null) return;

					if (e.KioskMsg is UIProcessingMessage procMsg)
					{
						txtProcessingMsg.Text = procMsg.ProcessMsg;
						txtProcessingMsg.Refresh();
					}
					else if (e.KioskMsg is UIAcceptableBanknote aBankNoteX)
					{
						NssIT.Kiosk.AppDecorator.Devices.Payment.Banknote[] noteArr = aBankNoteX.NoteArr;

						txtAcceptedBanknote.Text = "";
						foreach (NssIT.Kiosk.AppDecorator.Devices.Payment.Banknote billNote in noteArr)
						{
							txtAcceptedBanknote.Text += $@"{billNote.Currency} {billNote.Value}{"\r\n"}";
						}

						txtAcceptedBanknote.Refresh();
					}
					else if (e.KioskMsg is UIOutstandingPayment outPay)
					{
						lblPleasePay.Text = $@"RM {outPay.OutstandingAmount:#,##0.00}";
						lblPleasePay.Refresh();
					}
					else if (e.KioskMsg is UISetCancelPermission cancelPerm)
					{
						if (cancelPerm.IsCancelEnabled != UIAvailability.NotChanged)
							btnCancelSales.Enabled = (cancelPerm.IsCancelEnabled == UIAvailability.Enabled) ? true : false;

						btnCancelSales.Refresh();
					}
					else if (e.KioskMsg is UICountdown countD)
					{
						if (countD.CountdownMsg != null)
							lblCountDown.Text = countD.CountdownMsg;

						lblCountDown.Refresh();
					}
					else if (e.KioskMsg is UICustomerMessage custMsg)
					{
						if (custMsg.DisplayCustmerMsg != UIVisibility.VisibleNotChanged)
						{
							txtCustomerMsg.Visible = (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled) ? true : false;

							SetVisibility(txtCustomerMsg, txtCustomerMsg.Visible);

							if (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled)
								txtErrorMsg.Visible = false;

							SetVisibility(txtErrorMsg, txtErrorMsg.Visible);
						}

						txtErrorMsg.Refresh();

						txtCustomerMsg.Text = custMsg.CustmerMsg ?? txtCustomerMsg.Text;
						txtCustomerMsg.Refresh();
					}
					else if (e.KioskMsg is UIError err)
					{
						if (err.DisplayErrorMsg != UIVisibility.VisibleNotChanged)
							txtErrorMsg.Visible = (err.DisplayErrorMsg == UIVisibility.VisibleEnabled) ? true : false;

						SetVisibility(txtErrorMsg, txtErrorMsg.Visible);

						txtErrorMsg.Text = err.ErrorMessage ?? txtErrorMsg.Text;
						txtErrorMsg.Refresh();
					}
					else if (e.KioskMsg is UINewPayment newPay)
					{
						lblPrice.Text = $@"RM {newPay.Price:#,##0.00}";
						lblPleasePay.Text = $@"RM {newPay.Price:#,##0.00}";
						txtCustomerMsg.Text = $@"Please Pay RM {newPay.Price:#,##0.00}";

						this.Show();

						lblPrice.Refresh();
						lblPleasePay.Refresh();
						txtCustomerMsg.Refresh();
					}
					else if (e.KioskMsg is UIHideForm hideForm)
					{
						this.Hide();
					}

				}));
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, _processId, ex, "E01", "B2BPaymentUI._b2bapp_OnShowProcessingMessage");
			}
		}

		//private void _b2bapp_OnShowOutstandingPaymentMessage(object sender, UIMessageEventArgs e)
		//{
		//	try
		//	{
		//		this.Invoke(new Action(() => {

		//			lblPleasePay.Text = $@"RM {e.Message:#,##0.00}";
		//			lblPleasePay.Refresh();
		//		}));
		//	}
		//	catch (Exception ex)
		//	{
		//		_log.LogError(_logChannel, _processId, ex, "E01", "B2BPaymentUI._b2bapp_OnShowProcessingMessage");
		//	}
		//}

		//private void _b2bapp_OnShowErrorMessage(object sender, UIMessageEventArgs e)
		//{
		//	try
		//	{
		//		this.Invoke(new Action(() => {
		//			if (e.Enabled.HasValue)
		//				txtErrorMsg.Enabled = e.Enabled.Value;

		//			txtErrorMsg.Text = e.Message;
		//			txtErrorMsg.Refresh();

		//			if (e.Visibled.HasValue)
		//				SetVisibility(txtErrorMsg, e.Visibled.Value);

		//			txtErrorMsg.Refresh();
		//		}));
		//	}
		//	catch
		//	{ }
		//}

		//private void _b2bapp_OnShowCustomerMessage(object sender, UIMessageEventArgs e)
		//{
		//	try
		//	{
		//		this.Invoke(new Action(() => {

		//			if (e.Enabled.HasValue)
		//				txtCustomerMsg.Enabled = e.Enabled.Value;

		//			txtCustomerMsg.Text = e.Message ?? txtCustomerMsg.Text;
		//			txtCustomerMsg.Refresh();

		//			if (e.Visibled.HasValue)
		//			{
		//				if (e.Visibled.Value == true)
		//					SetVisibility(txtErrorMsg, false);

		//				SetVisibility(txtCustomerMsg, e.Visibled.Value);

		//				txtErrorMsg.Refresh();
		//				txtCustomerMsg.Refresh();
		//			}
		//		}));
		//	}
		//	catch
		//	{ }
		//}

		//private void _b2bapp_OnShowCountDownMessage(object sender, UIMessageEventArgs e)
		//{
		//	try
		//	{
		//		this.Invoke(new Action(() => {
		//			lblCountDown.Text = e.Message;
		//			lblCountDown.Refresh();
		//		}));
		//	}
		//	catch (Exception ex)
		//	{ }
		//}

		//private void _b2bapp_OnSetCancellingPermission(object sender, UIMessageEventArgs e)
		//{
		//	try
		//	{
		//		this.Invoke(new Action(() => {
		//			if (e.Enabled.HasValue)
		//				btnCancelSales.Enabled = e.Enabled.Value;

		//			btnCancelSales.Refresh();
		//		}));
		//	}
		//	catch (Exception ex)
		//	{ }
		//}

		System.Drawing.Size _hideSize = new Size(1, 1);
		private Dictionary<string, System.Drawing.Size> _visibilityList = new Dictionary<string, Size>();
		private void SetVisibility(System.Windows.Forms.Control ctrl, bool isVisible)
		{
			if (isVisible == false)
			{
				if ((ctrl.Size.Width != _hideSize.Width) || (ctrl.Size.Height != _hideSize.Height))
				{
					System.Drawing.Size orgSize;

					if (_visibilityList.ContainsKey(ctrl.Name) == false)
					{
						orgSize = ctrl.Size;
						ctrl.Size = _hideSize;
						_visibilityList.Add(ctrl.Name, orgSize);
					}
				}
			}
			else
			{
				if ((ctrl.Size.Width == _hideSize.Width) || (ctrl.Size.Height == _hideSize.Height))
				{
					if (_visibilityList.ContainsKey(ctrl.Name))
					{
						ctrl.Size = _visibilityList[ctrl.Name];
					}
				}
			}
		}

		public async Task<bool> StartPayment(string processId, decimal amount)
		{
			txtAcceptedBanknote.Text = "";

			bool isSuccess = await _b2bapp.MakePayment(processId, Guid.NewGuid(), amount);

			if (isSuccess)
			{
				_processId = processId;
				_amount = amount;
			}
			else
				_processId = null;

			lblPrice.Text = $@"RM {amount:#,##0.00}";
			lblPleasePay.Text = $@"RM {amount:#,##0.00}";
			txtCustomerMsg.Text = $@"Please Pay RM {amount:#,##0.00}";

			lblPrice.Refresh();
			lblPleasePay.Refresh();
			txtCustomerMsg.Refresh();

			return isSuccess;
		}

		public bool IsMachineReady()
		{
			return _b2bapp.ReadIsPaymentDeviceReady(out _, out _, out _);
		}

		public void ShowNewPaymentTransaction()
		{
			if (_processId == null)
				throw new Exception("Invalid Process ID.");

			lblPrice.Text = $@"RM {_amount:#,##0.00}";
			lblPrice.Refresh();

			this.Show();
		}

	}
}
