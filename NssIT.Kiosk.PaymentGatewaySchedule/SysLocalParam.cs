using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.PaymentGatewaySchedule
{
	/* Sample Parameters.txt
	 * 
	BaseURL=localhost/kioskmelaka
	KioskID=KIOSK02
	DBServer=DESKTOP-62O75L5
	PayMethod=C&D
	PrmPayWaveCOM=COM6
	xxxxx
	* Use "xxxxx" (5"x") to indicate end of Parameters setting.
	* Use "=" to assign value
	* A Space is not allowed in " " parameter setting.
	* PayMethod : C for Cash, D for Paywave(Credit Card); C&D mean Cash and Paywave ;
		Like ..
		PayMethod=D
			=> PayWave Only
		PayMethod=C
			=> Cash Only
		PayMethod=C&D
			=> Cash and PayWave
	* Use "&" for "and". Like C&D .
	* Value of a parameter always in Capital Letter.
	*/
	public class SysLocalParam : IDisposable
	{
		private const string _endOfParams = "XXXXX";

		private string _fullParamFileName = null;
		public bool PrmIsDebugMode { get; private set; } = false;
		public string PrmWebApiURL { get; private set; } = null;
		public string PrmKioskId { get; private set; } = null;

		public SysLocalParam() { }

		private string FullParamFileName
		{
			get
			{
				if (_fullParamFileName is null)
				{
					string execFilePath = Assembly.GetExecutingAssembly().Location;
					FileInfo fInf = new FileInfo(execFilePath);
					string execFolder = fInf.DirectoryName;
					_fullParamFileName = $@"{execFolder}\Parameter.txt";
				}
				return _fullParamFileName;
			}
		}

		public void ReadParameters()
		{
			try
			{
				string[] retParam = File.ReadAllLines(FullParamFileName);

				if ((retParam == null) || (retParam.Length == 0))
				{
					throw new Exception("No parameters found. Please make sure Parameters.txt has exist with valid values.");
				}

				// Reset Values
				PrmIsDebugMode = false;

				ReadAllValues(retParam);

				//if ((PrmWebApiURL.Length == 0) || (PrmLocalServicePort.Length == 0) || (PrmPayMethod.Length == 0))
				//{
				//	throw new Exception("Please set parameters for Base Url, Kiosk Id, and DB-server");
				//}
			}
			finally
			{

			}
		}

		private void ReadAllValues(string[] parameters)
		{
			string lineParam = null;
			string prmNm = null;
			string prmVal = null;
			int eqInx = -1;
			foreach (string aStr in parameters)
			{
				eqInx = -1;
				lineParam = aStr.Trim().Replace(" ", "");

				// if end of parameter has found ..
				if ((lineParam.Length >= 5) && (lineParam.Substring(0, 5).ToUpper().Equals(_endOfParams)))
					break;
				// -- -- -- -- --

				eqInx = lineParam.IndexOf("=");

				if (eqInx <= 0)
					continue;

				// Read a param value
				prmNm = lineParam.Substring(0, eqInx).ToUpper();
				prmVal = "";
				if (lineParam.Length >= (eqInx + 2))
					prmVal = lineParam.Substring(eqInx + 1).Trim();

				if (prmNm.ToUpper().Equals("IsDebugMode", StringComparison.InvariantCultureIgnoreCase))
				{
					if (bool.TryParse(prmVal, out bool res))
					{
						PrmIsDebugMode = res;
					}
					else
					{
						PrmIsDebugMode = false;
					}
				}

				else if (prmNm.ToUpper().Equals("WebApiURL", StringComparison.InvariantCultureIgnoreCase))
				{
					PrmWebApiURL = prmVal;

					PrmWebApiURL = string.IsNullOrEmpty(PrmWebApiURL) ? "" : PrmWebApiURL.Trim();

					if (PrmWebApiURL.Substring(PrmWebApiURL.Length - 1, 1).Equals(@"/") || PrmWebApiURL.Substring(PrmWebApiURL.Length - 1, 1).Equals(@"\"))
						PrmWebApiURL = PrmWebApiURL;
					else
						PrmWebApiURL = PrmWebApiURL + "/";

				}

				else if (prmNm.ToUpper().Equals("KioskId", StringComparison.InvariantCultureIgnoreCase))
				{
					PrmKioskId = prmVal;
				}

			}
		}

		public void Dispose()
		{

		}
	}
}