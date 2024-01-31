using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFmServerApplicationTest
{
    static class Program
    {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			string logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\WFmWebServiceTest\LogDB\NssITKioskLog01_Test.db;Version=3";
			NssIT.Kiosk.AppDecorator.Config.Setting sysSetting = null;

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

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

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
