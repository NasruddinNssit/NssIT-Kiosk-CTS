using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Config
{
	public class Setting
	{
		private bool _isDebugMode = false;

		
		private string _appVersion = "**#**";

		private string _logDbConnectionStr = null;
		private string _webServiceURL = null;
		private string _webApiURL = null;
		private string _ipAddress = null;
		private string _payMethod = "C";
		private AppGroup _appGroup = AppGroup.Unknown;
		private int _localServicePort = -1;
		private string _kioskId = null;
		private string _stationCode = null;
		private bool _isBoardingPassEnabled = false;
		private bool _isOnSkyWaySell = false;
		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static Setting _setting = null;
		public static string NullVersion { get => "**#**"; }
		private bool _isNoOperationTime = false;
		public string TVMKey { get; set; }
		public string HashSecretKey { get; set; }

		

		private Setting()
		{
			_appVersion = NullVersion;
		}

		public bool IsNullVersion
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_appVersion) == true)
					return true;
				else if (_appVersion?.Equals(NullVersion) == true)
					return true;
				return false;
			}
		}

		public static Setting GetSetting()
		{
			if (_setting == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_setting == null)
					{
						_setting = new Setting();
					}
					return _setting;
				}
				finally
				{
					_manLock.Release();
				}
			}
			else
				return _setting;
		}

		public static void Shutdown()
		{
			try
			{
				_manLock.WaitAsync().Wait();
				_setting = null;
			}
			finally
			{
				_manLock.Release();
			}
		}

		/// <summary>
		/// Version string not allows Blank/NULL.
		/// </summary>
		public string ApplicationVersion
		{
			get => _appVersion;
			set
			{
				if (value?.Trim().Length > 0)
					_appVersion = value.Trim();
			}
		}

		public string LogDbConnectionStr
		{
			get
			{
				return _logDbConnectionStr;
			}
			set
			{
				_logDbConnectionStr = string.IsNullOrWhiteSpace(value) ? null: value.Trim();
			}
		}

		public bool IsDebugMode
		{
			get
			{
				return _isDebugMode;
			}
			set
			{
				_isDebugMode = value;
			}
		}

		public bool IsBoardingPassEnabled
		{
			get
			{
				return _isBoardingPassEnabled;
			}
			set
			{
				_isBoardingPassEnabled = value;
			}
		}

        public bool IsOnSkyWaySell
        {
            get
            {
                return _isOnSkyWaySell;
            }
            set
            {
                _isOnSkyWaySell = value;
            }
        }

		public bool NoOperationTime
		{
			get
			{
				return _isNoOperationTime;
			}
			set
			{
				_isNoOperationTime = value;
			}
		}

        public string KioskId
		{
			get
			{
				return _kioskId;
			}
			set
			{
				_kioskId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		public string StationCode
		{
			get
			{
				return _stationCode;
			}
			set
			{
				_stationCode = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		public string WebApiURL
		{
			get
			{
				return _webApiURL;
			}
			set
			{
				_webApiURL = FixWebApiURL(value);
				EvaWebApiInfo();
			}
		}

		private string FixWebApiURL(string webApiURL)
		{
			string retUrl = webApiURL;

			retUrl = string.IsNullOrEmpty(retUrl) ? "" : retUrl.Trim();

			if (retUrl.Substring(retUrl.Length - 1, 1).Equals(@"/") || retUrl.Substring(retUrl.Length - 1, 1).Equals(@"\"))
				retUrl = retUrl;
			else
				retUrl = retUrl + "/";

			return retUrl;

		}

		public bool IsLiveWebApi => (WebAPICode == WebAPISiteCode.Live);
		public WebAPISiteCode WebAPICode { get; private set; } = WebAPISiteCode.Unknown;

		public string WebServiceURL
		{
			get
			{
				return _webServiceURL;
			}
			set
			{
				_webServiceURL = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		public int BTnGMinimumWaitingPeriod { get; set; } = 90;

		public PaymentType[] AvailablePaymentTypeList { get; set; } = new PaymentType[0];

		/// <summary>
		/// Assign IP for Local Machine/PC
		/// </summary>
		public string IPAddress
		{
			get
			{
				return _ipAddress;
			}
			set
			{
				_ipAddress = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		public string PayMethod
		{
			get
			{
				return _payMethod;
			}
			set
			{
				_payMethod = string.IsNullOrWhiteSpace(value) ? "C" : value.Trim();
			}
		}

		
		public int LocalServicePort
		{
			get
			{
				return _localServicePort;
			}
			set
			{
				_localServicePort = value;
			}
		}

		public AppGroup ApplicationGroup
		{
			get
			{
				return _appGroup;
			}
			set
			{
				_appGroup = value;
			}
		}

		public static string GetLocalIPAddress()
		{
			UnicastIPAddressInformation ucIP = null;

			NetworkInterface[] netIntfArr = NetworkInterface.GetAllNetworkInterfaces();

			IEnumerable<UnicastIPAddressInformation[]> unicastIPList
				= from ntInf in netIntfArr
				  where (ntInf.OperationalStatus == OperationalStatus.Up) &&
				  (ntInf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ntInf.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				  select ntInf.GetIPProperties().UnicastAddresses.ToArray();

			foreach (UnicastIPAddressInformation[] unicIPArr in unicastIPList)
			{
				UnicastIPAddressInformation ucIPX = (from ip in unicIPArr
													 where (ip != null && ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
													 select ip).Take(1).ToArray()[0];

				if (ucIPX != null)
				{
					ucIP = ucIPX;
					break;
				}
			}

			if (ucIP != null)
			{
				return ucIP.Address.ToString();
			}
			else
			{
				return null;
			}

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			//var host = Dns.GetHostEntry(Dns.GetHostName());
			//foreach (var ip in host.AddressList)
			//{
			//	if (ip.AddressFamily == AddressFamily.InterNetwork)
			//	{
			//		return ip.ToString();
			//	}
			//}
			//throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		private void EvaWebApiInfo()
		{
			WebAPICode = WebAPISiteCode.Live;

			if (string.IsNullOrWhiteSpace(_webApiURL) == false)
			{
				if (_webApiURL.IndexOf(@"ktmb-staging-api.azurewebsites.net", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Staging;
				}
				else if (_webApiURL.IndexOf(@"ktmb-dev-api.azurewebsites.net", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Development;
				}
				else if (_webApiURL.IndexOf(@"https://localhost", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Local_Host;
				}
				else if (_webApiURL.IndexOf(@"https://127.0.0.1", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Local_Host;
				}
			}
		}
	}
}
