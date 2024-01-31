using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.PaymentGatewaySchedule.SecurityProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.PaymentGatewaySchedule.Base
{
	public static class SysInit
	{
		private const string LogChannel = "SysInit";

		public static void Start(string versionString)
		{
			SysLocalParam SysParam = new SysLocalParam();
			SysParam.ReadParameters();

			NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

			_sysSetting.ApplicationVersion = versionString;

			//_sysSetting.EWalletWebApiBaseURL = SysParam.PrmEWalletWebApiBaseURL;
			//_sysSetting.LocalServicePort = SysParam.PrmLocalServerPort;
			//_sysSetting.PayMethod = SysParam.PrmPayMethod;

			_sysSetting.IsDebugMode = SysParam.PrmIsDebugMode;
			_sysSetting.WebApiURL = SysParam.PrmWebApiURL;
			_sysSetting.KioskId = SysParam.PrmKioskId;

			//if (_sysSetting.IsDebugMode)
			//{
			//	_sysSetting.IPAddress = "10.1.1.111";
			//	//_sysSetting.IPAddress = "10.238.4.15";
			//}
			//else
			_sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

			// CheckRegistryConfig();

			DbLog.GetDbLog()?.LogText(LogChannel, "*", _sysSetting, "K01", "SysInit.Start");

			Task.Delay(1000).Wait();
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			AuthenticationSet auth = new AuthenticationSet(_sysSetting.WebApiURL);
			_sysSetting.HashSecretKey = auth.HashSecretKey;
			_sysSetting.TVMKey = auth.TVMKey;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		}

		private static void CheckRegistryConfig()
		{
			Thread testT = new Thread(new ThreadStart(new Action(() => {
				try
				{
					string rValue = RegistrySetup.GetRegistrySetting().DeviceId;
					//_msg.ShowMessage($@"Current Device Id : {rValue}");
				}
				catch (Exception ex)
				{

					DbLog.GetDbLog()?.LogError(LogChannel, "*", ex, "EX01", "SysInit.CheckRegistryConfig");
					//MessageBox.Show($@"Error when Check Registry Config.; {ex.ToString()}");
				}
			})));
			testT.IsBackground = true;
			testT.Start();
		}
	}
}
