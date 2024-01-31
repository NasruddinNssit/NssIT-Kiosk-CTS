using Newtonsoft.Json;

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command;
using NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.LowCoin;
using System.Threading;
using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.ConstantX;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public partial class AxCX2_3 : Form, IAxCX2_3
	{
		private const string LogChannel = "CX2_3Api";
		

		private Boolean _isRunningLowCoin10 = false;
		private Boolean _isRunningLowCoin20 = false;
		private Boolean _isRunningLowCoin50 = false;

		public bool IsRecoveryInProgressAfterDispense
		{
			get;
			private set;
		} = false;

		public LowCoinManager LowCoinMan 
		{
			get;
		} = new LowCoinManager();

		public AxCX2_3()
		{
			InitializeComponent();
		}

		public bool MachineConnected
		{
			get;
			private set;
		} = false;

		public Exception MachineConnectionError
		{
			get;
			private set;
		} = null;

		private string _currProcessId = "-";
		public string CurrentProcessId
		{
			get
			{
				return _currProcessId;
			}
			set
			{
				_currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
			}
		}

		//private void ShowMsg(string msg) { }
		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public bool ConnectDevice()
		{
			int lRetVal = -1;

			string strOcxVersion = "";
			strOcxVersion = "";

			try
			{
				Log.LogText(LogChannel, CurrentProcessId, $@"Start Coin Machine ..", "A01", "AxCX2_3.ConnectDevice");

				axTQ01001.CommunicationChannel = false;

				lRetVal = axTQ01001.SetProtocol(2);
				Log.LogText(LogChannel, CurrentProcessId, $@"SetProtocol return : {lRetVal};", "A02", "AxCX2_3.ConnectDevice");

				lRetVal = axTQ01001.InitPortSettings(ref strOcxVersion);
				Log.LogText(LogChannel, CurrentProcessId, $@"InitPortSettings return : {lRetVal}; strOcxVersion = ""{strOcxVersion}""", "A03", "AxCX2_3.ConnectDevice");

				// When Coin Machine Detected
				if (lRetVal == 1)
				{
					Log.LogText(LogChannel, CurrentProcessId, $@"Clearing Machine Status .. ", "A04", "AxCX2_3.ConnectDevice");
					axTQ01001.ClearMachineStatus();

					Log.LogText(LogChannel, CurrentProcessId, $@"Clearing Sensor Status .. ", "A05", "AxCX2_3.ConnectDevice");
					axTQ01001.ClearSensorStatus();

					lRetVal = -1;
					object tempObj = null;
					lRetVal = axTQ01001.EnableBuzzerSound(false, ref tempObj);
					Log.LogText(LogChannel, CurrentProcessId, $@"Set EnableBuzzerSound to false; return : {lRetVal}; Out obj : {JsonConvert.SerializeObject(tempObj)}", "A06", "AxCX2_3.ConnectDevice");

					MachineConnected = true;

					if (InitialMachStatus() == true)
						LowCoinMan.IsReqToStartApp = false;
					else
                    {
						LowCoinMan.IsReqToStartApp = true;
						throw new Exception("--Fail status reading for Coin Machine when start API--");
					}
				}
				else
                {
					throw new Exception("--Fail to connect coin machine--");
				}
			}
			catch (Exception ex)
			{
				MachineConnectionError = ex;
				MachineConnected = false;
				LowCoinMan.IsReqToStartApp = true;
				Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "AxCX2_3.ConnectDevice");
			}
			finally
			{
				Log.LogText(LogChannel, CurrentProcessId, $@"Done", "A100", "AxCX2_3.ConnectDevice");
			}

			return (lRetVal == 1) ? true : false;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			bool InitialMachStatus()
			{
				SpinMachineMotor(-1, "InitialMachStatus");

				_isRunningLowCoin10 = false;
				_isRunningLowCoin20 = false;
				_isRunningLowCoin50 = false;

				if (CheckLowCoinStatus(out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50))
				{
					string lowCoinTypeStr = "";
					_isRunningLowCoin10 = isRunningLowCoin10;
					_isRunningLowCoin20 = isRunningLowCoin20;
					_isRunningLowCoin50 = isRunningLowCoin50;		
					
					if (_isRunningLowCoin10) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "10 cents";
					if (_isRunningLowCoin20) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "20 cents";	
					if (_isRunningLowCoin50) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "50 cents";			
					
					LowCoinMan.UpdateLowCoinStatesOnInit(_isRunningLowCoin10, _isRunningLowCoin20, _isRunningLowCoin50);

					return true;
				}
				else
					return false;
			}
		}

		public bool DispenseCoin(int totalDispenseCents, out string notEnoughCoinMessage)
		{
			notEnoughCoinMessage = null;

			bool isDispenseSuccess = false;
			bool needToCheckCoinStatus = false;
			string dispenseCode = "";

			try
			{
				if (LowCoinMan.IsReqToStartApp)
					throw new Exception("Request to Restart Application");

				dispenseCode = GetDispenseCode(totalDispenseCents, out notEnoughCoinMessage);

				isDispenseSuccess = false;
				if ((notEnoughCoinMessage == null) && (LowCoinMan.IsDispensePossible))
				{
					string dispenseMsg = $@"Dispense; Coin Dispense Code : {dispenseCode}; ** 10 cents : {dispenseCode.Substring(0, 1)} pcs ; ** 20 cents : {dispenseCode.Substring(1, 1)} pcs ; ** 50 cents : {dispenseCode.Substring(2, 1)} pcs ;";
					string dispenseAdMsg = $@"Will Dispense Total Coin (cents): {totalDispenseCents}; 10 cents : {dispenseCode.Substring(0, 1)} pcs ; 20 cents : {dispenseCode.Substring(1, 1)} pcs ; 50 cents : {dispenseCode.Substring(2, 1)} pcs ";

					Log.LogText(LogChannel, CurrentProcessId, dispenseMsg,
						"A01", "AxCX2_3.DispenseCoin", adminMsg: dispenseAdMsg);

					Log.LogText(LogChannel, CurrentProcessId, "Start Dispend Coin",
						"A01", "AxCX2_3.DispenseCoin", adminMsg: "Start Dispend Coin");

					try
					{
						needToCheckCoinStatus = true;

						isDispenseSuccess = axTQ01001.DispenseByColumn(dispenseCode);
						WaitDelay(4300); // Best solution before check status with ShowBinStatus();

						LowCoinMan.UpdateDispenseStatus(dispenseCode);

						if (isDispenseSuccess)
						{
							Log.LogText(LogChannel, CurrentProcessId, "End Dispense Coin; Dispense coin command sent successfully",
								"B06", "AxCX2_3.DispenseCoin", adminMsg: "End Dispense Coin; Dispense coin command sent successfully");
							/////break;
						}
						else
                        {
							LowCoinMan.IsReqToStartApp = true;
							Log.LogText(LogChannel, CurrentProcessId, $@"--Fail dispense coin; Need to check coin machine and restart application service--",
								"B06", "AxCX2_3.DispenseCoin", adminMsg: $@"--Fail dispense coin; Need to check coin machine and restart application service--");
						}
					}
					catch (Exception ex)
					{
						LowCoinMan.IsReqToStartApp = true;
						Log.LogError(LogChannel, CurrentProcessId, ex, "EX101", "AxCX2_3.DispenseCoin", adminMsg: $@"{ex.Message}; --Fail dispense coin; Need to check coin machine and restart application service--");
					}

					_isRunningLowCoin10 = false;
					_isRunningLowCoin20 = false;
					_isRunningLowCoin50 = false;

					bool isCheckStatusSuccess = CheckLowCoinStatus(out bool is10CentCoinLow, out bool is20CentCoinLow, out bool is50CentCoinLow);

					needToCheckCoinStatus = false;

					if (isCheckStatusSuccess)
					{
						_isRunningLowCoin10 = is10CentCoinLow;
						_isRunningLowCoin20 = is20CentCoinLow;
						_isRunningLowCoin50 = is50CentCoinLow;

						if (isDispenseSuccess == true)
						{
							///// Recovery After Coin Dispense --------------------------------------------------------------------------------
							try 
							{
								IsRecoveryInProgressAfterDispense = true;

								/////CYA-DEBUG..Testing ------------
								/////WaitDelay(60000);
								/////------------------------

								MaintainCoinMachine();
							}
							catch (Exception ex)
                            {
								Log.LogError(LogChannel, CurrentProcessId, ex, "EX61", "AxCX2_3.DispenseCoin");
							}
							finally
                            {
								IsRecoveryInProgressAfterDispense = false;
							}
							/////----------------------------------------------------------------------------------------------------------------

							string lowMsgX = LowCoinMan.GetStatusMessage();
							if (lowMsgX != null)
								Log.LogText(LogChannel, CurrentProcessId, lowMsgX,
									"C01", "AxCX2_3.DispenseCoin", adminMsg: lowMsgX);

							if (LowCoinMan.IsDispensePossible == false)
								Log.LogText(LogChannel, CurrentProcessId, $@"Coin machine has been blocked; Dispensing coin is not possible",
									"C02", "AxCX2_3.DispenseCoin", adminMsg: $@"Coin machine has been blocked; Dispensing coin is not possible");
						}
					}
					else
                    {
						LowCoinMan.IsReqToStartApp = true;

						Log.LogText(LogChannel, CurrentProcessId, $@"--Fail to read coin machine status--; Request to restart service application",
									"C12", "AxCX2_3.DispenseCoin", adminMsg: $@"--Fail to read coin machine status--; Request to restart service application");
					}
				}
				else
				{
					if (notEnoughCoinMessage != null)
                    {
						Log.LogText(LogChannel, CurrentProcessId, notEnoughCoinMessage, "G04", "AxCX2_3.DispenseCoin",
							adminMsg: notEnoughCoinMessage);
					}

					if (LowCoinMan.IsDispensePossible)
					{
						Log.LogText(LogChannel, CurrentProcessId, "Dispensing coin is not possible", "G05", "AxCX2_3.DispenseCoin",
							adminMsg: "Dispensing coin is not possible");
					}
				}
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "AxCX2_3.DispenseCoin");

				try
				{
					if (needToCheckCoinStatus)
					{
						bool isCheckStatusSuccess = CheckLowCoinStatus(out bool is10CentCoinLow, out bool is20CentCoinLow, out bool is50CentCoinLow);

						if (isCheckStatusSuccess)
						{
							_isRunningLowCoin10 = is10CentCoinLow;
							_isRunningLowCoin20 = is20CentCoinLow;
							_isRunningLowCoin50 = is50CentCoinLow;
						}
						else
						{
							LowCoinMan.IsReqToStartApp = true;

							Log.LogText(LogChannel, CurrentProcessId, $@"--Fail to read coin machine status--; Request to restart service application",
										"C22", "AxCX2_3.DispenseCoin", adminMsg: $@"--Fail to read coin machine status--; Request to restart service application");
						}
					}
				}
				catch { }

				throw ex;
			}
			finally
			{
				Log.LogText(LogChannel, CurrentProcessId, $@"Done", "A100", "AxCX2_3.DispenseCoin");
			}

			return isDispenseSuccess;
		}

		public string GetDispenseCode(int totalDispenseCents, out string notEnoughCoinMessage)
		{
			notEnoughCoinMessage = null;
			int intCoin50 = 0;
			int intCoin20 = 0;
			int intCoin10 = 0;

			string possibleDispenseCoins = "";

			if ((!_isRunningLowCoin10) || (LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin10Cent)))
			{
				if (((!_isRunningLowCoin50) || LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin50Cent))
					&& (totalDispenseCents > 0))
				{
					intCoin50 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 50M));

					if (intCoin50 > 9)
						intCoin50 = 9;

					totalDispenseCents -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}
				if (((!_isRunningLowCoin20) || LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin20Cent))
					&& (totalDispenseCents > 0))
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					totalDispenseCents -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
				if (totalDispenseCents > 0)
				{
					intCoin10 = Convert.ToInt32((decimal)totalDispenseCents / 10M);

					if (intCoin10 > 9)
						intCoin10 = 9;

					totalDispenseCents -= (intCoin10 * 10);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 10 cents: {intCoin10} pcs";
				}
			}

			// 10 cents and 30 cents cannot refund without 10 cents coin
			else if ((totalDispenseCents == 10) || (totalDispenseCents == 30))
				throw new Exception($@"Run out of 10 cents coin. Fail to refund {totalDispenseCents.ToString()} cents");

			// Refund 20 cents, 40 cents, 50 cents, 60 cents, 70 cents, 80 cents, and 90 cents.
			else if (
				((!_isRunningLowCoin50) && (!_isRunningLowCoin20))
				||
				((LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin50Cent)) && (LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin20Cent)))
				)
			{
				if (totalDispenseCents > 0)
				{
					if ((totalDispenseCents > 100) && ((totalDispenseCents % 20) != 0))
					{
						intCoin50 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 50M));

						if ((intCoin50 % 2) == 0)
							intCoin50 -= 1;
					}
					else
						intCoin50 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 50M));


					if ((intCoin50 > 9) && ((intCoin50 % 2) == 1))
						intCoin50 = 9;
					else if ((intCoin50 > 9) && ((intCoin50 % 2) == 0))
						intCoin50 = 8;

					int estRemainder = totalDispenseCents - (intCoin50 * 50);
					if (((estRemainder % 20) != 0) && (intCoin50 > 0))
						intCoin50--;


					totalDispenseCents -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}

				// Refund with one or two 20 cents coins
				if (totalDispenseCents > 0)
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					totalDispenseCents -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
			}
			else
			{
				// Refund only 50 cents.
				if (((!_isRunningLowCoin50) || (LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin50Cent)))
					&& (totalDispenseCents > 0))
				{
					intCoin50 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 50M));

					if ((intCoin50 > 9) && ((intCoin50 % 2) == 1))
						intCoin50 = 9;
					else if ((intCoin50 > 9) && ((intCoin50 % 2) == 0))
						intCoin50 = 8;

					totalDispenseCents -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}

				// Refund 20 cents, 40 cents, 60 cents, 80 cents.
				if (((!_isRunningLowCoin20) || (LowCoinMan.GetDispensePossibleByCoinCent(CoinCent.Coin20Cent)))
					&& (totalDispenseCents > 0))
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)totalDispenseCents / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					totalDispenseCents -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
			}

			if (possibleDispenseCoins.Length > 0)
				Log.LogText(LogChannel, CurrentProcessId, $@"Possible Dispense Coins .. {possibleDispenseCoins};", "A20", "AxCX2_3.GetDispenseCode");

			if (totalDispenseCents != 0)
				notEnoughCoinMessage = $@"Fail to dispense (in coin) for another {totalDispenseCents.ToString()} cents";

			string dispenseCode = $@"{intCoin10.ToString().Trim()}{intCoin20.ToString().Trim()}{intCoin50.ToString().Trim()}00000";

			Log.LogText(LogChannel, CurrentProcessId, 
				$@"Dispense Code : {dispenseCode}; 10 cents : {dispenseCode.Substring(0, 1)}pcs, 20 cents : {dispenseCode.Substring(1, 1)}pcs, 50 cents : {dispenseCode.Substring(2, 1)}pcs", 
				"A23", "AxCX2_3.GetDispenseCode");

			return dispenseCode;
		}

        public bool CheckDispensePossibility(out string lowCoinMsg, out string machOutOfSvcMsg,
            out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50)
        {
			lowCoinMsg = null;
            machOutOfSvcMsg = null;
            isRunningLowCoin10 = false;
            isRunningLowCoin20 = false;
            isRunningLowCoin50 = false;

			LowCoinMan.QueryLowCoin(out bool low10Cent, out bool low20Cent, out bool low50Cent);

			lowCoinMsg = LowCoinMan.GetStatusMessage();
			isRunningLowCoin10 = low10Cent;
			isRunningLowCoin20 = low20Cent;
			isRunningLowCoin50 = low50Cent;

			if (LowCoinMan.IsDispensePossible == false)
            {
                if (LowCoinMan.IsReqToStartApp)
                    machOutOfSvcMsg = "--Coin Dispensing Possibility Check: Request to restart service application--";
				else if (LowCoinMan.IsSpinMotorMalfunction)
					machOutOfSvcMsg = "--Coin Dispensing Possibility Check: Rebooting bin motor malfunction--";
				else
                    machOutOfSvcMsg = "--Coin Dispensing Possibility Check: Coin has reached out of service status--";
            }
            if ((lowCoinMsg is null) && (machOutOfSvcMsg != null))
                lowCoinMsg = machOutOfSvcMsg;

            return LowCoinMan.IsDispensePossible;
        }

		///// <summary>
		///// Check possibility of dispense coin
		///// </summary>
		///// <param name="totalDispenseCents"></param>
		///// <param name="lowCoinMsg">Low coin message</param>
		///// <param name="machOutOfSvcMsg">Machine out of service message</param>
		///// <returns></returns>
		//public bool CheckDispensePossibility(int totalDispenseCents, 
		//	out string lowCoinMsg, out string machOutOfSvcMsg,
		//	out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50)
		//{
		//	lowCoinMsg = null;
		//	machOutOfSvcMsg = null;
		//	isRunningLowCoin10 = false;
		//	isRunningLowCoin20 = false;
		//	isRunningLowCoin50 = false;
		//	//// long lRetVal2 = 0;
		//	decimal intTemp = 0;
		//	object lowCoinColStatus = null;
		//	///lRetVal2 = axTQ01001.GetLowCoinColumnStatus(ref LowCoinColStatus);
		//	///
		//	bool isCheckStatusSuccess = CheckLowCoinStatus(out bool is10CentCoinLow, out bool is20CentCoinLow, out bool is50CentCoinLow);
		//	if (isCheckStatusSuccess)
		//	{
		//		string lowCoinTypeStr = "";
		//		Object[] myarray = (Object[])lowCoinColStatus;
		//		for (var i = 0; i < 3; i++)
		//		{
		//			switch (i)
		//			{
		//				case 0:
		//					isRunningLowCoin10 = Convert.ToBoolean(myarray[i]);
		//					if (isRunningLowCoin10) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "10 cents low coin";
		//					break;
		//				case 1:
		//					isRunningLowCoin20 = Convert.ToBoolean(myarray[i]);
		//					if (isRunningLowCoin20) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "20 cents low coin";
		//					break;
		//				case 2:
		//					isRunningLowCoin50 = Convert.ToBoolean(myarray[i]);
		//					if (isRunningLowCoin50) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "50 cents low coin";
		//					break;
		//				default:
		//					lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + $@"Coin Type ({i}) Not Found";
		//					break;
		//			}
		//		}
		//		LowCoinManager.UpdateLowCoinStates(isRunningLowCoin10, isRunningLowCoin20, isRunningLowCoin50);
		//		lowCoinMsg = LowCoinManager.GetStatusMessage();
		//		if (lowCoinMsg?.Length > 0)
		//		{
		//			Log.LogText(LogChannel, CurrentProcessId, $@"{lowCoinMsg}", 
		//				"A12", "AxCX2_3.CheckDispensePossibility");
		//		}
		//	}
		//	else
		//	{
		//		lowCoinMsg = $@"--Error found in Coin Machine when reading coin status--";
		//		Log.LogText(LogChannel, CurrentProcessId, lowCoinMsg, 
		//			"A18", "AxCX2_3.CheckDispensePossibility");
		//		machOutOfSvcMsg = "--Unable to read coin machine status--; Request to restart service application--"
		//	}
		//	lowCoinColStatus = null;
		//	if (machOutOfSvcMsg is null)
		//  {
		//		if (LowCoinManager.IsDispensePossible == false)
		//		{
		//			if (LowCoinManager.IsReqToStartApp)
		//				machOutOfSvcMsg = "--Coin Dispensing Possibility Check: Request to restart service application--";
		//			else
		//				machOutOfSvcMsg = "--Coin Dispensing Possibility Check: Dispensing coin is not possible; Coin may too low--";
		//		}
		//		if ((lowCoinMsg is null) && (machOutOfSvcMsg != null))
		//			lowCoinMsg = machOutOfSvcMsg;
		//	}
		//	return ((machOutOfSvcMsg is null) && (lowCoinMsg is null));
		//}

		public bool CheckLowCoinStatus(out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50)
		{
			isRunningLowCoin10 = true;
			isRunningLowCoin20 = true;
			isRunningLowCoin50 = true;

			long lRetVal2 = 0;
			object LowCoinColStatus = null;

			for (int tryInx=0; tryInx < 3; tryInx++)
            {
				lRetVal2 = axTQ01001.GetLowCoinColumnStatus(ref LowCoinColStatus);
				WaitDelay(50);

				if (lRetVal2 == 1)
				{
					Object[] myarray = (Object[])LowCoinColStatus;
					for (var i = 0; i < 3; i++)
					{
						switch (i)
						{
							case 0:
								isRunningLowCoin10 = Convert.ToBoolean(myarray[i]);
								break;
							case 1:
								isRunningLowCoin20 = Convert.ToBoolean(myarray[i]);
								break;
							case 2:
								isRunningLowCoin50 = Convert.ToBoolean(myarray[i]);
								break;
							default:
								break;
						}
					}

					LowCoinMan.UpdateLowCoinStates(isRunningLowCoin10, isRunningLowCoin20, isRunningLowCoin50);

					return true;
				}
				else
				{
					Log.LogText(LogChannel, CurrentProcessId, $@"Error found in Coin Machine when reading coin status; Code = {lRetVal2.ToString()};",
						"EX01", "AxCX2_3.CheckLowCoinStatus", AppDecorator.Log.MessageType.Error);
				}
			}

			return false;
		}

        //private bool ResetCoinMachine()
        //{
        //	bool retDone = false;

        //	object pVarStatus = null;
        //	int retInt = -1;
        //	bool retRes = false;

        //	try
        //	{
        //		Log.LogText(LogChannel, CurrentProcessId, "Start Reset-Coin-Machine ..", "A01", "AxCX2_3.ResetCoinMachine");

        //		retRes = axTQ01001.ClearMachineStatus();
        //		Log.LogText(LogChannel, CurrentProcessId, $@"ClearMachineStatus return : {retRes.ToString()};", "A02", "AxCX2_3.ResetCoinMachine");
        //		Task.Delay(500).Wait();

        //		retRes = axTQ01001.ClearSensorStatus();
        //		Log.LogText(LogChannel, CurrentProcessId, $@"ClearSensorStatus return : {retRes.ToString()};", "A03", "AxCX2_3.ResetCoinMachine");
        //		Task.Delay(500).Wait();

        //		retInt = axTQ01001.ClearColumnHistory();
        //		Log.LogText(LogChannel, CurrentProcessId, $@"ClearColumnHistory return : {retInt.ToString()};", "A04", "AxCX2_3.ResetCoinMachine");
        //		Task.Delay(500).Wait();

        //		retInt = axTQ01001.ResetMachineStatus(ref pVarStatus);
        //		Log.LogText(LogChannel, CurrentProcessId, $@"ResetMachineStatus return : {retInt.ToString()}; pVarStatus : {JsonConvert.SerializeObject(pVarStatus)}", "A05", "AxCX2_3.ResetCoinMachine");
        //		Task.Delay(500).Wait();

        //		retInt = axTQ01001.RebootMachine();
        //		Log.LogText(LogChannel, CurrentProcessId, $@"RebootMachine return : {retInt.ToString()};", "A06", "AxCX2_3.ResetCoinMachine");
        //		Task.Delay(500).Wait();

        //		object tempObj = null;
        //		retInt = axTQ01001.EnableBuzzerSound(false, ref tempObj);
        //		Log.LogText(LogChannel, CurrentProcessId, $@"Reset Coin Machine --- DONE; Set EnableBuzzerSound to false; return : {retInt}; Out obj : {JsonConvert.SerializeObject(tempObj)}", "A20", "AxCX2_3.ResetCoinMachine");

        //		retDone = true;
        //	}
        //	catch (Exception ex)
        //	{
        //		retDone = false;
        //		Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "AxCX2_3.ResetCoinMachine");
        //	}
        //	finally
        //	{
        //		Log.LogText(LogChannel, CurrentProcessId, $@"Done", "A100", "AxCX2_3.ResetCoinMachine");
        //	}

        //	return retDone;
        //}

        public bool SpinDispenseByBin(CX2D3SpinIndex binIndex)
        {
            bool isDone = false;

            Log.LogText(LogChannel, CurrentProcessId, $@"Start Spin Coin Box (Inx : {binIndex}) ..", "A01", "AxCX2_3.SpinByBin");

            if (axTQ01001.SpinByBin(Convert.ToInt32(binIndex)))
            {
                isDone = true;
                Log.LogText(LogChannel, CurrentProcessId, $@"Spin Coin Box (Inx : {binIndex}) Completed", "A02", "AxCX2_3.SpinByBin");
            }
            else
            {
                Log.LogText(LogChannel, CurrentProcessId, $@"Spin Coin Box (Inx : {binIndex}) Error; Return : false", "A03", "AxCX2_3.SpinByBin");
            }

            return isDone;
        }

        public void CloseDevice()
		{
			try
			{
				axTQ01001.Dispose();
			}
			catch { }
		}

		private void WaitDelay(int millisec)
		{
			Thread tWorker = new Thread(new ThreadStart(new Action(() =>
			{
				Thread.Sleep(millisec);
			})))
			{
				IsBackground = true,
				Priority = ThreadPriority.Highest
			};

			this.Invoke(new Action(() =>
			{
				tWorker.Start();
				tWorker.Join();
			}));
		}

		/// <summary>
		/// This method used to reboot and recover coin machine when fail to dispense coin.
		/// </summary>
		public void RebootRecoverSequence(bool spinMotor = true)
		{
			int minTryUpdateMachineErrorsCount = 3;
			int maxTryUpdateMachineErrorsCount = 6;

			if (spinMotor)
            {
				if (SpinMachineMotor(-1, "RebootRecoverSequence") == false)
					Log.LogError(LogChannel, CurrentProcessId, new Exception("Fail to reboot coin machine"), "X03", "AxCX2_3.RebootRecoverSequence");
			}
			
			for (int upMCntInt = 0; upMCntInt < maxTryUpdateMachineErrorsCount; upMCntInt++)
			{
				UpdateMachineErrors(out bool errorFound, out string errorMsg);

				///// .. 3 sec deley is requested below for UpdateMachineErrors(..)
				WaitDelay(3000);

				if (errorFound)
					Log.LogText(LogChannel, CurrentProcessId, errorMsg, "A03", "AxCX2_3.RebootRecoverSequence", AppDecorator.Log.MessageType.Error);

				//else if (upMCntInt >= minTryUpdateMachineErrorsCount)
				//	break;

				else 
					break;
			}
		}

		private void MaintainCoinMachine()
		{
			if (LowCoinMan.IsRebootCoinMachineRequested == false)
				return;

			bool isMotorSpan = false;

			if (LowCoinMan.IsAllCoinEmpty == false)
			{
				int topRebootCount = 15;
				int maxRebootCount = 0;
				int rebootCounter = 0;

				LowCoinMan.QueryLowCoin(out bool isLow10Cent, out bool isLow20Cent, out bool isLow50Cent);

				int retInt = 0;
				int noOfLowCoin = 0;
				int singleLowCoinCent = 0;

				if (isLow10Cent)
				{
					noOfLowCoin++;
					singleLowCoinCent = 10;
				}

				if (isLow20Cent)
				{
					noOfLowCoin++;
					singleLowCoinCent = 20;
				}

				if (isLow50Cent)
				{
					noOfLowCoin++;
					singleLowCoinCent = 50;
				}

				rebootCounter = 0;
				if ((noOfLowCoin == 1) && (singleLowCoinCent > 0))
				{

					LowCoinMan.QueryCoinStatus(singleLowCoinCent, out bool isLow, out bool isEmpty, out bool isEnableToDispense);

					if (isEmpty == false)
					{
						int lowCoinCountX = 0;
						maxRebootCount = 3;

						for (int rebCount = 0; rebCount < maxRebootCount; rebCount++)
						{
							if (rebootCounter > topRebootCount)
								break;

							rebootCounter++;

							SpinMachineMotor(rebootCounter, "MaintainCoinMachine(A)");
							isMotorSpan = true;

							bool isCheckStatusSuccess = CheckLowCoinStatus(out bool is10CentCoinLow, out bool is20CentCoinLow, out bool is50CentCoinLow);

							if (isCheckStatusSuccess == false)
                            {
								LowCoinMan.IsReqToStartApp = true;
								Log.LogText(LogChannel, CurrentProcessId, $@"Fail to read coin machine status", "X04", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
									adminMsg: "Fail to read coin machine status");
								return;
                            }

							LowCoinMan.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

							if (maxRebootCount == 3)
							{
								lowCoinCountX = 0;
								if (is10CentCoinLow == true)
									lowCoinCountX++;
								if (is20CentCoinLow == true)
									lowCoinCountX++;
								if (is50CentCoinLow == true)
									lowCoinCountX++;

								if (lowCoinCountX > 1)
									maxRebootCount = 10;
							}


							if (isCheckStatusSuccess)
							{
								_isRunningLowCoin10 = is10CentCoinLow;
								_isRunningLowCoin20 = is20CentCoinLow;
								_isRunningLowCoin50 = is50CentCoinLow;
							}

							if (isCheckStatusSuccess)
							{
								LowCoinMan.UpdateEmptyCoinCounterAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

								if ((is10CentCoinLow == false) && (is20CentCoinLow == false) && (is50CentCoinLow == false))
								{
									break;
								}
								else if (singleLowCoinCent == 10)
								{
									LowCoinMan.QueryCoinStatus(10, out bool is10CentLow, out bool is10CentEmpty, out bool is10CentEnableToDispense);

									if (is10CentEmpty)
									{
										Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (A) - 10 Cent Coin Bin is empty. Not able to dispense coin. Coin machine blocked", 
											"X06", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
											adminMsg: $@"Reboot Coin Machine (A) - 10 Cent Coin Bin is empty. Not able to dispense coin. Coin machine blocked");

										break;
									}
									else if (is10CentEnableToDispense == false)
										rebCount = 1;
								}
							}
						}
						Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (A); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {LowCoinMan.IsDispensePossible}",
											"E01", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
											adminMsg: $@"Reboot Coin Machine (A); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {LowCoinMan.IsDispensePossible}");
					}
				}

				//else if ((noOfLowCoin > 1) 
				//	&& (_lowCoinManager.IsAllCoinEmpty == false) 
				//	&& ((_lowCoinManager.IsDispensePossible == false) || (_lowCoinManager.IsRequestCriticalRebootSpinMotor == true)))

				else if ((noOfLowCoin > 1)
					&& (LowCoinMan.IsRequestCriticalRebootSpinMotor == true))
				{
					int outStanding = 0;

					LowCoinMan.QueryCoinStatus(10, out bool is10CentLow, out bool is10CentEmpty, out bool is10CentEnableToDispense);

					if (is10CentEmpty == false)
					{
						LowCoinMan.QueryCoinStatus(20, out _, out bool is20CentEmpty, out _);
						LowCoinMan.QueryCoinStatus(50, out _, out bool is50CentEmpty, out _);

						if ((is20CentEmpty == false) || (is50CentEmpty == false))
						{
							maxRebootCount = 6;

							for (int rebCount = 0; rebCount < maxRebootCount; rebCount++)
							{
								if (rebootCounter > topRebootCount)
									break;

								rebootCounter++;

								SpinMachineMotor(rebootCounter, "MaintainCoinMachine(B)");
								isMotorSpan = true;

								bool isCheckStatusSuccess = CheckLowCoinStatus(out bool is10CentCoinLow, out bool is20CentCoinLow, out bool is50CentCoinLow);

								if (isCheckStatusSuccess == false)
								{
									LowCoinMan.IsReqToStartApp = true;
									Log.LogText(LogChannel, CurrentProcessId, $@"Fail to read coin machine status", "X14", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
										adminMsg: "Fail to read coin machine status");
									return;
								}

								LowCoinMan.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

								if (isCheckStatusSuccess)
								{
									_isRunningLowCoin10 = is10CentCoinLow;
									_isRunningLowCoin20 = is20CentCoinLow;
									_isRunningLowCoin50 = is50CentCoinLow;
								}

								if (isCheckStatusSuccess)
								{
									if ((is10CentCoinLow == false) && (is20CentCoinLow == false) && (is50CentCoinLow == false))
									{
										break;
									}
									else
									{
										LowCoinMan.UpdateEmptyCoinCounterAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

										outStanding = 0;
										LowCoinMan.QueryCoinStatus(10, out bool is10CentLowX2, out bool is10CentEmptyX2, out bool is10CentEnableToDispenseX2);
										LowCoinMan.QueryCoinStatus(20, out bool is20CentLowX2, out bool is20CentEmptyX2, out bool is20CentEnableToDispenseX2);
										LowCoinMan.QueryCoinStatus(50, out bool is50CentLowX2, out bool is50CentEmptyX2, out bool is50CentEnableToDispenseX2);

										if (is10CentLowX2)
											outStanding++;

										if (is20CentLowX2)
											outStanding++;

										if (is50CentLowX2)
											outStanding++;

										if (is10CentEmptyX2)
										{
											Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) - 10 Cent is empty. Not able to dispense coin. Coin machine Blocked",
												"X16", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
												adminMsg: $@"Reboot Coin Machine (B) - 10 Cent Coin Bin is empty. Not able to dispense coin. Coin machine blocked");

											break;
										}
										else if (is10CentEnableToDispenseX2 == false)
										{
											rebCount = 1;
										}
										else if (is20CentEmptyX2 && is50CentEmptyX2)
										{
											Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) - 20 Cent & 50 Cent are empty.",
												"X17", "AxCX2_3.MaintainCoinMachine", AppDecorator.Log.MessageType.Error,
												adminMsg: $@"Reboot Coin Machine (B) - 20 Cent & 50 Cent are empty.");

											break;
										}
										else if (outStanding > 1)
										{
											rebCount = 1;
										}
										else if (is20CentEmptyX2 && (is50CentLowX2 == false))
										{
											Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) - 20 Cent is Empty; 50 Cent Bin has recovered.",
												"K01", "AxCX2_3.MaintainCoinMachine", 
												adminMsg: $@"Reboot Coin Machine (B) - 20 Cent is Empty; 50 Cent Bin has recovered.");

											break;
										}
										else if (is50CentEmptyX2 && (is20CentLowX2 == false))
										{
											Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) - 50 Cent is Empty; 20 Cent Bin has recovered.",
												"K02", "AxCX2_3.MaintainCoinMachine",
												adminMsg: $@"Reboot Coin Machine (B) - 50 Cent is Empty; 20 Cent Bin has recovered.");
											break;
										}
									}
								}
							}
						}

						Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {LowCoinMan.IsDispensePossible}",
											"K20", "AxCX2_3.MaintainCoinMachine",
											adminMsg: $@"Reboot Coin Machine (B); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {LowCoinMan.IsDispensePossible}");
					}
				}

				if (isMotorSpan)
                {
					// Note : Clear rubbish message from coin machine. Else coin machine status reading may not accurate.
					
					bool retRes;
					object pVarStatus = null;

					retRes = axTQ01001.ClearMachineStatus();
                    Log.LogText(LogChannel, CurrentProcessId, $@"ClearMachineStatus return : {retRes.ToString()};", "J02", "AxCX2_3.MaintainCoinMachine");
					WaitDelay(300);

					retRes = axTQ01001.ClearSensorStatus();
                    Log.LogText(LogChannel, CurrentProcessId, $@"ClearSensorStatus return : {retRes.ToString()};", "J03", "AxCX2_3.MaintainCoinMachine");
					WaitDelay(300);

					retInt = axTQ01001.ClearColumnHistory();
                    Log.LogText(LogChannel, CurrentProcessId, $@"ClearColumnHistory return : {retInt.ToString()};", "J04", "AxCX2_3.MaintainCoinMachine");
					WaitDelay(300);

					retInt = axTQ01001.ResetMachineStatus(ref pVarStatus);
                    Log.LogText(LogChannel, CurrentProcessId, $@"ResetMachineStatus return : {retInt.ToString()}; pVarStatus : {JsonConvert.SerializeObject(pVarStatus)}", "J05", "AxCX2_3.MaintainCoinMachine");
					WaitDelay(300);

					RebootRecoverSequence(false);
				}
			}
		}

		/// <summary>
		/// Spin motor for 1 times when any of coin bin is low status
		/// </summary>
		/// <param name="spinMotorCount"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public bool SpinMachineMotor(int spinMotorCount, string tag = "")
        {
			if (string.IsNullOrWhiteSpace(tag))
				tag = "";

			int retInt = 0;
			Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) Loop Count : {spinMotorCount}", "A03", $@"AxCX2_3.SpinMachineMotor->{tag}");
			retInt = axTQ01001.RebootMachine();
			Log.LogText(LogChannel, CurrentProcessId, $@"Reboot Coin Machine (B) return : {retInt.ToString()};", "A04", $@"AxCX2_3.SpinMachineMotor->{tag}");
			WaitDelay(4300);  // Valid to apply - Best Solution

			if (retInt != 0)
			{
				LowCoinMan.UpdateSpinMotorState(false);
				return false;
			}
			else
			{
				LowCoinMan.UpdateSpinMotorState(true);
				return true;
			}
		}

		private void UpdateMachineErrors(out bool errorFound, out string errorMsg)
		{
			bool errorFoundX = false;
			string errorMsgX = null;

			this.Invoke(new Action(() =>
			{
				try
				{
					int lRetVal = 0;
					object refObj = null;

					refObj = null;
					lRetVal = 0;
					lRetVal = axTQ01001.GetMachineErrors(ref refObj);
					if (lRetVal == 1)
					{
						string StrTemp = "0000000" + Convert.ToString(Convert.ToByte(refObj), 2);
						StrTemp = StrTemp.Substring(StrTemp.Length - 7, 7);
						string StrValue = "";

						for (var i = 4; i <= 6; i++)
						{
							StrValue = StrTemp.Substring(i, 1);
							switch (i)
							{
								case 4:
									if (StrValue == "0")
									{}
									else
									{
										UpdateError("--Coin Eject Timeout Error--");
									}
									break;
								case 5:
									if (StrValue == "0")
									{}
									else
									{
										UpdateError("--Coin Sensor Failed Error--");
									}
									break;
								case 6:
									if (StrValue == "0")
									{}
									else
									{
										UpdateError("--Config Mismatch Error--");
									}
									break;
							}
						}
					}
					else
					{
						UpdateError("--Last Transaction Error (Coin Mach.B)--; " + axTQ01001.GetTransactLastError());
					}
				}
				catch (Exception ex)
				{
					UpdateError("--Last Transaction Error (Coin Mach.C)--; " + ex.Message);
				}
			}));

			errorFound = errorFoundX;
			errorMsg = errorMsgX;

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void UpdateError(string newErrorMsg)
			{
				if (string.IsNullOrWhiteSpace(newErrorMsg))
					return;

				errorFoundX = true;

				if (errorMsgX is null)
					errorMsgX = newErrorMsg;
				else
					errorMsgX += "; " + newErrorMsg;

			}
		}
	}
}