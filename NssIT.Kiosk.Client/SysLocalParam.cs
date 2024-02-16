using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client
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

		//Mandatory Parameters
		public int PrmClientPort { get; private set; } = -1;
		public int PrmLocalServerPort { get; private set; } = -1;

		// public string PrmAcroRd32FilePath { get; private set; } = null;
		// public string PrmPayMethod { get; private set; } = null;

		//Optional Parameters
		public bool PrmNoPaymentNeed { get; private set; } = false;
		public bool PrmIsDebugMode { get; private set; } = false;
		public bool PrmIsDemo { get; private set; } = false;
		public bool PrmMyKadScanner { get; private set; } = false;
		public string PrmAppGroupCode { get; private set; } = "Melaka";
		public bool IsNoOperationTime { get; private set; } = false;
		public string PrmPayWaveCOM { get; private set; } = null;
        public bool PrmNoCardSettlement { get; private set; } = false;

        public string StartOperationTime { get; private set; } = null;
		public string EndOperationTime { get; private set; } = null;

        public string PrmCardSettlementTime { get; private set; } = null;
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

		public AppGroup PrmAppGroup
		{
			get
			{
				string grpCode = (PrmAppGroupCode ?? "").Trim().ToUpper();

				if (grpCode.IndexOf("LARKIN") >= 0)
					return AppGroup.Larkin;
				else if (grpCode.IndexOf("GOMBAK") >= 0)
					return AppGroup.Gombak;
				else if (grpCode.IndexOf("KLANG") >= 0)
					return AppGroup.Klang;
				else if (grpCode.IndexOf("GENTING") >= 0)
					return AppGroup.Genting;
				else
					return AppGroup.MelakaSentral;
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

				PrmIsDemo = false;
				PrmIsDebugMode = false;
				PrmNoPaymentNeed = false;
				PrmMyKadScanner = false;
                PrmNoCardSettlement = false;
                PrmPayWaveCOM = null;
				IsNoOperationTime = false;
				StartOperationTime = null;
				EndOperationTime = null;
                PrmCardSettlementTime = null;
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

				else if (prmNm.ToUpper().Equals("IsDemo", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if (prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0, 1).ToUpper();

					if (char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
					{
						PrmIsDemo = true;
					}
					else
					{
						PrmIsDemo = false;
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

                else if(prmNm.ToUpper().Equals("IsNoOperationTime", StringComparison.InvariantCultureIgnoreCase))
				{
					string char1 = "";
					if(prmVal.Trim().Length > 0)
						char1 = prmVal.Trim().Substring(0,1).ToUpper();
					if(char1.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
						IsNoOperationTime = true;
					else
						IsNoOperationTime = false;
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
				//PrmWithMyKadScanner
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
				else if(prmNm.ToUpper().Equals("PayWaveCOM", StringComparison.InvariantCultureIgnoreCase))
				{
                    PrmPayWaveCOM = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.Trim().ToUpper();
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
				else if(prmNm.ToUpper().Equals("StartOperationTime", StringComparison.InvariantCultureIgnoreCase))
				{
					string timeString = prmVal.Trim();
					if(CheckIsTimeStringValid(timeString) == false)
					{
                        throw new Exception("Invalid StartOperationTime parameter. Please entry time in HH:mm (24Hours format).");

                    }

					StartOperationTime = string.IsNullOrWhiteSpace(prmVal) ? null : timeString;
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

                else if(prmNm.ToUpper().Equals("EndOperationTime", StringComparison.InvariantCultureIgnoreCase))
				{
					string timeString = prmVal.Trim();
					if(CheckIsTimeStringValid(timeString) == false)
					{
                        throw new Exception("Invalid EndOperationtime parameter. Please entry time in HH:mm (24Hours format).");

                    }

					EndOperationTime = string.IsNullOrWhiteSpace(prmVal)? null : timeString;
                }
				//else if (prmNm.ToUpper().Equals("AcroRd32", StringComparison.InvariantCultureIgnoreCase))
				//{
				//	PrmAcroRd32FilePath = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.Trim();
				//}
				//else if (prmNm.ToUpper().Equals("PayMethod", StringComparison.InvariantCultureIgnoreCase))
				//{
				//	PrmPayMethod = string.IsNullOrWhiteSpace(prmVal) ? null : prmVal.ToUpper().Trim();
				//}
				else if (prmNm.ToUpper().Equals("AppGroupCode", StringComparison.InvariantCultureIgnoreCase))
				{
					if (prmVal.Length > 0)
					{
						PrmAppGroupCode = prmVal.ToUpper();
					}
				}
			}
		}


		private CultureInfo _dateProvider = CultureInfo.InvariantCulture;
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
		{  }
	}
}