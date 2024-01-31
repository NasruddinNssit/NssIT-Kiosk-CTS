using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;

namespace KioskClientTcpTest
{
	/// <summary>
	/// Interaction logic for wfmCashPayment.xaml
	/// </summary>
	public partial class wfmCashPayment : Window
	{
		private string _logChannel = "CashPaymentUI";
		private string _currProcessId = "-";
		private string _docNo = "-";
		private string _lblPleasePayLableContentStr = "";
		private decimal _amount = 0.00M;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		private INetMediaInterface _netInterface;

		private NetClientCashMachineService _cashMachineService = null;

		private Brush _BtnCancelSalesEnabledBackground = null;
		private Brush _BtnCancelSalesDisabledBackground = new SolidColorBrush(Color.FromRgb(128, 128, 128));

		public wfmCashPayment(INetMediaInterface netInterface, NetClientCashMachineService cashMachineService, 
			string processId, decimal amount, string docNo)
		{
			InitializeComponent();

			_BtnCancelSalesEnabledBackground = btnCancelSales.Background;

			_netInterface = netInterface;
			_cashMachineService = cashMachineService;
			_amount = (amount < 0) ? 0.00M : amount;
			_currProcessId = processId ?? "-";
			_docNo = docNo ?? "-";
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Screen.AllScreens.Length > 1)
			{
				Screen s1 = Screen.AllScreens[0];
				System.Drawing.Rectangle r1 = s1.WorkingArea;

				Screen s2 = Screen.AllScreens[1];
				System.Drawing.Rectangle r2 = s2.WorkingArea;

				this.Top = (r2.Height - this.Height) / 2;
				this.Left = (r1.Width + ((r2.Width - this.Width) / 2) )  ;
			}

			else
			{
				Screen s1 = Screen.AllScreens[0];
				System.Drawing.Rectangle r1 = s1.WorkingArea;
				this.Top = r1.Top;
				this.Left = r1.Left;
			}

			InitApp();

			_lblPleasePayLableContentStr = (string)lblPleasePayLable.Content;
		}

		private void InitApp()
		{
			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
			if (_netInterface != null)
			{
				lblPrice.Content = $@"Price RM {_amount:#,##0.00}";
				lblPaidAmount.Content = $@"RM 0.00";
				lblPleasePay.Content = $@"RM {_amount:#,##0.00}";
				lblCountDown.Content = $@"70";

				imgRm1.Visibility = Visibility.Collapsed;
				imgRm5.Visibility = Visibility.Collapsed;
				imgRm10.Visibility = Visibility.Collapsed;
				imgRm20.Visibility = Visibility.Collapsed;
				imgRm50.Visibility = Visibility.Collapsed;

				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
				_cashMachineService.StartPayment(_docNo, _amount, _docNo);
				//StartPayment(_docNo, _amount, _docNo);
			}
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e?.ReceivedData?.Module == AppModule.UIPayment)
			{
				switch (e?.ReceivedData.Instruction)
				{
					case CommInstruction.UIPaymShowCountdownMessage:
						ShowCountdownMessage(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowCustomerMessage:
						ShowCustomerMessage(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowProcessingMessage:
						ShowProcessingMessage(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowOutstandingPayment:
						ShowOutstandingPayment(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowRefundPayment:
						ShowShowRefundStatus(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowBanknote:
						ShowBanknote(e.ReceivedData);
						break;
					case CommInstruction.ShowErrorMessage:
						ShowErrorMessage(e.ReceivedData);
						break;
					case CommInstruction.UIPaymShowForm:
						InitCashPaymentPage(e.ReceivedData);
						break;
					case CommInstruction.UIPaymHideForm:
						HideForm(e.ReceivedData);
						break;
					case CommInstruction.UIPaymSetCancelPermission:
						SetCancelPermission(e.ReceivedData);
						break;
					default:
						break;
				}
			}
		}

		private void ShowCountdownMessage(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UICountdown cntDown)
				{
					lblCountDown.Content = $@"{cntDown.CountdownMsg}";
				}
			}), null);
		}

