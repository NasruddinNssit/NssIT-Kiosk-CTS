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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CashMachineTCPTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string _logChannel = "WFmMainWindow";

		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\CashMachineTCPTest\LogDB\NssITKioskLog01.db;Version=3";

		private NetClientService _netClientSvc = null;
		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		private DispatcherTimer _checkCashMachineStatusTmr = null;


		pgBanknoteTest _pgBanknoteTest = null;

		public MainWindow()
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

				_pgBanknoteTest = new pgBanknoteTest(_netClientSvc);

			}
			catch (Exception ex)
			{
				_log?.LogError(_logChannel, "-", ex, "EX01", "WFmMainWindow.Window_Loaded");
			}
		}

		private bool? _isCashMachReady = null;
		private void _checkCashMachineStatusTmr_Tick(object sender, EventArgs e)
		{
			this.Dispatcher.Invoke(new Action(() => {
				try
				{
					ShowMsg("Read Cash Machine status ..");

					_isCashMachReady = true;
					_checkCashMachineStatusTmr.Stop();
					_checkCashMachineStatusTmr.IsEnabled = false;

					//_isCashMachReady = _netClientSvc.BanknoteSvcClient.CheckCashMachineIsReady("-");
					_isCashMachReady = _netClientSvc.BanknoteSvcClient.CheckCashMachineIsReady("-", out string errMsg);
					
					if (_isCashMachReady == false)
					{
						if (string.IsNullOrWhiteSpace(errMsg) == false)
							ShowMsg(errMsg);
						else
							ShowMsg("Machine is not ready.. ..");
					}
					else
					{
						ShowMsg("Cash Machine is Ready");
						//btnStartPayment.IsEnabled = true;
					}
				}
				catch (Exception ex)
				{
					ShowMsg(ex.ToString());
				}
				finally
				{
					if (_isCashMachReady.HasValue && _isCashMachReady.Value == false)
					{
						_checkCashMachineStatusTmr.IsEnabled = true;
						_checkCashMachineStatusTmr.Start();
					}
				}
			}));

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

		
		private void BtnBankNotePage_Click(object sender, RoutedEventArgs e)
		{
			frmWorkSpace.NavigationService.Navigate(_pgBanknoteTest);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_netClientSvc.Dispose();
		}
	}
}
