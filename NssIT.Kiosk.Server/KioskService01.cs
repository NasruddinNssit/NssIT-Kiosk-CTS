using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Device.B2B.B2BApp;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.Server.Service.Adaptor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.AppService.Network;
using NssIT.Kiosk.Device.B2B.Server.Service.Adaptor;
using NssIT.Kiosk.AppDecorator.Config;
using System.Threading;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NssIT.Kiosk.Server
{
	public partial class KioskService01 : ServiceBase
	{
		

		string logChannel = "KioskService";
		string logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\NssIT.Kiosk.Server\LogDB\NssITKioskLog01_Test.db;Version=3";

        private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;
		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface = null;
		private CashPaymentServerSvcAdaptor _cashPaymentSvc = null;
		private SalesServerSvcAdaptor _salesSvr = null;
		private B2BServerServiceAdaptor _b2bSvr = null;
		private CollectTicketSvrAdaptor _colTickSvr = null;
		private BTnGServerSvcAdaptor _bTnGSvr = null;
		private IUIPayment _cashPaymentApp = null;

		private NetInfoRepository _netInfoRepository = null;

		/// <summary>
		/// Version Refer to an application Version, Date, and release count of the day.
		/// Like "V1.R20200805.1" mean application Version is V1, the release Year is 2020, 5th (05) of August (08), and 1st (.1) release count of the day.
		/// Note : With "XX#XX" for undeployable version. This version is not for any release purpose. Only for development process.
		/// </summary>
		private string SystemVersion = "V1.R240312.1";

		public KioskService01()
		{
			InitializeComponent();
		}

		public SysLocalParam SysParam { get; private set; }

		protected override void OnStart(string[] args)
		{
			try
			{
				_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
              //  System.Diagnostics.Debugger.Launch();

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Resolve Cert Problem 
                /////Trust all certificates
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                ///// trust sender
                System.Net.ServicePointManager.ServerCertificateValidationCallback
                                = ((sender, cert, chain, errors) => cert.Subject.Contains("YourServerName"));

				///// validate cert by calling a function
				System.Net.ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                _log.LogText(logChannel, "KioskService01", "Start - KioskService01 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A01", "KioskService01.OnStart");

				RegistrySetup.GetRegistrySetting().ReadAllRegistryValues(out string registryErrorMsg);

				if (string.IsNullOrWhiteSpace(registryErrorMsg) == false)
				{
					_log.LogText(logChannel, "OnStartup", $@"Registry Error -- {registryErrorMsg}", "X21", "KioskService01.OnStart");
					throw new Exception(registryErrorMsg);
				}

				ValidateRegistrySetting();

				SysParam = new SysLocalParam();
				SysParam.ReadParameters();

				_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

				_sysSetting.ApplicationVersion = SystemVersion;

				_sysSetting.WebServiceURL = SysParam.PrmWebServiceURL;
				_sysSetting.IsDebugMode = SysParam.PrmIsDebugMode;
				_sysSetting.LocalServicePort = SysParam.PrmLocalServerPort;
				_sysSetting.PayMethod = SysParam.PrmPayMethod;
				_sysSetting.ApplicationGroup = SysParam.PrmAppGroup;
				_sysSetting.BTnGMinimumWaitingPeriod = SysParam.PrmBTnGMinimumWaitingPeriod;
				_sysSetting.AvailablePaymentTypeList = SysParam.PrmAvailablePaymentTypeList;
				_sysSetting.IsBoardingPassEnabled = SysParam.PrmIsBoardingPassEnabled;
				
				if (_sysSetting.IsDebugMode)
				{
					//_sysSetting.IPAddress = "10.1.1.111";
					//CYA-DEBUG .. (Refer to Production) _sysSetting.IPAddress = "10.238.4.15";
					//CYA-DEBUG .. (Refer to Internet Testing Site)	
					//_sysSetting.IPAddress = "192.168.1.13";
					_sysSetting.IPAddress = "192.168.1.12";
				}
				else
					_sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();


				NssIT.Kiosk.AppDecorator.Common.AppStationCode.GetStationInfo(SysParam.PrmAppGroup, out string stationCode, out _);

				_sysSetting.KioskId = $@"{stationCode}-{_sysSetting.IPAddress}";
				_sysSetting.StationCode = stationCode;
				_sysSetting.WebApiURL = RegistrySetup.GetRegistrySetting().WebApiUrl;

				_log.LogText(logChannel, "-", $@"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Server Application Init XXXXX SystemVersion: {SystemVersion}; BTnGSection: {RegistrySetup.GetRegistrySetting().BTnGSection}; XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", 
					"A10", "KioskService01.OnStart");
				//-----------------------------------------------------------------------------------------------------------------------------
				// Setting Clarification
				if ((_sysSetting.AvailablePaymentTypeList is null) || (_sysSetting.AvailablePaymentTypeList.Length == 0))
                {
					throw new Exception("No payment method found in setting parameter");
                }
				//-----------------------------------------------------------------------------------------------------------------------------
				_netInfoRepository = new NetInfoRepository();
				_netInterface = new LocalTcpService(_sysSetting.LocalServicePort);
		       _cashPaymentApp = new B2BPaymentApplication();

				// Standard Server Service Adaptors ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				// Module : UIKioskSales
				_salesSvr = new SalesServerSvcAdaptor(_netInterface, _netInfoRepository, _sysSetting.ApplicationGroup);
				// Module : UIPayment
		     _cashPaymentSvc = new CashPaymentServerSvcAdaptor(_netInterface, _cashPaymentApp, _netInfoRepository);

				// Module : UICollectTicket
				if (_sysSetting.IsBoardingPassEnabled)
					_colTickSvr = new CollectTicketSvrAdaptor(_netInterface, _netInfoRepository);
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				// Custom Device Server Service Adaptors
				// Module : UIB2B (Cash)
				if ( (from pM in _sysSetting.AvailablePaymentTypeList 
					  where pM == AppDecorator.Common.AppService.Sales.PaymentType.Cash 
					  select pM).ToArray().Length > 0)
                {
					_b2bSvr = new B2BServerServiceAdaptor(_netInterface, _netInfoRepository);
				}
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				// BTnGServerSvcAdaptor
				// Module : UIBTnG (Payment Gateway / eWallet)
				if ((from pM in _sysSetting.AvailablePaymentTypeList
					 where pM == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway
					 select pM).ToArray().Length > 0)
				{
					_bTnGSvr = new BTnGServerSvcAdaptor(_netInterface, _netInfoRepository);
				}
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 

				_log.LogText(logChannel, "SystemParam", SysParam, "PARAMETER", "KioskService01.OnStart");
				_log.LogText(logChannel, "SystemSetting", _sysSetting, "SETTING", "KioskService01.OnStart");

				//-----------------------------------------------------------------------------------------------------------------------------
				_sysSetting.HashSecretKey = RegistrySetup.GetRegistrySetting().HashSecretKey;
				_sysSetting.TVMKey = RegistrySetup.GetRegistrySetting().TVMKey;
			}
			catch (Exception ex)
			{
				_log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.OnStart");
				this.Stop();
				throw ex;
			}
			finally
			{
				_log.LogText(logChannel, "KioskService01", "End - KioskService01", "A100", "KioskService01.OnStart");
			}
		}

		// callback used to validate the certificate in an SSL conversation
		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
		{
			//bool result = cert.Subject.Contains("YourServerName");
			//return result;
			return true;
		}

		protected override void OnStop()
		{
			if (_netInterface != null)
			{
				try
				{
					_netInterface.ShutdownService();
				}
				catch { }
				try
				{
					_netInterface.Dispose();
				}
				catch { }

				_netInterface = null;
			}

			if (_netInfoRepository != null)
			{
				try
				{
					_netInfoRepository.Dispose();
				}
				catch { }

				_netInfoRepository = null;
			}

			if (_cashPaymentSvc != null)
			{
				try
				{
					_cashPaymentSvc.Dispose();
				}
				catch { }

				_cashPaymentSvc = null;
			}

			if (_cashPaymentApp != null)
			{
				try
				{
					_cashPaymentApp.ShutDown();
				}
				catch { }
				try
				{
					_cashPaymentApp.Dispose();
				}
				catch { }

				_cashPaymentApp = null;
			}

			if (_b2bSvr != null)
			{
				try
				{
					_b2bSvr.Dispose();
				}
				catch { }

				_b2bSvr = null;
			}

			if (_bTnGSvr != null)
			{
				try
				{
					_bTnGSvr.Dispose();
				}
				catch { }

				_bTnGSvr = null;
			}

			try
			{
				NssIT.Kiosk.AppDecorator.Config.Setting.Shutdown();
			}
			catch { }

			Task.Delay(500).Wait();
		}

		private void ValidateRegistrySetting()
        {
			RegistrySetup regSet = RegistrySetup.GetRegistrySetting();

			if (string.IsNullOrWhiteSpace(regSet.BTnGSection))
				throw new Exception("Invalid Windows setting; no setting found.");

			if (string.IsNullOrWhiteSpace(regSet.HashSecretKey))
				throw new Exception("Invalid Windows setting; Web Access Special Words not found.");

			if (string.IsNullOrWhiteSpace(regSet.TVMKey))
				throw new Exception("Invalid Windows setting; Kiosk Special Words not found.");

			if (string.IsNullOrWhiteSpace(regSet.WebApiUrl))
				throw new Exception("Invalid Windows setting; Web API URL not found.");

			if (string.IsNullOrWhiteSpace(regSet.BTnGMerchantId))
				throw new Exception("Invalid Windows setting; Payment Gateway Account not found.");

			if (string.IsNullOrWhiteSpace(regSet.BTnGCommonCode))
				throw new Exception("Invalid Windows setting; Payment Gateway Common Words not found.");

			if (string.IsNullOrWhiteSpace(regSet.BTnGSpecialCode))
				throw new Exception("Invalid Windows setting; Payment Gateway Special Words not found.");
			
		}
	}
}
