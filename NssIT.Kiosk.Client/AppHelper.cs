using NssIT.Kiosk.Client.NetClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client
{
	/// <summary>
	/// Application Helper
	/// </summary>
    public class AppHelper
    {
		private string _logChannel = "AppSys";

		private bool _isClientStartedSuccessful = false;

		/// <summary>
		/// Return true when Valid Logon.
		/// </summary>
		/// <returns></returns>
		public bool SystemHealthCheck()
		{
			bool isServerResponded = false;
			bool serverWebServiceIsDetected = false; 
			bool serverAppHasDisposed = true;
			bool serverAppHasShutdown = true;

			if (_isClientStartedSuccessful == false)
				App.NetClientSvc.SalesService.QuerySalesServerStatus(out isServerResponded, out serverAppHasDisposed,
						out serverAppHasShutdown, out serverWebServiceIsDetected, waitDelaySec: 60);

			if ((_isClientStartedSuccessful) || (isServerResponded && serverWebServiceIsDetected))
			{
				_isClientStartedSuccessful = true;

				App.NetClientSvc.SalesService.WebServerLogon(out bool isServerRespondedX, out bool isLogonSuccessX,
					out bool isNetworkTimeoutX, out bool isValidAuthenticationX, out bool isLogonErrorFoundX, out string errorMessageX, waitDelaySec: 90);

				if (isServerRespondedX && isValidAuthenticationX && (isLogonErrorFoundX == false))
				{
					bool isLowCoin = false;
					bool isCoinMachRecoveryInProgressAfterDispense = false;
					string errorMsg = null;

					if (App.SysParam.PrmNoPaymentNeed)
					{
						return true;
					} 
					else if ((App.AvailablePaymentTypeList is null) || (App.AvailablePaymentTypeList.Length == 0))
					{
						throw new Exception($@"No payment method available; (EXIT10000022)");
					}
					else if (App.CheckIsPaymentTypeAvailable(AppDecorator.Common.AppService.Sales.PaymentType.Cash))
					{
						if ((App.SysParam.PrmNoPaymentNeed) || (!App.NetClientSvc?.CashPaymentService?.CheckCashMachineIsReady("-", 
							out isLowCoin, out isCoinMachRecoveryInProgressAfterDispense, out errorMsg, 90) == true))
						{
							return true;
						}
						else
						{
							string errMsg = "Cash Machine not ready";

							if (isCoinMachRecoveryInProgressAfterDispense)
								errMsg += "; Coin machine under a short maintenance. This may take 15 seconds to 3 minutes";

							if (isLowCoin)
								errMsg += "; Low coin encountered";
							if (string.IsNullOrEmpty(errorMsg) == false)
								errMsg += $@"; {errorMsg}";

							throw new Exception($@"{errMsg}; (EXIT10000021)");
						}
					}
					else
					{
						return true;
					}
				}
				else
				{
					if (isServerRespondedX == false)
					{
						_isClientStartedSuccessful = false;
						throw new Exception("Local Server not responding; (EXIT10000001)");
					}
					else if (isNetworkTimeoutX)
					{
						_isClientStartedSuccessful = false;
						throw new Exception("Network Timeout; (EXIT10000002)");
					}
					else if (isValidAuthenticationX == false)
					{
						throw new Exception("Invalid Authentication; (EXIT10000003)");
					}
					else if (isLogonSuccessX == false)
					{
						throw new Exception("Fail login; (EXIT10000004)");
					}
					else if (isLogonErrorFoundX == true)
					{
						throw new Exception($@"Error when logon; (EXIT10000005); {errorMessageX}");
					}
					else
					{
						_isClientStartedSuccessful = false;
						App.Log.LogText(_logChannel, "-", $@"Sales Server Service has unknown error; (EXIT10000006); isServerRespondedX: {isServerRespondedX}; isLogonSuccessX: {isLogonSuccessX}; isNetworkTimeoutX: {isNetworkTimeoutX}; isValidAuthenticationX: {isValidAuthenticationX}; isLogonErrorFoundX: {isLogonErrorFoundX}; errorMessageX: {errorMessageX}"
							, "A20", "pgIntro.SystemInit", AppDecorator.Log.MessageType.Error);
						throw new Exception($@"Sales Server Service has unknown error; (EXIT10000006)");
					}
				}
			}
			else
			{
				_isClientStartedSuccessful = false;

				if (isServerResponded == false)
					throw new Exception("Local Server not responding; (EXIT10000011)");
				else if (serverAppHasDisposed)
					throw new Exception("Local Server disposed; (EXIT10000012)");
				else if (serverAppHasShutdown)
					throw new Exception("Local Server already shutdown; (EXIT10000013)");
				else if (serverWebServiceIsDetected == false)
					throw new Exception("Web Server not detected; (EXIT10000014)");
				else
				{
					App.Log.LogText(_logChannel, "-", $@"Sales Server has unknown error; (EXIT10000015); isServerResponded: {isServerResponded}; serverAppHasDisposed: {serverAppHasDisposed}; serverAppHasShutdown: {serverAppHasShutdown}; serverWebServiceIsDetected: {serverWebServiceIsDetected}"
							, "A20", "pgIntro.SystemInit", AppDecorator.Log.MessageType.Error);
					throw new Exception("Local Server has unknown error; (EXIT10000015)");
				}
			}
		}
	}
}
