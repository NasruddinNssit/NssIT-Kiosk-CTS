using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.B2BTools
{
	public class B2BCountryToCurrencyCode
	{
		private static SemaphoreSlim _syncLoc = new SemaphoreSlim(1);
		private static Dictionary<string, string> _list = new Dictionary<string, string>();

		public static string GetCurrencyString(string countryCode)
		{
			string retCurr = null;

			if (_list.Count == 0)
			{
				try
				{
					_syncLoc.WaitAsync().Wait();
					CreateCountryToCurrencyList();
				}
				catch (Exception ex)
				{
					throw ex;			
				}
				finally
				{
					_syncLoc.Release();
				}
			}

			retCurr = _list[(countryCode ?? "##")?.ToUpper()];
			retCurr = retCurr ?? "##";

			return retCurr;
		}

		private static void CreateCountryToCurrencyList()
		{
			_list.Add("MYR", "RM");
		}

		//class CountryToCurrency
		//{
		//	public string CountryCode { get; }
		//	public string Currency { get; }

		//	public CountryToCurrency(string countryCode, string currency)
		//	{
		//		CountryCode = countryCode;
		//		Currency = currency;
		//	}
		//}
	}
}
