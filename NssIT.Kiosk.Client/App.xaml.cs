using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Base.Time;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.Client.ViewPage.Menu;
using NssIT.Kiosk.Device.PAX.IM20.PayECRApp;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private const int MaxAdvanceTicketDays = 90;

		private const string _logChannel = "AppSys";

		private static LibShowMessageWindow.MessageWindow _messageWindow = null;
		private static IMainScreenControl _mainScreenControl = null;

		public const int CustomerInfoTimeoutExtensionSec = 60;

		/// <summary>
		/// Version Refer to an application Version, Date, and release count of the day.
		/// Like "V1.R20200805.1" mean application Version is V1, the release Year is 2020, 5th (05) of August (08), and 1st (.1) release count of the day.
		/// Note : With "XX#XX" for undeployable version. This version is not for any release purpose. Only for development process.
		/// </summary>
		public static string SystemVersion = "V1.R240301.1";

		public static string ServerVersion = NssIT.Kiosk.AppDecorator.Config.Setting.NullVersion;

		public static AppGroup ServerAppGroup = AppGroup.Unknown;

		public static PaymentType[] AvailablePaymentTypeList = null;

		public static bool IsBoardingPassEnabled = false;

		public static AppDecorator.Config.Setting _sysSetting = null;
		public static AppHelper AppHelp { get; private set; } = null;

		public static bool IsLocalServerReady { get; set; } = false;
		public static bool IsClientReady { get; set; } = false;

		public static SysLocalParam SysParam { get; private set; }

		public static DbLog Log { get; private set; }

		public static UserSession LatestUserSession { get; set; }

		public static NetClientService NetClientSvc { get; private set; }

		private static AppSalesSvcEventsHandler _appSalesSvcEventsHandler = null;

		private static AppCollectTicketSvcEventsHandler _appCollectTicketSvcEventsHandler = null;

		public static ReportPDFFileManager ReportPDFFileMan { get; private set; }

		public static SynchronizationContext ExecuteContext { get; private set; }

		public static int AppMaxSeatPerTrip { get; private set; } = 20;

		public static ScreenImageManager PaymentScrImage { get; private set; }

		/// <summary>
		/// Company Logo Cache for Collect Ticket (Collect Boarding Pass)
		/// </summary>
		public static WebImageCacheX CTCompanyLogoCache { get; private set; } = new WebImageCacheX(12);

		public static TravelMode AvailableTravelMode { get; private set; } = TravelMode.DepartOnly;

		public static ResetTimeoutManager TimeoutManager { get; private set; } = null;
		public static CollectTicketCountDown CollectBoardingPassCountDown { get; private set; } = null;

		public static DateTime MaxTicketAdvanceDate { get; private set; } = DateTime.Now.AddDays(MaxAdvanceTicketDays);

		private SysLog _sysLog = null;
		public static List<string> HostNumberForSettlementsTesting = new List<string>();
		public static AppModule CurrentSaleTransactionModule { get; private set; } = AppModule.Unknown;

		private static AppOperationHandler _appOperationHandler = null;
        public static PayWaveSettlementScheduler CardSettlementScheduler { get; private set; } = null;
        public static MarkLogList MarkLog { get; set; }
		public static void ResetMaxTicketAdvanceDate()
		{
			DateTime dateTime = DateTime.Now.AddDays(MaxAdvanceTicketDays);

			MaxTicketAdvanceDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
		}

		public static void ResetCurrentSaleTransactionModule()
        {
			CurrentSaleTransactionModule = AppModule.Unknown;
		}

		public static void SetCurrentSaleTransactionToTicketing()
        {
			CurrentSaleTransactionModule = AppModule.UIKioskSales;
		}

		public static void SetCurrentSaleTransactionToCollectTicket()
		{
			CurrentSaleTransactionModule = AppModule.UICollectTicket;
		}

		public static bool CheckIsPaymentTypeAvailable(PaymentType typeOfPayment)
        {
			if (AvailablePaymentTypeList is null)
				return false;

			else if ((from pM in AvailablePaymentTypeList
				 where pM == typeOfPayment
				 select pM).ToArray().Length > 0)
            {
				return true;
            }

			return false;
		}

		public static string GetFullSystemVersion()
        {
			string AppGroupStr = "";

			if (App.SysParam.PrmAppGroup == App.ServerAppGroup)
			{
				// AG : Application Group
				if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Larkin)
					AppGroupStr = "AG:Larkin";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Gombak)
					AppGroupStr = "AG:Gombak";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Klang)
					AppGroupStr = "AG:Klang";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Unknown)
					AppGroupStr = "AG:#";
				else if (App.SysParam.PrmAppGroup == AppGroup.Genting)
					AppGroupStr = "AG:Genting";
				else
					AppGroupStr = "AG:Melaka";
			}
			else
			{
				// CNT : Client 
				if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Larkin)
					AppGroupStr = "CNT:Larkin";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Gombak)
					AppGroupStr = "CNT:Gombak";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Klang)
					AppGroupStr = "CNT:Klang";
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Unknown)
					AppGroupStr = "CNT:#";
                else if (App.SysParam.PrmAppGroup == AppGroup.Genting)
                    AppGroupStr = "AG:Genting";
                else
					AppGroupStr = "CNT:Melaka";

				// SVR : Server
				if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Larkin)
					AppGroupStr += ";SVR:Larkin";
				else if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Gombak)
					AppGroupStr += ";SVR:Gombak";
				else if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Klang)
					AppGroupStr += ";SVR:Klang";
				else if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Unknown)
					AppGroupStr += ";SVR:#";
                else if (App.SysParam.PrmAppGroup == AppGroup.Genting)
                    AppGroupStr = "AG:Genting";
                else
					AppGroupStr += ";SVR:Melaka";
			}
			
			// Check Payment Types
			string payTypeStr = "";
			if (App.AvailablePaymentTypeList?.Length > 0)
            {
				string pStr = null;
				
				foreach(PaymentType pT in App.AvailablePaymentTypeList)
                {
					pStr = null;
					if (pT == PaymentType.Cash)
					{
						pStr = "Cash";

						if (App.SysParam.PrmNoPaymentNeed)
                        {
							pStr = $@"{pStr}(Test-with-no-cash)";
						}
					}
					else if (pT == PaymentType.CreditCard)
						pStr = "Credit Card";
					else if (pT == PaymentType.PaymentGateway)
						pStr = "eWallet";

					if (pStr != null)
						payTypeStr = payTypeStr + (payTypeStr.Length == 0 ? "": ",") + pStr;
				}

				if (payTypeStr.Length > 0)
					payTypeStr = $@" / Payment({payTypeStr})";
			}
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			string retVer = $@"{AppGroupStr} / {App.SystemVersion ?? "*"} / {App.ServerVersion ?? "*"} {payTypeStr}";

			return retVer;
		}

		/// <summary>
		/// Return true when System Setting has no problem.
		/// </summary>
		/// <param name="errorMessage"></param>
		/// <returns></returns>
		public static bool ValidateSystemSetting(out string errorMessage)
		{
			errorMessage = null;

			if (string.IsNullOrWhiteSpace(App.ServerVersion))
				return true;
			else if (App.ServerVersion.Equals(AppDecorator.Config.Setting.NullVersion))
				return true;

			if (App.ServerAppGroup != App.SysParam.PrmAppGroup)
			{
				string cntVer = "";
				string svrVer = "";

				// Client Version
				if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Larkin)
					cntVer = "Larkin";

				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Gombak)
					cntVer = "Gombak";

				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Klang)
					cntVer = "Klang";

				else if (App.SysParam.PrmAppGroup == AppGroup.Genting)
					cntVer = "Genting";

				else
					cntVer = "Melaka";

				// Server Version
				if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Larkin)
					svrVer = "Larkin";

				else if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Gombak)
					svrVer = "Gombak";

				else if (App.ServerAppGroup == AppGroup.Genting)
					svrVer = "Genting";
				else if (App.ServerAppGroup == AppDecorator.Common.AppGroup.Klang)
					svrVer = "Klang";

				else
					svrVer = "Melaka";

				

				errorMessage = $@"Application Group setting mismatch. (EXIT500035); Client Version : {cntVer}; Server Version : {svrVer}";

				return false;
			}

			return true;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			_sysLog = new SysLog();

			this.Exit += App_Exit;
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;

			ExecuteContext = SynchronizationContext.Current;

			try
			{
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
				MarkLog = MarkLogList.GetLogList().ActivateCardMarkingLog();

				Log.LogText(_logChannel, "-", $@"Start - App XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
						"A01", "App.OnStartup");

				_sysSetting = AppDecorator.Config.Setting.GetSetting();
				SysParam = new SysLocalParam();
				SysParam.ReadParameters();

				// Note : AcroRd32 will block system from Opening IP Port correctly.
				//PDFTools.KillAdobe("AcroRd32");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//System Parameter Validation
				//-----------------------------

				_sysSetting.NoOperationTime = SysParam.IsNoOperationTime;
				_sysSetting.NoCardSettlement = SysParam.PrmNoCardSettlement;
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

                //AcroRd32.exe file checking
                //if (passSysParameterCheck == true)
                //{
                //	try
                //	{
                //		FileInfo fInf = new FileInfo(SysParam.PrmAcroRd32FilePath);
                //		if (fInf.Exists == false)
                //		{
                //			passSysParameterCheck = false;
                //			MessageBox.Show($@"Fail to allocate AcroRd32.exe refer to AcroRd32 parameter {SysParam.PrmAcroRd32FilePath}; Please verify the path of the file.  And set the right default printer.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //		}
                //	}
                //	catch (Exception ex)
                //	{
                //		passSysParameterCheck = false;
                //		MessageBox.Show($@"Error when clarify AcroRd32 parameter {SysParam.PrmAcroRd32FilePath}; {ex.Message}; Please verify the path of the file.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //	}
                //}


                if (passSysParameterCheck == false)
				{
					System.Windows.Application.Current.Shutdown();
					return;
				}

				//PDFTools.AdobeReaderFullFilePath = SysParam.PrmAcroRd32FilePath;
				
				SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

				if (SysParam.PrmIsDebugMode == true)
				{ 
					_messageWindow = new LibShowMessageWindow.MessageWindow();
					System.Windows.Forms.Application.DoEvents();
				}

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				try
				{
					if (KioskServiceSwitching.StartService())
					{
						Log.LogText(_logChannel, "-", $@"Service -Nssit.Kiosk.Server- should be started", "A10B", "App.OnStartup");

						Task.Delay(1000 * 12).Wait();
					}
					else
					{
						Log.LogText(_logChannel, "-", $@"-Nssit.Kiosk.Server- may already started", "A10C", "App.OnStartup");
					}
				}
				catch (Exception ex)
				{
					Log.LogText(_logChannel, "-", $@"Error starting -NssIT.Kiosk.Server service-; {ex.Message}", "EX01B", "App.OnStartup",
						AppDecorator.Log.MessageType.Error);
				}
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


				PaymentScrImage = new ScreenImageManager("PaymentDone");

				AppHelp = new AppHelper();

				NetClientSvc = new NetClientService();

				_appSalesSvcEventsHandler = new AppSalesSvcEventsHandler(NetClientSvc);

				_appCollectTicketSvcEventsHandler = new AppCollectTicketSvcEventsHandler(NetClientSvc);

				Log.LogText(_logChannel, "-", $@"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Client Application Init. XXXXXX ClientPort: {SysParam.PrmClientPort}; IsDebugMode: {SysParam.PrmIsDebugMode}; LocalServerPort: {SysParam.PrmLocalServerPort}; ApplicationGroup: {Enum.GetName(typeof(AppGroup), App.SysParam.PrmAppGroup)}; SystemVersion: {SystemVersion}; XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
						"A10", "App.OnStartup");

				ReportPDFFileMan = new ReportPDFFileManager();

				TimeoutManager = ResetTimeoutManager.GetLocalTimeoutManager();
				CollectBoardingPassCountDown = new CollectTicketCountDown();
				_appOperationHandler = new AppOperationHandler(SysParam.StartOperationTime, SysParam.EndOperationTime);
                //WndTestingMonitor testMon = null;
                CardSettlementScheduler = new PayWaveSettlementScheduler(SysParam.PrmPayWaveCOM, SysParam.PrmCardSettlementTime);
                MainWindow main = new MainWindow();
				_mainScreenControl = (IMainScreenControl)main;
				_mainScreenControl.InitForOperationTimeScheduler(_appOperationHandler);
				_mainScreenControl.InitiateMaintenance(CardSettlementScheduler);
				main.WindowState = WindowState.Maximized;

				if ((SysParam.PrmIsDebugMode == false) || (SysParam.PrmIsDemo == true))
				{
					main.WindowStyle = WindowStyle.None;
					//main.Topmost = true;
				}

				main.Show();
				System.Windows.Forms.Application.DoEvents();

				((IMainScreenControl)main).ShowWelcome();
				App.IsClientReady = true;

				System.Windows.Forms.Application.DoEvents();

				if (SysParam.PrmIsDebugMode)
				{
					//DEBUG-Testing .. testMon = new WndTestingMonitor(main);
					//DEBUG-Testing .. testMon.Show();
					//DEBUG-Testing .. ShowDebugMsg("Start Debug");
				}

				//var args = e.Args;
				//if (args.Contains("/other")) { new OtherWindow().Show(); }
				//else { new MainWindow().Show(); }

				Log.LogText(_logChannel, "SystemParam", SysParam, "PARAMETER", "App.OnStartup");
				//Log.LogText(_logChannel, "SystemSetting", _sysSetting, "SETTING", "App.OnStartup");
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

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			_sysLog.WriteLog(e.Exception.ToString());
			ShieldErrorScreen.ShowMessage($@"System encountered difficulty and will be shutdowned;{"\r\n"}{DateTime.Now.ToString("HH:mm:ss")}{"\r\n"}{e.Exception.Message}");
		}

		private static bool _hasShutDown = false;
		private static SemaphoreSlim _appLock = new SemaphoreSlim(1);
		public static void ShutdownX()
		{
			try
			{
				_appLock.WaitAsync().Wait();

				if (_hasShutDown)
					return;

				_hasShutDown = true;

				NetClientSvc.Dispose();
				Task.Delay(3000).Wait();
			}
			catch { }
			finally
			{
				_appLock.Release();
			}

			try {
				MarkLog.QuitMarkingLog();
			}
			catch { }
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			ShutdownX();
		}

		protected override void OnExit(System.Windows.ExitEventArgs e)
		{
			ShutdownX();
		}

		public static string ExecutionFolderPath
		{
			get
			{
				string executionFilePath = Assembly.GetExecutingAssembly().Location;

				FileInfo fInf = new FileInfo(executionFilePath);
				string executionFolderPath = fInf.DirectoryName;

				return executionFolderPath;
			}
		}

		public static void ShowDebugMsg(string msg)
		{
			if (SysParam.PrmIsDebugMode)
			{
				DebugMsg.ShowMessage(msg);
			}
		}


		private static bool _msgWinCreated = false;
		private static SemaphoreSlim _msgWinLock = new SemaphoreSlim(1);
		private static LibShowMessageWindow.MessageWindow DebugMsg
		{
			get
			{
				//if (_messageWindow is null)
				//{
				//	try
				//	{
				//		_msgWinLock.WaitAsync().Wait();

				//		if ((_messageWindow is null) && (_msgWinCreated == false))
				//		{
				//			Thread tWorker = new Thread(new ThreadStart(new Action(() => {
				//				try
				//				{
				//					_messageWindow = new LibShowMessageWindow.MessageWindow();
				//					System.Windows.Forms.Application.DoEvents();
				//				}
				//				catch (Exception ex)
				//				{
				//					string errMsg = ex.Message;
				//				}
				//			})));

				//			tWorker.IsBackground = true;
				//			tWorker.Start();
				//			//tWorker.Join();
				//			_msgWinCreated = true;
				//			Task.Delay(3000).Wait();
				//		}
				//	}
				//	catch (Exception ex2)
				//	{
				//		string errMsg = ex2.Message;
				//	}
				//	finally
				//	{
				//		_msgWinLock.Release();
				//	}
				//}
				return _messageWindow;
			}
		}

		public static IMainScreenControl MainScreenControl
		{
			get
			{
				return _mainScreenControl;
			}
		}

	}
}
