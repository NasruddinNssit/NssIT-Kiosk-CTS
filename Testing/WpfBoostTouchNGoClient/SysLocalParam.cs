using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoClient
{
	/* Sample Parameters.txt
	 * 
	ClientPort=9838
	LocalServerPort=7385
	IsDebugMode=true
	AcroRd32=C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe

	xxxxx
	* Use "xxxxx" (5"x") to indicate end of Parameters setting.
	* Use "=" to assign value
	* A Space " " is not allowed in parameter setting except a file path.
	* PayMethod : C for Cash, D for Paywave(Credit Card); C&D mean Cash and Paywave ;
		D = Paywave Only
		C = Cash Only
		C&D = Paywave & Cash
		Use "&" for "and". Like C&D .

	* IsDebugMode : This is an optionanl parameter. Remove this parameter if not necessary.
				: Example -> IsDebugMode=true

	* ClientPort      : Port number used for data receiving in client application (NssIT.Kiosk.Client.exe)
	* LocalServerPort : Port number used for data receiving at Local Server (NssIT.Kiosk.Server.exe / Windows Service)
	* AcroRd32 : File Location of Adobe Reader (AcroRd32.exe); Example like --> C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe
	*****IsDebugMode=true
	* Value of a parameter always in Capital Letter.
	*/
	public class SysLocalParam : IDisposable
	{
		private const string _endOfParams = "XXXXX";
		private string _fullParamFileName = null;

		private CultureInfo _dateProvider = CultureInfo.InvariantCulture;

		//Mandatory Parameters
		public int PrmClientPort { get; private set; } = -1;
		public int PrmLocalServerPort { get; private set; } = -1;
		//public string PrmAcroRd32FilePath { get; private set; } = null;
		//public string PrmWebApiURL { get; private set; } = null;
		public string PrmPayWaveCOM { get; private set; } = null;

		//public string PrmPayMethod { get; private set; } = null;

		/// <summary>
		/// Time shown HH:mm in 24 Hours format.
		/// </summary>
		public string PrmCardSettlementTime { get; private set; } = null;

		//Optional Parameters
		public bool PrmNoPaymentNeed { get; private set; } = false;
		public bool PrmIsDebugMode { get; private set; } = false;
		public bool PrmMyKadScanner { get; private set; } = false;
		public bool PrmNoCardSettlement { get; private set; } = false;
		public bool? PrmCheckPrinterPaperLow { get; private set; } = null;
		public string PrmPrinterName { get; private set; } = null;

		public string FinalizedPrinterName { get; private set; } = null;

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
				PrmClientPort = -1;
				PrmLocalServerPort = -1;
				//PrmAcroRd32FilePath = null;
				//PrmPayMethod = null;

				PrmIsDebugMode = false;
				PrmNoPaymentNeed = false;
				PrmMyKadScanner = false;
				PrmNoCardSettlement = false;
				PrmCheckPrinterPaperLow = null;
				PrmPayWaveCOM = null;
				PrmCardSettlementTime = null;
				PrmPrinterName = null;

				FinalizedPrinterName = null;

				ReadAllValues(retParam);

				//if ((PrmBaseURL.Length == 0) || (PrmDBServer.Length == 0) || (PrmKioskID.Length == 0))
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
				//lineParam = aStr.Trim().Replace(" ", "");
				lineParam = aStr.Trim();

				// if end of parameter has found ..
				if ((lineParam.Length >= 5) && (lineParam.Substring(0, 5).ToUpper().Equals(_endOfParams)))
					break;
				// -- -- -- -- --

				eqInx = lineParam.IndexOf("=");

				if (eqInx <= 0)
					continue;

				// Read a param value
				prmNm = lineParam.Substring(0, eqInx).ToUpper().Trim();
				prmVal = "";
				if (lineParam.Length >= (eqInx + 2))
					prmVal = lineParam.Substring(eqInx + 1).Trim();

				if (prmNm.ToUpper().Equals("IsDebugMode", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmIsDebugMode = true;
					}
					else
					{
						PrmIsDebugMode = false;
					}
				}
				else if (prmNm.ToUpper().Equals("NoPaymentNeed", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmNoPaymentNeed = true;
					}
					else
					{
						PrmNoPaymentNeed = false;
					}
				}

				else if (prmNm.ToUpper().Equals("MyKadScanner", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmMyKadScanner = true;
					}
					else
					{
						PrmMyKadScanner = false;
					}
				}

				else if (prmNm.ToUpper().Equals("NoCardSettlement", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmNoCardSettlement = true;
					}
					else
					{
						PrmNoCardSettlement = false;
					}
				}

				else if (prmNm.ToUpper().Equals("CheckPrinterPaperLow", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("N", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmCheckPrinterPaperLow = false;
					}
					else
					{
						PrmCheckPrinterPaperLow = true;
					}
				}

				else if (prmNm.ToUpper().Equals("ClientPort", StringComparison.InvariantCultureIgnoreCase))
				{
					int intPortX = -1;
					if (int.TryParse(prmVal, out intPortX))
					{
						if (intPortX > 0)
						{
							PrmClientPort = intPortX;
						}
						else
							PrmClientPort = -1;
					}
				}
				else if (prmNm.ToUpper().Equals("LocalServerPort", StringComparison.InvariantCultureIgnoreCase))
				{
					int intPortX = -1;
					if (int.TryParse(prmVal, out intPortX))
					{
						if (intPortX > 0)
						{
							PrmLocalServerPort = intPortX;
						}
						else
							PrmLocalServerPort = -1;
					}
				}

				//else if (prmNm.ToUpper().Equals("WebApiURL", StringComparison.InvariantCultureIgnoreCase))
				//{
				//	PrmWebApiURL = prmVal;

				//	PrmWebApiURL = string.IsNullOrEmpty(PrmWebApiURL) ? "" : PrmWebApiURL.Trim();

				//	if (PrmWebApiURL.Substring(PrmWebApiURL.Length - 1, 1).Equals(@"/") || PrmWebApiURL.Substring(PrmWebApiURL.Length - 1, 1).Equals(@"\"))
				//		PrmWebApiURL = PrmWebApiURL;
				//	else
				//		PrmWebApiURL = PrmWebApiURL + "/";

				//}

				//else if (prmNm.ToUpper().Equals("AcroRd32", StringComparison.InvariantCultureIgnoreCase))
				//{
				//	PrmAcroRd32FilePath = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.Trim();
				//}

				else if (prmNm.ToUpper().Equals("PayWaveCOM", StringComparison.InvariantCultureIgnoreCase))
				{
					PrmPayWaveCOM = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.Trim().ToUpper();
				}

				else if (prmNm.ToUpper().Equals("CardSettlementTime", StringComparison.InvariantCultureIgnoreCase))
				{
					string timeString = prmVal.Trim();

					if (CheckIsTimeStringValid(timeString) == false)
					{
						throw new Exception("Invalid CardSettlementTime parameter. Please entry time in HH:mm (24Hours format).");
					}

					PrmCardSettlementTime = string.IsNullOrWhiteSpace(prmVal) ? null : timeString;
				}

				else if (prmNm.ToUpper().Equals("PrinterName", StringComparison.InvariantCultureIgnoreCase))
				{
					string prtName = prmVal.Trim();
					PrmPrinterName = string.IsNullOrWhiteSpace(prmVal) ? null : prtName;
				}

				//else if (prmNm.ToUpper().Equals("PayMethod", StringComparison.InvariantCultureIgnoreCase))
				//{
				//    PrmPayMethod = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.ToUpper().Trim();
				//}
			}
		}

		public void SetPrinterName(string printerName)
		{
			FinalizedPrinterName = printerName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeString">In format "HH:mm"</param>
		/// <returns></returns>
		private bool CheckIsTimeStringValid(string timeString)
		{
			string dateStr = $@"2020/09/16 {timeString}";
			if (DateTime.TryParseExact(dateStr, "yyyy/MM/dd HH:mm", _dateProvider, DateTimeStyles.None, out DateTime res) == true)
				return true;
			else
				return false;
		}

		//public bool IsPayMethodValid
		//{
		//	get
		//	{
		//		bool result = false;

		//		if (PrmPayMethod is null)
		//			result = false;
		//		else if (PrmPayMethod.Equals("C"))
		//			result = true;

		//		return result;
		//	}
		//}

		public void Dispose()
		{ }
	}
}