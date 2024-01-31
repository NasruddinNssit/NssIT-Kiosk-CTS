using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;

namespace NssIT.Kiosk.Server
{
	/* Sample Parameters.txt
	 * 
	BaseURL=localhost/kioskmelaka
	KioskID=KIOSK02
	DBServer=DESKTOP-62O75L5
	PayMethod=C,D
	PrmPayWaveCOM=COM6
	xxxxx
	* Use "xxxxx" (5"x") to indicate end of Parameters setting.
	* Use "=" to assign value
	* A Space is not allowed in " " parameter setting.
	* PayMethod : C for Cash, D for Paywave(Credit Card); C,D mean Cash and Paywave ;
		Like ..
		PayMethod=D
			=> PayWave Only
		PayMethod=C
			=> Cash Only
		PayMethod=C,D
			=> Cash and PayWave
	* Use "," for "and". Like C,D .
	* Value of a parameter always in Capital Letter.
	*/
	public class SysLocalParam : IDisposable
	{
		private const string _endOfParams = "XXXXX";
		private const int _bTnGMinimumWaitingPeriod = 90;

		private string _fullParamFileName = null;
		public bool PrmIsDebugMode { get; private set; } = false;
		public bool PrmIsBoardingPassEnabled { get; private set; } = false;
		public string PrmWebServiceURL { get; private set; } = null;
		public int PrmLocalServerPort { get; private set; } = -1;
		public string PrmPayMethod { get; private set; } = "C";
		public string PrmAppGroupCode { get; private set; } = "Melaka";

		public PaymentType[] PrmAvailablePaymentTypeList { get; private set; } = new PaymentType[0];

		/// <summary>
		/// 'Boost/Touch n Go' minimum waiting period in seconds. This period used for waiting response after 2D Barcode has shown.
		/// </summary>
		public int PrmBTnGMinimumWaitingPeriod { get; private set; } = _bTnGMinimumWaitingPeriod;
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
				PrmIsBoardingPassEnabled = false;
				PrmWebServiceURL = null;
				PrmLocalServerPort = -1;
				PrmPayMethod = "C";
				PrmAppGroupCode = "Melaka";
				PrmBTnGMinimumWaitingPeriod = _bTnGMinimumWaitingPeriod;
				PrmAvailablePaymentTypeList = new PaymentType[0];

				ReadAllValues(retParam);

				//if ((PrmWebServiceURL.Length == 0) || (PrmLocalServicePort.Length == 0) || (PrmPayMethod.Length == 0))
				//{
				//	throw new Exception("Please set parameters for Base Url, Kiosk Id, and DB-server");
				//}
			}
			finally
			{

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
				else
					return AppGroup.MelakaSentral;
			}
		}

		private void ReadAllValues(string[] parameters)
		{
			string lineParam = null;
			string prmNm = null;
			string prmVal = null;
			int eqInx = -1;
			List<PaymentType> payTypList = new List<PaymentType>();

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
				prmNm = lineParam.Substring(0, eqInx).ToUpper().Trim();
				prmVal = "";
				if (lineParam.Length >= (eqInx + 2))
					prmVal = lineParam.Substring(eqInx + 1).Trim();

				if (prmNm.ToUpper().Equals("IsDebugMode", StringComparison.InvariantCultureIgnoreCase))
				{
					if (bool.TryParse(prmVal, out bool res))
					{
						PrmIsDebugMode = res;
					}
					else if (prmVal.Substring(0, 1).ToUpper().Equals("T"))
					{
						PrmIsDebugMode = true;
					}
					else
					{
						PrmIsDebugMode = false;
					}
				}

				else if (prmNm.ToUpper().Equals("IsBoardingPassEnabled", StringComparison.InvariantCultureIgnoreCase))
				{
					if (bool.TryParse(prmVal, out bool res))
					{
						PrmIsBoardingPassEnabled = res;
					}
					else if (prmVal.Substring(0, 1).ToUpper().Equals("T"))
					{
						PrmIsBoardingPassEnabled = true;
					}
					else
					{
						PrmIsBoardingPassEnabled = false;
					}
				}

				else if (prmNm.ToUpper().Equals("WebServiceURL", StringComparison.InvariantCultureIgnoreCase))
				{
					PrmWebServiceURL = prmVal;					
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

				else if (prmNm.ToUpper().Equals("PayMethod", StringComparison.InvariantCultureIgnoreCase))
				{
					if (prmVal.Length > 0)
					{
						PrmPayMethod = prmVal.ToUpper().Replace("&", ",").Replace("   ", "").Replace("  ", "").Replace(" ", "");
						string[] payMethodArr = PrmPayMethod.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

						PaymentType tmpPaymentType = PaymentType.Unknown;
						foreach (string methodCode in payMethodArr)
                        {
							tmpPaymentType = PaymentType.Unknown;

							if (methodCode.Contains("C"))
								tmpPaymentType = PaymentType.Cash;

							else if (methodCode.Contains("D"))
								tmpPaymentType = PaymentType.CreditCard;

							else if (methodCode.Contains("T"))
								tmpPaymentType = PaymentType.PaymentGateway;

							if (tmpPaymentType != PaymentType.Unknown)
                            {
								if ((from pM in payTypList 
									 where pM == tmpPaymentType 
									 select pM).ToArray().Length == 0)
                                {
									payTypList.Add(tmpPaymentType);
								}
                            }
						}
					}
				}

				else if (prmNm.ToUpper().Equals("AppGroupCode", StringComparison.InvariantCultureIgnoreCase))
				{
					if (prmVal.Length > 0)
					{
						PrmAppGroupCode = prmVal.ToUpper();
					}
				}

				else if (prmNm.ToUpper().Equals("BTnGMinimumWaitingPeriod", StringComparison.InvariantCultureIgnoreCase))
				{
					if (string.IsNullOrWhiteSpace(prmVal) == false)
					{
						if ((int.TryParse(prmVal, out int intVal) == true) && (intVal > _bTnGMinimumWaitingPeriod))
						{
							PrmBTnGMinimumWaitingPeriod = intVal;
						}
					}
				}
			}

			PrmAvailablePaymentTypeList = payTypList.ToArray();

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			
			

		}

		public void Dispose()
		{

		}
	}
}
