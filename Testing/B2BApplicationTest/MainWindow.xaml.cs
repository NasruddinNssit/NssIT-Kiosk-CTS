using NssIT.Kiosk.Device.B2B.B2BApp;
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

namespace B2BApplicationTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		B2BPaymentUI _payUi = null;

		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\KioskClientTcpTest\LogDB\NssITKioskLog01.db;Version=3";

		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
			_sysSetting.LogDbConnectionStr = _logDBConnStr;

			_payUi = new B2BPaymentUI();
		}

		private void BtnStartPayment_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_payUi.IsMachineReady() == false)
				{
					throw new Exception("Cash Machine is not ready..");
				}

				decimal amount = 0.00M;

				ShowMsg(); ShowMsg(); ShowMsg();

				if (decimal.TryParse(txtAmount.Text, out amount) == false)
					throw new Exception("Invalid Amount value.");

				if (amount <= 0.999999999999M)
					throw new Exception("Amount must greated or equal 1.00.");

				if ( string.IsNullOrWhiteSpace( txtDocNo.Text ) )
					throw new Exception("Invalid Document No.");

				amount = (Math.Floor((amount * 100.00M))) / 100M;
				txtAmount.Text = amount.ToString();

				//_payUi.Show();

				Task<bool> resTask = _payUi.StartPayment(txtDocNo.Text.Trim(), amount);
				
				resTask.Wait();

				if (resTask.Result == false)
					throw new Exception("Unable to start new Transaction; System busy;");

				

			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}

		}

		private void BtnCashMachineStatus_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_payUi.IsMachineReady())
					ShowMsg("Cash Machine Is Ready");
				else
					ShowMsg("Cash Machine IS NOT Ready");
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

			}), DispatcherPriority.Background);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_payUi.Close();
		}
	}
}
