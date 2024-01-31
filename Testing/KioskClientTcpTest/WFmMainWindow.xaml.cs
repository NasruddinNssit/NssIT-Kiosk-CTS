using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.AppService.Network.TCP;
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
using System.Windows.Threading;

namespace KioskClientTcpTest
{
	/// <summary>
	/// Interaction logic for WFmMainWindow.xaml
	/// </summary>
	public partial class WFmMainWindow : Window
	{
		private string _logChannel = "WFmMainWindow";

		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\KioskClientTcpTest\LogDB\NssITKioskLog01.db;Version=3";

		private NetClientService _netClientSvc = null;
		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		private DispatcherTimer _checkCashMachineStatusTmr = null;

		public WFmMainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
				_sysSetting.LogDbConnectionStr = _logDBConnStr;
				_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

				_netClientSvc = new NetClientService();

				_checkCashMachineStatusTmr = new DispatcherTimer();
				_checkCashMachineStatusTmr.Tick += _checkCashMachineStatusTmr_Tick;
				_checkCashMachineStatusTmr.Interval = new TimeSpan(0, 0, 3);
				_checkCashMachineStatusTmr.IsEnabled = true;
				_checkCashMachineStatusTmr.Start();

			}
			catch (Exception ex)
			{
				_log?.LogError(_logChannel, "-", ex, "EX01", "WFmMainWindow.Window_Loaded");		
			}
		}

		private bool? _isCashMachReady = null; 
		private void _checkCashMachineStatusTmr_Tick(object sender, EventArgs e)
		{
			bool isLowCoin = false;

			this.Dispatcher.Invoke(new Action(() => {
				try
				{
					ShowMsg("Read Cash Machine status ..");

					_isCashMachReady = true;
					_checkCashMachineStatusTmr.Stop();
					_checkCashMachineStatusTmr.IsEnabled = false;

					_isCashMachReady = _netClientSvc.CashMachineService.CheckCashMachineIsReady("-", out isLowCoin, out string errMsg);

					if (isLowCoin)
					{
						ShowMsg(string.IsNullOrWhiteSpace(errMsg) ? "Machine encountered Low Coin." : errMsg);
					}
					else if (_isCashMachReady == false)
					{
						if (string.IsNullOrWhiteSpace(errMsg) == false)
							ShowMsg(errMsg);
						else
							ShowMsg("Machine is not ready.. ..");
					}
					else
					{
						ShowMsg("Cash Machine is Ready");
						btnStartPayment.IsEnabled = true;
					}
				}
				catch (Exception ex)
				{
					ShowMsg(ex.ToString());
				}
				finally
				{
					if ((_isCashMachReady.HasValue) && (_isCashMachReady.Value == false) && (isLowCoin == false))
					{
						_checkCashMachineStatusTmr.IsEnabled = true;
						_checkCashMachineStatusTmr.Start();
					}
				}
			}));
			
		}

		private void BtnStartPayment_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//if (_payUi.IsMachineReady() == false)
				//{
				//	throw new Exception("Cash Machine is not ready..");
				//}

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

				/////xxxxxxxxxxxxxxxxxxx
				bool _isCashMachReady = _netClientSvc.CashMachineService.CheckCashMachineIsReady(txtDocNo.Text, out bool isLowCoin, out string errMsg);

				if (isLowCoin)
				{
					ShowMsg(string.IsNullOrWhiteSpace(errMsg) ? "Machine encountered Low Coin." : errMsg);
				}
				else if (_isCashMachReady == false)
				{
					throw new Exception(errMsg);
				}
				else
				{
					ShowMsg("Cash Machine is Ready");
					//btnStartPayment.IsEnabled = true;
				}
				/////xxxxxxxxxxxxxxxxxxx

				_netClientSvc.StartCashPayment(txtDocNo.Text.Trim(), amount, txtDocNo.Text.Trim());

			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void ShowMsg(string msg = null)
		{
			DateTime tm = DateTime.Now;
			msg = (msg ?? "").Trim();

			this.Dispatcher.Invoke(new Action(() => {
				if (msg.Length > 0)
					txtMsg.AppendText($@"{tm.ToString("HH:mm:ss.fff")}-{msg}{"\r\n"}");

				else if (txtMsg.Text.Length > 1)
					txtMsg.AppendText("\r\n");

				txtMsg.ScrollToEnd();
				System.Windows.Forms.Application.DoEvents();

			}), DispatcherPriority.Background);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_netClientSvc.Dispose();
		}

		private void BtnCashMachineStatus_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
