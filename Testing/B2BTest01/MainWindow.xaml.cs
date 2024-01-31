using NssIT.Kiosk.Device.B2B.AccessSDK;
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
using NssIT.Kiosk;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;

namespace B2BTest01
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\B2BTest01\bin\Debug\SqliteDb\NssITKioskLog01.db;Version=3";
		private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		private B2BTest _b2bTest = null;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		public MainWindow()
		{
			_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
			_sysSetting.LogDbConnectionStr = _logDBConnStr;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_b2bTest = new B2BTest();
			_b2bTest.CurrentProcessId = "PT" + DateTime.Now.ToString("HHmmss");
		}

		private void BtnInitB2B_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//this.Dispatcher.Invoke(new Action(()=> {
				_b2bTest.InitDevice();
				//}));
				ShowMsg("End B2B Init..");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtnGetB2BCassetteStatus_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//_b2bTest.StartReceive(20.00M);
				B2BCassetteInfoCollection casesColl = _b2bTest.GetCassetteQtyStatus();

				for (int caseInx = 0; caseInx < casesColl.GetNumberOfCassette(); caseInx++)
				{
					ShowMsg($@"Cassette No.: {casesColl[caseInx].CassetteNo} ; Bill Type : {casesColl[caseInx].BillType} ; Bill Qty. : {casesColl[caseInx].BillQty} ; IsCassettePresence : {casesColl[caseInx].IsCassettePresence} ; Is Cassette Full : {casesColl[caseInx].IsCassetteFull} ; Is Escrow Enable : {casesColl[caseInx].IsEscrowEnable} ; ");
				}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtnRecevingB2B_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_b2bTest.StartReceive(20.00M);
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtnEndRecevingB2B_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_b2bTest.EndReceivingCash();
				ShowMsg("End B2B Init..");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtnCancelRecevingB2B_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_b2bTest.CancelReceive();
				ShowMsg("Cancel B2B Cancel..");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtbCloseB2B_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_b2bTest.CloseDevice();
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void BtnReadLog_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				GrdData.ItemsSource = _log.GetLatestLog(350)?.DefaultView;
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		private void ShowMsg(string msg)
		{
			this.Dispatcher.Invoke(new Action(() => {
				TxtMsg.Text = $@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {(msg ?? "")}{"\r\n"}{TxtMsg.Text}";
			}));
			
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				_b2bTest.CloseDevice();
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}

		
	}
}