		private void ShowCustomerMessage(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UICustomerMessage custMsg)
				{
					if (custMsg.CustmerMsg != null)
						txtCustomerMsg.Text = custMsg.CustmerMsg;

					if ((custMsg.DisplayCustmerMsg != UIVisibility.VisibleNotChanged))
						txtCustomerMsg.Visibility = (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;
				}
			}), null);
		}

		private void ShowProcessingMessage(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIProcessingMessage procMsg)
				{
					if (procMsg.ProcessMsg != null)
						txtProcessingMsg.Text = procMsg.ProcessMsg;
				}
			}), null);
		}

		private void ShowOutstandingPayment(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIOutstandingPayment outPay)
				{
					if (outPay.CustmerMsg != null)
						txtCustomerMsg.Text = outPay.CustmerMsg;

					if (outPay.ProcessMsg != null)
						txtProcessingMsg.Text = outPay.ProcessMsg;

					lblPaidAmount.Content = $@"RM {outPay.PaidAmount:#,##0.00}";

					if (outPay.IsPaymentDone == false)
						lblPleasePay.Content = $@"RM {outPay.OutstandingAmount:#,##0.00}";

					else if (outPay.IsRefundRequest)
						lblPleasePay.Content = "";

					else
					lblPleasePay.Content = "";

					if (outPay.IsPaymentDone)
					{
						btnCancelSales.IsEnabled = false;
						btnCancelSales.Background = _BtnCancelSalesDisabledBackground;
					}
				}
			}), null);
		}

		private void ShowShowRefundStatus(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIRefundPayment refundStt)
				{
					if (refundStt.CustmerMsg != null)
						txtCustomerMsg.Text = refundStt.CustmerMsg;

					if (refundStt.ProcessMsg != null)
						txtProcessingMsg.Text = refundStt.ProcessMsg;

					lblPaidAmount.Content = $@"RM {refundStt.PaidAmount:#,##0.00}";

					lblPleasePayLable.Content = "Refund Amount :";
					lblPleasePay.Content = $@"RM {refundStt.RefundAmount:#,##0.00}";
				}
			}), null);
		}

		private void ShowBanknote(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIAcceptableBanknote bankNote)
				{
					txtAcceptedBanknote.Text = "";
					imgRm1.Visibility = Visibility.Collapsed;
					imgRm5.Visibility = Visibility.Collapsed;
					imgRm10.Visibility = Visibility.Collapsed;
					imgRm20.Visibility = Visibility.Collapsed;
					imgRm50.Visibility = Visibility.Collapsed;

					foreach (NssIT.Kiosk.AppDecorator.Devices.Payment.Banknote billNote in bankNote.NoteArr)
					{
						//txtAcceptedBanknote.Text += $@"{billNote.Currency} {billNote.Value}{"\r\n"}";
						switch (billNote.Value)
						{
							case 1:
								imgRm1.Visibility = Visibility.Visible;
								break;
							case 5:
								imgRm5.Visibility = Visibility.Visible;
								break;
							case 10:
								imgRm10.Visibility = Visibility.Visible;
								break;
							case 20:
								imgRm20.Visibility = Visibility.Visible;
								break;
							case 50:
								imgRm50.Visibility = Visibility.Visible;
								break;
						}
					}
				}
			}), null);
		}

		private void ShowErrorMessage(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIError err)
				{
					if (err.ErrorMessage != null)
						txtErrorMsg.Text = err.ErrorMessage;

					if (err.DisplayErrorMsg != UIVisibility.VisibleNotChanged)
						txtErrorMsg.Visibility = (err.DisplayErrorMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;
				}
			}), null);
		}

		private void InitCashPaymentPage(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				txtCustomerMsg.Visibility = Visibility.Visible;
				txtErrorMsg.Visibility = Visibility.Collapsed;
				btnCancelSales.IsEnabled = true;
				btnCancelSales.Background = _BtnCancelSalesEnabledBackground;
				btnCancelSales.Content = "Cancel";

				if (netMsg.MsgObject is UINewPayment newPay)
				{
					if (newPay.CustmerMsg != null)
						txtCustomerMsg.Text = newPay.CustmerMsg;

					if (newPay.ProcessMsg != null)
						txtProcessingMsg.Text = newPay.ProcessMsg;

					lblPrice.Content = $@"Price RM {newPay.Price:#,##0.00}";
					lblPaidAmount.Content = $@"RM 0.00";
					lblPleasePay.Content = $@"RM {newPay.Price:#,##0.00}";
					lblCountDown.Content = $@"{newPay.InitCountDown}";
					_currProcessId = newPay.ProcessId;
				}

				//this.Show();
				//this.ShowDialog();
			}), null);
		}

		private void HideForm(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UIHideForm hideFm)
				{
					this.Close();
				}
			}), null);
		}

		private void SetCancelPermission(NetMessagePack netMsg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				if (netMsg.MsgObject is UISetCancelPermission cancPerm)
				{
					if (cancPerm.CancelTag != null)
						btnCancelSales.Content = cancPerm.CancelTag;

					if (cancPerm.IsCancelEnabled != UIAvailability.NotChanged)
					{
						btnCancelSales.IsEnabled = (cancPerm.IsCancelEnabled == UIAvailability.Enabled) ? true : false;

						if (btnCancelSales.IsEnabled == true)
							btnCancelSales.Background = _BtnCancelSalesEnabledBackground;
						else
							btnCancelSales.Background = _BtnCancelSalesDisabledBackground;
					}
				}
			}), null);
		}

		//private bool StartPayment(string processId, decimal amount, string docNo)
		//{
		//	bool isSuccess = false;
		//	try
		//	{
		//		UIMakeNewPayment newPay = new UIMakeNewPayment(processId, DateTime.Now)
		//		{
		//			Price = amount,
		//			DocNo = docNo 
		//		};
		//		NetMessagePack msgPack = new NetMessagePack(newPay) { DestinationPort = GetServerPort() };

		//		_netInterface.SendMsgPack(msgPack);
		//		_currProcessId = processId;
		//		isSuccess = true;
		//	}
		//	catch (Exception ex)
		//	{
		//		_log.LogError(_logChannel, processId, ex, "EX01-StartPayment", "wfmCashPayment.BtnCancelSales_Click");
		//		throw ex;
		//	}

		//	return isSuccess;
		//}

		private int GetServerPort() => 7385;
		
		private void BtnCancelSales_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				UICancelPayment cancPay = new UICancelPayment(_currProcessId, DateTime.Now) ;
				NetMessagePack msgPack = new NetMessagePack(cancPay) { DestinationPort = GetServerPort() };

				_netInterface.SendMsgPack(msgPack);

				btnCancelSales.IsEnabled = false;
				btnCancelSales.Background = _BtnCancelSalesDisabledBackground;
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, _currProcessId, ex, "EX01", "wfmCashPayment.BtnCancelSales_Click");
			}
		}

		private void Shutdown()
		{
			_log = null;
			_cashMachineService = null;
			if (_netInterface != null)
			{
				_netInterface.OnDataReceived -= _netInterface_OnDataReceived;
				_netInterface = null;
			}
		}

		private void Window_Unloaded(object sender, RoutedEventArgs e) => Shutdown();

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Shutdown();

	}
}
