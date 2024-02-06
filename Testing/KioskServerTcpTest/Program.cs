using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Device.B2B.B2BApp;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.Server.Service.Adaptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.AppService.Network;
using NssIT.Kiosk.Device.B2B.Server.Service.Adaptor;
using System.Configuration;
using System.Net.NetworkInformation;

namespace KioskServerTcpTest
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			string logChannel = "WFmMainWindow";
			string logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\KioskServerTcpTest\LogDB\NssITKioskLog01.db;Version=3";

			NssIT.Kiosk.AppDecorator.Config.Setting sysSetting = null;
			NssIT.Kiosk.Log.DB.DbLog log = null;
			INetMediaInterface netInterface = null;

			SalesServerSvcAdaptor salesSvr = null;
			CashPaymentServerSvcAdaptor cashPaymentSvr = null;
			B2BServerServiceAdaptor b2bSvr = null;

			IUIPayment cashPaymentApp = null;

			NetInfoRepository netInfoRepository = null;

			try
			{
				sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
				sysSetting.LogDbConnectionStr = logDBConnStr;

				sysSetting.WebServiceURL = ConfigurationManager.AppSettings.Get("WebServiceURL");
				sysSetting.IsDebugMode = (bool.TryParse(ConfigurationManager.AppSettings.Get("IsDebugMode"), out bool isDebugMode) ? isDebugMode : false);

				if (sysSetting.IsDebugMode)
				{
					sysSetting.IPAddress = "10.1.1.111";
				}
				else
				{
					sysSetting.IPAddress = GetLocalIPAddress();
				}

				log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

				netInfoRepository = new NetInfoRepository();
				netInterface = new LocalTcpService(GetLocalServicePort());

				cashPaymentApp = new B2BPaymentApplication();

				// Kiosk's Standard Server Service Adaptors
				salesSvr = new SalesServerSvcAdaptor(netInterface, netInfoRepository, NssIT.Kiosk.AppDecorator.Common.AppGroup.MelakaSentral, false);
				cashPaymentSvr = new CashPaymentServerSvcAdaptor(netInterface, cashPaymentApp, netInfoRepository);

				// Custom Device Server Service Adaptors
				b2bSvr = new B2BServerServiceAdaptor(netInterface, netInfoRepository);

				Console.WriteLine("Server is ready..");
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				Console.WriteLine("");
				Console.WriteLine("Press enter to end..");
				Console.ReadLine();

				try
				{
					salesSvr?.Dispose();
				}
				catch { }

				try
				{
					cashPaymentSvr?.Dispose();
				}
				catch { }

				try
				{
					b2bSvr?.Dispose();
				}
				catch { }

				try
				{
					cashPaymentApp?.ShutDown();
					cashPaymentApp?.Dispose();
				}
				catch { }

				try
				{
					netInterface?.ShutdownService();
					netInterface?.Dispose();
				}
				catch { }

				try
				{
					netInfoRepository.Dispose();
				}
				catch { }

				Console.WriteLine("Press enter to exit..");
				Console.ReadLine();
			}

			int GetLocalServicePort() => 7385;

			string GetLocalIPAddress()
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
		}
	}
}
