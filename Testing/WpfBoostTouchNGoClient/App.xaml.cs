using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfBoostTouchNGoClient.NetClient;

namespace WpfBoostTouchNGoClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static string _logChannel = "AppSys";
		private static NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;
		private static LibShowMessageWindow.MessageWindow _messageWindow = null;
		private static AppBTnGSvcEventsHandler _appBTnGSvcEventsHandler = null;

		public static DbLog Log { get; private set; }
		public static SysLocalParam SysParam { get; private set; }
		public static SynchronizationContext ExecuteContext { get; private set; }
		public static NetClientService NetClientSvc { get; private set; }
		public static MainWindow AppWindow { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			this.Exit += App_Exit;
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;

			ExecuteContext = SynchronizationContext.Current;

			try
			{
				_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

				_sysSetting.ApplicationVersion = "WpfBoostTouchNGoClient_x01";
				if (_sysSetting.IsDebugMode)
				{
					_sysSetting.IPAddress = "10.1.1.111";
					//_sysSetting.IPAddress = "10.238.4.15";
				}
				else
					_sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				String thisprocessname = Process.GetCurrentProcess().ProcessName;

				if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
				{
					MessageBox.Show("Twice access to Kiosk Client Application is prohibited", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					System.Windows.Application.Current.Shutdown();
					return;
				}

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				Log = DbLog.GetDbLog();

				RegistrySetup.GetRegistrySetting().ReadAllRegistryValues(out string registryErrorMsg);

				if (string.IsNullOrWhiteSpace(registryErrorMsg) == false)
				{
					Log.LogText(_logChannel, "OnStartup", $@"Registry Error -- {registryErrorMsg}", "X21", "App.OnStart");

					throw new Exception(registryErrorMsg);
				}

				Log.LogText(_logChannel, "-", $@"Start - WpfBoostTouchNGoClient - App XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
						"A01", "App.OnStartup");

				SysParam = new SysLocalParam();
				SysParam.ReadParameters();

				// Note : AcroRd32 will block system from Opening IP Port correctly.
				//PDFTools.KillAdobe("AcroRd32");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//System Parameter Validation
				//-----------------------------

				bool passSysParameterCheck = true;

				//Server & Client Port Checking
				if ((SysParam.PrmLocalServerPort < 0) || (SysParam.PrmLocalServerPort > 65535))
				{
					passSysParameterCheck = false;
					MessageBox.Show("Invalid LocalServerPort parameter", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				else if ((SysParam.PrmClientPort < 0) || (SysParam.PrmClientPort > 65535))
				{
					passSysParameterCheck = false;
					MessageBox.Show("Invalid ClientPort parameter", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				//// Payment Method Checking
				//else if ((SysParam.IsPayMethodValid == false))
				//{
				//	passSysParameterCheck = false;
				//	MessageBox.Show("Invalid PayMethod parameter. Only C is allowed at the moment.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//}
				// Adobe Reader File Path Checking
				//else if ((SysParam.PrmAcroRd32FilePath is null))
				//{
				//	passSysParameterCheck = false;
				//	MessageBox.Show("Adobe Reader File (AcroRd32.exe) Path is missing. Please make sure Adobe Reader has installed. And set the right default printer.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//}

				else if ((SysParam.PrmPayWaveCOM is null))
				{
					passSysParameterCheck = false;
					MessageBox.Show("COM Port for credit card machine is missing. Please make sure COM Port has installed properly. And assign the COM Port into parameter file.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				else if ((SysParam.PrmCardSettlementTime is null))
				{
					passSysParameterCheck = false;
					MessageBox.Show("CardSettlementTime parameter is missing. Please enter a time in HH:mm (24Hours format).", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				else if ((SysParam.PrmCheckPrinterPaperLow.HasValue == false))
				{
					passSysParameterCheck = false;
					MessageBox.Show("CheckPrinterPaperLow parameter is missing. Please set a 'Yes' or 'No' to the parameter.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				if (passSysParameterCheck == false)
				{
					System.Windows.Application.Current.Shutdown();
					return;
				}

				if (SysParam.PrmIsDebugMode == true)
				{
					_messageWindow = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
					System.Windows.Forms.Application.DoEvents();
				}

				_sysSetting.ApplicationVersion = "Testing - WpfBoostTouchNGoClient";

				Log.LogText(_logChannel, "SystemSetting", _sysSetting, "SETTING", "App.OnStartup");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				_sysSetting.HashSecretKey = RegistrySetup.GetRegistrySetting().HashSecretKey;
				_sysSetting.TVMKey = RegistrySetup.GetRegistrySetting().TVMKey;
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				NetClientSvc = new NetClientService();

				_appBTnGSvcEventsHandler = new AppBTnGSvcEventsHandler(NetClientSvc);

				Log.LogText(_logChannel, "SystemParam", SysParam, "SYS", "App.OnStartup");

				System.Windows.Forms.Application.DoEvents();

				Log.LogText(_logChannel, "SystemParam", SysParam, "PARAMETER", "App.OnStartup");

				AppWindow = new MainWindow();
				AppWindow.Show();
			}
			catch (Exception ex)
			{
				if (Log != null)
				{
					Log.LogError(_logChannel, "-", ex, "EX01", "App.OnStartup");
				}

				MessageBox.Show($@"Error. Application is quit; {ex.Message}", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				System.Windows.Application.Current.Shutdown();
			}
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			string ss = e.Exception.Message;
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			int ss = e.ApplicationExitCode;

			try
			{
				NetClientSvc?.Dispose();

				if (NetClientSvc != null)
					Task.Delay(3000).Wait();
			}
			catch { }
			finally
			{
				NetClientSvc = null;
			}
		}

		protected override void OnExit(System.Windows.ExitEventArgs e)
		{
			try
			{
				NetClientSvc?.Dispose();

				if (NetClientSvc != null)
					Task.Delay(3000).Wait();
			}
			catch { }
			finally
			{
				NetClientSvc = null;
			}
		}
	}
}
