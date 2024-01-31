using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Device.B2B.B2BApp;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace B2BApplicationTest
{
	/// <summary>
	/// Interaction logic for B2BPaymentUI.xaml
	/// </summary>
	public partial class B2BPaymentUI : Window
	{
		//////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		//private const int GWL_STYLE = -16;
		//private const int WS_SYSMENU = 0x80000;
		//[DllImport("user32.dll", SetLastError = true)]
		//private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		//[DllImport("user32.dll")]
		//private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		//////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

		private string _logChannel = "B2BPaymentUI";

		public B2BPaymentApplication _b2bapp;

		private string _processId = null;
		private decimal _amount = 0.00M;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		public B2BPaymentUI()
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

		//private void _b2bapp_OnHidePaymantPage(object sender, UIMessageEventArgs e)
		//{
		//	this.Dispatcher.Invoke(new Action(() => {

				
		//		//_b2bapp_OnHidePaymantPage
		//		this.Hide();
		//	}));
		//}

		//private void _b2bapp_OnShowPaymantPage(object sender, UIMessageEventArgs e)
		//{
		//	this.Dispatcher.Invoke(new Action(() => {


				

				
		//	}));
		//}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//var hwnd = new WindowInteropHelper(this).Handle;
			//SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
			//////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

		}

		public bool IsMachineReady()
		{
			return _b2bapp.ReadIsPaymentDeviceReady(out _, out _, out _);
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

			lblPrice.Content = $@"RM {amount:#,##0.00}";
			lblPleasePay.Content = $@"RM {amount:#,##0.00}";
			txtCustomerMsg.Text = $@"Please Pay RM {amount:#,##0.00}";
			

			return isSuccess;
		}

		public void ShowNewPaymentTransaction()
		{
			if (_processId == null)
				throw new Exception("Invalid Process ID.");

			lblPrice.Content = $@"RM {_amount:#,##0.00}";
			
			this.Show();
		}

		private void BtnCancelSales_Click(object sender, RoutedEventArgs e)
		{
			_b2bapp.CancelTransaction();
		}

		private void _b2bapp_OnShowProcessingMessage(object sender, UIMessageEventArgs e)
		{
			try
			{
				this.Dispatcher.Invoke(new Action(() => {

					if (e == null) return;

					if (e.KioskMsg is UIProcessingMessage procMsg)
					{
						txtProcessingMsg.Text = procMsg.ProcessMsg;
					}
					else if (e.KioskMsg is UIAcceptableBanknote aBankNote)
					{
						NssIT.Kiosk.AppDecorator.Devices.Payment.Banknote[] noteArr = aBankNote.NoteArr;

						txtAcceptedBanknote.Text = "";
						foreach (NssIT.Kiosk.AppDecorator.Devices.Payment.Banknote billNote in noteArr)
						{
							txtAcceptedBanknote.Text += $@"{billNote.Currency} {billNote.Value}{"\r\n"}";
						}
					} 
					else if (e.KioskMsg is UIOutstandingPayment outPay)
					{
						lblPleasePay.Content = $@"RM {outPay.OutstandingAmount:#,##0.00}";
					}
					else if (e.KioskMsg is UISetCancelPermission cancelPerm)
					{
						if (cancelPerm.IsCancelEnabled != UIAvailability.NotChanged)
							btnCancelSales.IsEnabled = (cancelPerm.IsCancelEnabled == UIAvailability.Enabled) ? true : false;
					}
					else if (e.KioskMsg is UICountdown countD)
					{
						if (countD.CountdownMsg != null)
							lblCountDown.Content = countD.CountdownMsg;
					}
					else if (e.KioskMsg is UICustomerMessage custMsg)
					{
						if (custMsg.DisplayCustmerMsg != UIVisibility.VisibleNotChanged )
						{
							txtCustomerMsg.Visibility = (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;

							if (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled)
								txtErrorMsg.Visibility = Visibility.Collapsed;
						}

						txtCustomerMsg.Text = custMsg.CustmerMsg ?? txtCustomerMsg.Text;
					}
					else if (e.KioskMsg is UIError err)
					{
						if (err.DisplayErrorMsg != UIVisibility.VisibleNotChanged)
							txtErrorMsg.Visibility = (err.DisplayErrorMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;

						txtErrorMsg.Text = err.ErrorMessage ?? txtErrorMsg.Text;
					}
					else if (e.KioskMsg is UINewPayment newPay)
					{
						lblPrice.Content = $@"RM {newPay.Price:#,##0.00}";
						lblPleasePay.Content = $@"RM {newPay.Price:#,##0.00}";
						txtCustomerMsg.Text = $@"Please Pay RM {newPay.Price:#,##0.00}";

						this.Show();
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

		private void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			_b2bapp.ShutDown();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_b2bapp.ShutDown();
		}
	}
}
