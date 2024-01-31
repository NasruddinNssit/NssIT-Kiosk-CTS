using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormCoinTest.LowCoin;

namespace WinFormCoinTest
{
	public partial class Form1 : Form
	{
		private Boolean _isRunningLowCoin10 = false;
		private Boolean _isRunningLowCoin20 = false;
		private Boolean _isRunningLowCoin50 = false;

		private LowCoinManager _lowCoinManager = new LowCoinManager();

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{ }

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (_disposed == false)
					axTQ01001.Dispose();
				_disposed = true;
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void ShowMsg(string msg)
		{
			msg = msg ?? "--";
			txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {msg}{"\r\n"}");
			//txtMsg.Refresh();
			Application.DoEvents();
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			int lRetVal = -1;
			string strOcxVersion = "";
			strOcxVersion = "";

			try
			{
				ShowMsg($@"========== ========== Start - Coin Machine ========== ==========");

				axTQ01001.CommunicationChannel = false;

				lRetVal = axTQ01001.SetProtocol(2);
				ShowMsg($@"SetProtocol return : {lRetVal};");

				lRetVal = axTQ01001.InitPortSettings(ref strOcxVersion);
				ShowMsg($@"InitPortSettings return : {lRetVal}; strOcxVersion = ""{strOcxVersion}""");

				// When Coin Machine Detected
				if (lRetVal == 1)
				{
					ShowMsg($@"ClearMachineStatus .. ");
					axTQ01001.ClearMachineStatus();

					ShowMsg($@"ClearSensorStatus .. ");
					axTQ01001.ClearSensorStatus();

					lRetVal = -1;
					object tempObj = null;
					lRetVal = axTQ01001.EnableBuzzerSound(false, ref tempObj);
					ShowMsg($@"Set EnableBuzzerSound to false; return : {lRetVal}; Out obj : {JsonConvert.SerializeObject(tempObj)}");

					if (InitialMachStatus() == true)
					{
						ShowMsg($@"Initial checking coin machine status - Done");

						string lowInitMsg = _lowCoinManager.GetStatusMessage();

						if (lowInitMsg != null)
							ShowMsg(lowInitMsg);
					}
					else
                    {
						ShowMsg($@"!!!!! Unable to read machine status when start coin machine!!!!!");
					}
				}
				else
                {
					ShowMsg($@"!!!!! Fail to init. port when start coin machine!!!!!");
				}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			
			bool InitialMachStatus()
            {
				long lRetVal2 = 0;
				_isRunningLowCoin10 = false;
				_isRunningLowCoin20 = false;
				_isRunningLowCoin50 = false;
				decimal intTemp = 0;
				object LowCoinColStatus = null;

				lRetVal2 = axTQ01001.GetLowCoinColumnStatus(ref LowCoinColStatus);

				if (lRetVal2 == 1)
				{
					string lowCoinTypeStr = "";
					Object[] myarray = (Object[])LowCoinColStatus;
					for (var i = 0; i < 3; i++)
					{
						switch (i)
						{
							case 0:
								_isRunningLowCoin10 = Convert.ToBoolean(myarray[i]);
								if (_isRunningLowCoin10) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "10 cents";
								break;
							case 1:
								_isRunningLowCoin20 = Convert.ToBoolean(myarray[i]);
								if (_isRunningLowCoin20) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "20 cents";
								break;
							case 2:
								_isRunningLowCoin50 = Convert.ToBoolean(myarray[i]);
								if (_isRunningLowCoin50) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "50 cents";
								break;
							default:
								ShowMsg("Coin Type Not Found");
								break;
						}
					}

					_lowCoinManager.UpdateLowCoinStatesOnInit(_isRunningLowCoin10, _isRunningLowCoin20, _isRunningLowCoin50);

					return true;
				}
				else
					return false;
			}
		}

		private void btnDispense01_Click(object sender, EventArgs e)
		{
			TryDispense(txtCoinAmount.Text);

			//for (decimal val = 8.00M; val >= 0.00M; val = (val - 0.10M))
			//{
			//	string rfVal = string.Format("{0:#,##0.00}", val);
			//	ShowMsg("Check dispense coin Value : " + rfVal);
			//	TryDispense(rfVal);
			//}
		}

		private void OnDispenseCents_Click(object sender, EventArgs e)
		{
			Button currButton = (Button)sender;
			ShowMsg($@"Button Tag is {(string)currButton.Tag}");

			ShowMsg($@"========== ========== Start - Dispense ========== ==========");
			TryDispense((string)currButton.Tag);
			ShowMsg($@"-----Done-----{"\r\n"}");
		}

		private void OnBasicDispenseCents_Click(object sender, EventArgs e)
		{
			Button currButton = (Button)sender;

			ShowMsg($@"Basic Dispense - Button Tag is {(string)currButton.Tag}");
			TryBasicDispense((string)currButton.Tag, Convert.ToInt32(txtNoOfCoin.Value));
		}

		private void TryDispense(string priceStr)
		{
			decimal amount = 0.00M;
			decimal totalCent = 0.00M;
			bool flg = true;
			bool flgCoin = false;
			bool needToCheckCoinStatus = false;
						
			try
			{
				if (_lowCoinManager.IsReqToStartApp)
					throw new Exception("Request to Restart Application");

				if (decimal.TryParse(priceStr, out amount) && (amount > 0))
				{
					totalCent = Math.Floor(amount * 100M);

					ShowMsg($@"Dispense Total : {totalCent.ToString()} cents");

					//flgCoin = CheckRefundPossibility(totalCent, flg);

					string lowMsg = _lowCoinManager.GetStatusMessage();

					if (lowMsg != null)
						ShowMsg(lowMsg);

					//ShowMsg($@"Possibility of coin refund : {(flgCoin ? "Yes" : "No")}");

					if (_lowCoinManager.IsDispensePossible)
					//if (flgCoin)
					{
						string notEnoughCoinMessage = null;

						string dispenseCode = GetCoinDispenseCode(Convert.ToInt32(totalCent), out notEnoughCoinMessage);

						if (notEnoughCoinMessage != null)
                        {
							ShowMsg($@"!!!!! Not Enough Coin : {notEnoughCoinMessage} !!!!!");
						}

						//if (notEnoughCoinMessage == null)
						//{
						ShowMsg($@"Coin Dispense Code : {dispenseCode}; ** 10 cents : {dispenseCode.Substring(0, 1)} pcs ; ** 20 cents : {dispenseCode.Substring(1, 1)} pcs ; ** 50 cents : {dispenseCode.Substring(2, 1)} pcs ;");

						flgCoin = false;

						int maxTryDispenseCount = 10;

						for (int dpInx = 0; dpInx < maxTryDispenseCount; dpInx++)
                        {
							try
							{
								if (dpInx > 0)
									ShowMsg("Retry coin dispense .. .. .. .. ..");

								needToCheckCoinStatus = true;

								flgCoin = axTQ01001.DispenseByColumn(dispenseCode);
								if (flgCoin)
								{
									_lowCoinManager.UpdateDispenseStatus(dispenseCode);
									ShowMsg($@"Dispense coin command sent successfully.");

									WaitDelay(4300); // Best solution before check status with ShowBinStatus();
									break;
								}
								else
								{
									WaitDelay(500);
									if ((dpInx + 1) < maxTryDispenseCount)
                                    {
										ShowMsg($@"Fail dispense coin status; Application will retry to dispense again..");
										RebootRecoverSequence();

										WaitDelay(4300); // Best solution before check status with ShowBinStatus();
									}
									else
                                    {
										ShowMsg($@"-Retry coin dispense has failed; End Try--; Dispense Code: {dispenseCode}; Retry Dispense Loop Count : {dpInx}");
									}
								}
							}
							catch (Exception ex)
							{
								ShowMsg(ex.ToString());
								WaitDelay(4300); // Best solution before check status with ShowBinStatus();
							}
						}

						if (flgCoin == false)
                        {
							_lowCoinManager.IsReqToStartApp = true;
                        }

						_proceedLoopTest = flgCoin;

						_isRunningLowCoin10 = false;
						_isRunningLowCoin20 = false;
						_isRunningLowCoin50 = false;

						ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
                   		_lowCoinManager.UpdateLowStateAfterCoinJustDispensed(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);
                    
						needToCheckCoinStatus = false;

						if (isCheckStatusSuccess)
						{
							_isRunningLowCoin10 = is10CentCoinLow;
							_isRunningLowCoin20 = is20CentCoinLow;
							_isRunningLowCoin50 = is50CentCoinLow;
						}

						//}
						//else
						//	ShowMsg(notEnoughCoinMessage);

						MaintainCoinMachine();

						//if (isCheckStatusSuccess)
      //                  {
						//	_lowCoinManager.UpdateEmptyCoinCounter(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);
						//}

						string lowMsgX = _lowCoinManager.GetStatusMessage();
						if (lowMsgX == null)
							ShowMsg($@"Coin machine still available");
						else
							ShowMsg(lowMsgX);

						if (_lowCoinManager.IsDispensePossible == false)
							ShowMsg($@"Coin machine has been blocked");

					}
					else
					{
						_proceedLoopTest = false;
						ShowMsg($@"Unable to dispense coin.");
					}
				}
				else if (amount < 0.10M)
					throw new Exception($@"Coin dispense amount must more then 0.10; App. try to dispense amount : {priceStr}");
				else
					throw new Exception($@"Invalid Coin amount specification; Try to dispense amount : {priceStr}");
			}
			catch (Exception ex)
			{
				try
				{
					if (needToCheckCoinStatus)
					{
						ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
						if (isCheckStatusSuccess)
						{
							_isRunningLowCoin10 = is10CentCoinLow;
							_isRunningLowCoin20 = is20CentCoinLow;
							_isRunningLowCoin50 = is50CentCoinLow;
						}
					}
				}
				catch { }

				ShowMsg(ex.ToString());
			}
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

		private void TryBasicDispense(string priceStr, int noOfCoin)
		{
			decimal amount = 0.00M;
			decimal totalCent = 0.00M;
			bool flg = true;
			bool flgCoin = false;

			try
			{
				ShowMsg($@"========== ========== Start - Dispense ========== ==========");

				if (decimal.TryParse(priceStr, out amount) && (amount > 0))
				{
					totalCent = Math.Floor(amount * 100M);

					ShowMsg($@"Dispense Total : {totalCent.ToString()} cents");

					string dispenseCode = GetBasicCoinDispenseCode(Convert.ToInt32(totalCent), noOfCoin);

					ShowMsg($@"Coin Dispense Code : {dispenseCode}; ** 10 cents : {dispenseCode.Substring(0, 1)} pcs ; ** 20 cents : {dispenseCode.Substring(1, 1)} pcs ; ** 50 cents : {dispenseCode.Substring(2, 1)} pcs ;");

					flgCoin = false;
					flgCoin = axTQ01001.DispenseByColumn(dispenseCode);
					if (flgCoin)
					{
						ShowMsg($@"Dispense coin command sent successfully.");

						WaitDelay(300);
					}
					else
						ShowMsg($@"Dispense coin completed with fail state.");

					ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
					if (isCheckStatusSuccess)
					{
						_isRunningLowCoin10 = is10CentCoinLow;
						_isRunningLowCoin20 = is20CentCoinLow;
						_isRunningLowCoin50 = is50CentCoinLow;
					}

				}
				else if (amount < 0.10M)
					throw new Exception($@"Coin dispense amount must more then 0.10; App. try to dispense amount : {priceStr}");
				else
					throw new Exception($@"Invalid Coin amount specification; Try to dispense amount : {priceStr}");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnResetCoinMachine_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Reset-Coin-Machine ========== ==========");

				retRes = axTQ01001.ClearMachineStatus();
				ShowMsg($@"ClearMachineStatus return : {retRes.ToString()};");
				Task.Delay(500).Wait();

				retRes = axTQ01001.ClearSensorStatus();
				ShowMsg($@"ClearSensorStatus return : {retRes.ToString()};");
				Task.Delay(500).Wait();

				retInt = axTQ01001.ClearColumnHistory();
				ShowMsg($@"ClearColumnHistory return : {retInt.ToString()};");
				Task.Delay(500).Wait();

				retInt = axTQ01001.ResetMachineStatus(ref pVarStatus);
				ShowMsg($@"ResetMachineStatus return : {retInt.ToString()}; pVarStatus : {JsonConvert.SerializeObject(pVarStatus)}");
				Task.Delay(500).Wait();

				retInt = axTQ01001.RebootMachine();
				ShowMsg($@"RebootMachine return : {retInt.ToString()};");
				WaitDelay(4300);

				object tempObj = null;
				retInt = axTQ01001.EnableBuzzerSound(false, ref tempObj);
				ShowMsg($@"Set EnableBuzzerSound to false; return : {retInt}; Out obj : {JsonConvert.SerializeObject(tempObj)}");

				ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
				_lowCoinManager.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnClearMachine_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Clear Machine ========== ==========");

				retRes = axTQ01001.ClearMachineStatus();
				ShowMsg($@"ClearMachineStatus return : {retRes.ToString()};");
				Task.Delay(500).Wait();
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnClearSensor_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Clear Sensor ========== ==========");

				retRes = axTQ01001.ClearSensorStatus();
				ShowMsg($@"ClearSensorStatus return : {retRes.ToString()};");
				Task.Delay(500).Wait();
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnClearColumnHis_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Clear Column History ========== ==========");

				retInt = axTQ01001.ClearColumnHistory();
				ShowMsg($@"ClearColumnHistory return : {retInt.ToString()};");
				Task.Delay(500).Wait();
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnResetMachine_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Reset Machine Status ========== ==========");

				retInt = axTQ01001.ResetMachineStatus(ref pVarStatus);
				ShowMsg($@"ResetMachineStatus return : {retInt.ToString()}; pVarStatus : {JsonConvert.SerializeObject(pVarStatus)}");
				Task.Delay(500).Wait();
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnRebootMachine_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Reboot Machine ========== ==========");

				retInt = axTQ01001.RebootMachine();
				ShowMsg($@"RebootMachine return : {retInt.ToString()};");
				WaitDelay(4300);

				ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
				_lowCoinManager.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnSetBuzzer_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			try
			{
				ShowMsg($@"========== ========== Start - Set Buzzer Sound ========== ==========");

				object tempObj = null;
				
				if (rbtBuzzerOn.Checked)
					retInt = axTQ01001.EnableBuzzerSound(true, ref tempObj);
				else
					retInt = axTQ01001.EnableBuzzerSound(false, ref tempObj);

				ShowMsg($@"Set Buzzer Sound to {rbtBuzzerOn.Checked==true}; return : {retInt}; Out obj : {JsonConvert.SerializeObject(tempObj)}");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnSpinCoinBox1_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Spin Coin Box1 (10 cent) ========== ==========");

				if (axTQ01001.SpinByBin(1))
				{ }
				else
					ShowMsg($@"Spin Coin Box1 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnSpinCoinBox2_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Spin Coin Box2 (20 cent) ========== ==========");

				if (axTQ01001.SpinByBin(2))
				{ }
				else
					ShowMsg($@"Spin Coin Box2 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnSpinCoinBox3_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Spin Coin Box3 (50 cent) ========== ==========");

				if (axTQ01001.SpinByBin(3))
				{ }
				else
					ShowMsg($@"Spin Coin Box3 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnGetLastDispenseHistory_Click(object sender, EventArgs e)
		{
			object refObj = null;
			int lRetVal = -1;

			int flg10 = 0;
			int flg20 = 0;
			int flg50 = 0;

			try
			{
				ShowMsg($@"========== ========== Start - Get Accumulated Dispensed History ========== ==========");
				ShowMsg("..... history is refer to last switch on time;");

				lRetVal = axTQ01001.GetColumnHistory(ref refObj);

				if (lRetVal == 1)
				{
					Boolean Errflg = false;
					Object[] myarray = (Object[])refObj;
					for (var i = 0; i < 3; i++)
					{
						switch (i)
						{
							case 0:
								flg10 = Convert.ToInt32(myarray[i]);
								break;
							case 1:
								flg20 = Convert.ToInt32(myarray[i]);
								break;
							case 2:
								flg50 = Convert.ToInt32(myarray[i]);
								break;
							default:
								ShowMsg($@"Coin Type Not Found for ({i.ToString()})");
								Errflg = true;
								break;
						}
					}
					if (!Errflg)
					{
						ShowMsg($@"{"\t\t"} 10 cents :: {flg10.ToString()};{"\t"} 20 cents :: {flg20.ToString()};{"\t"} 50 cents :: {flg50.ToString()};");
					}
				}
				else
				{
					ShowMsg($@"axTQ01001.GetTransactLastError return : {axTQ01001.GetTransactLastError()}");
				}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void btnGetMachineStatus_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Get Machine Status ========== ==========");
				UpdateMachineStatus();
			}
			catch (Exception ex)
			{

				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private string GetCoinDispenseCode(int dblTempCoinCent, out string notEnoughCoinMessage)
		{
			notEnoughCoinMessage = null;

			int intCoin50 = 0;
			int intCoin20 = 0;
			int intCoin10 = 0;

			string possibleDispenseCoins = "";

			if ((!_isRunningLowCoin10) || (_lowCoinManager.GetDispensePossibleByCoinCent(10)))
			{
				if (((!_isRunningLowCoin50) || (_lowCoinManager.GetDispensePossibleByCoinCent(50)))
					&& (dblTempCoinCent > 0))
				{
					intCoin50 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 50M));

					if (intCoin50 > 9)
						intCoin50 = 9;

					dblTempCoinCent -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}
				if (((!_isRunningLowCoin20) || (_lowCoinManager.GetDispensePossibleByCoinCent(20)))
					&& (dblTempCoinCent > 0))
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					dblTempCoinCent -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
				if (dblTempCoinCent > 0)
				{
					intCoin10 = Convert.ToInt32((decimal)dblTempCoinCent / 10M);

					if (intCoin10 > 9)
						intCoin10 = 9;

					dblTempCoinCent -= (intCoin10 * 10);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 10 cents: {intCoin10} pcs";
				}
			}

			// 10 cents and 30 cents cannot refund without 10 cents coin
			else if ((dblTempCoinCent == 10) || (dblTempCoinCent == 30))
				throw new Exception($@"Run out of 10 cents coin. Fail to refund {dblTempCoinCent.ToString()} cents");

			// Refund 20 cents, 40 cents, 50 cents, 60 cents, 70 cents, 80 cents, and 90 cents.
			else if (
				((!_isRunningLowCoin50) && (!_isRunningLowCoin20))
				||
				((_lowCoinManager.GetDispensePossibleByCoinCent(50)) && (_lowCoinManager.GetDispensePossibleByCoinCent(20)))
				)
			{
				if (dblTempCoinCent > 0)
				{
					if ((dblTempCoinCent > 100) && ((dblTempCoinCent % 20) != 0))
					{
						intCoin50 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 50M));

						if ((intCoin50 % 2) == 0)
							intCoin50 -= 1;
					}
					else
						intCoin50 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 50M));


					if ((intCoin50 > 9) && ((intCoin50 % 2) == 1))
						intCoin50 = 9;
					else if ((intCoin50 > 9) && ((intCoin50 % 2) == 0))
						intCoin50 = 8;

					int estRemainder = dblTempCoinCent - (intCoin50 * 50);
					if (((estRemainder % 20) != 0) && (intCoin50 > 0))
						intCoin50--;
					

					dblTempCoinCent -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}

				// Refund with one or two 20 cents coins
				if (dblTempCoinCent > 0)
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					dblTempCoinCent -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
			}
			else
			{
				// Refund only 50 cents.
				if (((!_isRunningLowCoin50) || (_lowCoinManager.GetDispensePossibleByCoinCent(50)))
					&& (dblTempCoinCent > 0))
				{
					intCoin50 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 50M));

					if ((intCoin50 > 9) && ((intCoin50 % 2) == 1))
						intCoin50 = 9;
					else if ((intCoin50 > 9) && ((intCoin50 % 2) == 0))
						intCoin50 = 8;

					dblTempCoinCent -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}

				// Refund 20 cents, 40 cents, 60 cents, 80 cents.
				if (((!_isRunningLowCoin20) || (_lowCoinManager.GetDispensePossibleByCoinCent(20)))
					&& (dblTempCoinCent > 0))
				{
					intCoin20 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 20M));

					if (intCoin20 > 9)
						intCoin20 = 9;

					dblTempCoinCent -= (intCoin20 * 20);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 20 cents: {intCoin20} pcs";
				}
			}

			if (possibleDispenseCoins.Length > 0)
				ShowMsg($@"Possible Dispense Coins .. {possibleDispenseCoins};");

			if (dblTempCoinCent != 0)
				notEnoughCoinMessage = $@"Fail to dispense (in coin) for another {dblTempCoinCent.ToString()} cents";

			string retVal = $@"{intCoin10.ToString().Trim()}{intCoin20.ToString().Trim()}{intCoin50.ToString().Trim()}00000";

			return retVal;
		}

		private string GetBasicCoinDispenseCode(int dblCoinCent, int noOfCoin )
		{
			int intCoin50 = 0;
			int intCoin20 = 0;
			int intCoin10 = 0;

			string possibleDispenseCoins = "";
			string retVal = "";

			if ((noOfCoin < 0) || (noOfCoin > 9))
            {
				throw new Exception($@"Invalid Number of Coin({noOfCoin.ToString().Trim()})");
            }

			if (dblCoinCent == 50)
            {
				retVal = $@"00{noOfCoin.ToString().Trim()}00000";
			}
			else if (dblCoinCent == 20)
			{
				retVal = $@"0{noOfCoin.ToString().Trim()}000000";
			}
			else if (dblCoinCent == 10)
			{
				retVal = $@"{noOfCoin.ToString().Trim()}0000000";
			}
			else
            {
				throw new Exception($@"Invalid Cent Coin({dblCoinCent.ToString().Trim()})");
			}

			return retVal;
		}

		public Boolean CheckRefundPossibility(decimal dblTempCoinBalc, Boolean flg)
		{
			long lRetVal2 = 0;
			_isRunningLowCoin10 = false;
			_isRunningLowCoin20 = false;
			_isRunningLowCoin50 = false;
			decimal intTemp = 0;
			object LowCoinColStatus = null;

			lRetVal2 = axTQ01001.GetLowCoinColumnStatus(ref LowCoinColStatus);

			if (lRetVal2 == 1)
			{
				string lowCoinTypeStr = "";
				Object[] myarray = (Object[])LowCoinColStatus;
				for (var i = 0; i < 3; i++)
				{
					switch (i)
					{
						case 0:
							_isRunningLowCoin10 = Convert.ToBoolean(myarray[i]);
							if (_isRunningLowCoin10) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "10 cents";
							break;
						case 1:
							_isRunningLowCoin20 = Convert.ToBoolean(myarray[i]);
							if (_isRunningLowCoin20) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "20 cents";
							break;
						case 2:
							_isRunningLowCoin50 = Convert.ToBoolean(myarray[i]);
							if (_isRunningLowCoin50) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "50 cents";
							break;
						default:
							ShowMsg("Coin Type Not Found");
							flg = false;
							break;
					}
				}

				_lowCoinManager.UpdateLowCoinStates(_isRunningLowCoin10, _isRunningLowCoin20, _isRunningLowCoin50);

				if (lowCoinTypeStr.Length > 0)
					ShowMsg($@"Low Coin Status Triggered for {lowCoinTypeStr}");
				
				if ((!_isRunningLowCoin10) && (!_isRunningLowCoin20) && (!_isRunningLowCoin50))
				{
					ShowMsg("Normal Coin Status for 10 cents, 20 cents, and 50 cents");
				}
				else if (_isRunningLowCoin10 && _isRunningLowCoin20 && _isRunningLowCoin50)
				{
					ShowMsg($@"Low Coin Status for 10 cents, 20 cents, and 50 cents; Unable to dispense total of {dblTempCoinBalc} cents");
					flg = false;
				}
				else if (_isRunningLowCoin10 && _isRunningLowCoin20)
				{
					intTemp = dblTempCoinBalc - (Convert.ToInt32(Math.Floor(dblTempCoinBalc / 50M)) * 50M);
					if (intTemp > 0)
					{
						ShowMsg($@"Low Coin Status for 10 cents and 20 cents; Unable to dispense total of {dblTempCoinBalc} cents");
						flg = false;
					}
				}
				else if (_isRunningLowCoin10 && _isRunningLowCoin50)
				{
					intTemp = dblTempCoinBalc - (Convert.ToInt32(Math.Floor(dblTempCoinBalc / 20M)) * 20M);
					if (intTemp > 0)
					{
						ShowMsg($@"Low Coin Status for 10 cents, 50 cents; Unable to dispense total of {dblTempCoinBalc} cents");
						flg = false;
					}
				}
				else if (_isRunningLowCoin10)
				{
					// With 20 cents or 50 cents coins IS NOT able to refund 10 cents, 30 cents.
					if ((dblTempCoinBalc == 30) || (dblTempCoinBalc == 10))
					{
						ShowMsg($@"Low Coin Status for 10 cents; Unable to dispense total of {dblTempCoinBalc} cents");
						flg = false;
					}
					else
					{
						// .. by Pass Statement.
						//
						// Note : With 50 cents coins and 20 sents coins available, machine is able to 
						//        refund coin amount for 20 cents, 40 cents, 50 cents, 60 cents, 70 cents, 80 cents, or 90 cents.
					}
				}
			}
			else
			{
				ShowMsg($@"Error Return from Coin Machine when Get Low Coin Column Status; lRetVal2 = {lRetVal2.ToString()};");
				flg = false;
				ShowMsg($@"axTQ01001.GetTransactLastError return : {axTQ01001.GetTransactLastError()}");
			}

			LowCoinColStatus = null;
			// return flg;
			return _lowCoinManager.IsDispensePossible;
		}

		public void UpdateMachineStatus()
		{
			try
			{
				object test = null;
				object refObj = null;
				int lRetVal = -1;

				//btnResetCoinMachine_Click(null, null);

				axTQ01001.ClearSensorStatus();
				axTQ01001.ClearMachineStatus();
				axTQ01001.ResetMachineStatus(ref test);
				axTQ01001.Refresh();

				refObj = null;
				lRetVal = -1;
				lRetVal = axTQ01001.GetMachineStatus(ref refObj);

				if (lRetVal == 1)
				{
					string StrTemp = Convert.ToString(Convert.ToByte(refObj), 2);
					string StrValue = "";
					for (var i = 1; i <= 6; i++)
					{
						StrValue = StrTemp.Substring(i, 1);
						switch (i)
						{
							case 1:
								if (StrValue == "0")
								{
									lblM1.BackColor = Color.LightGreen;
								}
								else
								{
									lblM1.BackColor = Color.Red;
								}
								break;
							case 2:
								if (StrValue == "0")
								{
									lblM2.BackColor = Color.LightGreen;
								}
								else
								{
									lblM2.BackColor = Color.Red;
								}
								break;
							case 3:
								if (StrValue == "0")
								{
									lblM3.BackColor = Color.LightGreen;
								}
								else
								{
									lblM3.BackColor = Color.Red;
								}
								break;
							case 4:
								if (StrValue == "0")
								{
									lblM4.BackColor = Color.LightGreen;
								}
								else
								{
									lblM4.BackColor = Color.Red;
								}
								break;
							case 5:
								if (StrValue == "0")
								{
									lblM5.BackColor = Color.LightGreen;
								}
								else
								{
									lblM5.BackColor = Color.Red;
								}
								break;
							case 6:
								if (StrValue == "0")
								{
									lblM6.BackColor = Color.LightGreen;
								}
								else
								{
									lblM6.BackColor = Color.Red;
								}
								break;
						}
					}
					//SetCoinLevel();
				}
				else
				{
					ShowMsg($@"axTQ01001.GetTransactLastError return : {axTQ01001.GetTransactLastError()}");
				}
			}
			catch (Exception ex)
			{
				ShowMsg("Unexpected Error (UpdateMachineStatus) : " + ex.Message);
			}
		}

		private void btnGetLowBinStatus_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Get Low Bin Status ========== ==========");

				/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				object pVarStatus = null;
				int retInt = -1;
				bool retRes = false;

				if (chkClearMachine.Checked)
				{
					ShowCurrentReadStatusState(chkClearMachine);
					retRes = axTQ01001.ClearMachineStatus();
					ShowMsg($@"ClearMachineStatus return : {retRes.ToString()};");
					Task.Delay(450).Wait();
				}

				if (chkClearSensor.Checked)
				{
					ShowCurrentReadStatusState(chkClearSensor);
					retRes = axTQ01001.ClearSensorStatus();
					ShowMsg($@"ClearSensorStatus return : {retRes.ToString()};");
					Task.Delay(450).Wait();
				}

				if (chkClearColumnHistory.Checked)
				{
					ShowCurrentReadStatusState(chkClearColumnHistory);
					retInt = axTQ01001.ClearColumnHistory();
					ShowMsg($@"ClearColumnHistory return : {retInt.ToString()};");
					Task.Delay(450).Wait();
				}

				if (chkResetMachine.Checked)
				{
					ShowCurrentReadStatusState(chkResetMachine);
					retInt = axTQ01001.ResetMachineStatus(ref pVarStatus);
					ShowMsg($@"ResetMachineStatus return : {retInt.ToString()}; pVarStatus : {JsonConvert.SerializeObject(pVarStatus)}");
					Task.Delay(450).Wait();
				}

				if (chkRebootMachine.Checked)
				{
					ShowCurrentReadStatusState(chkRebootMachine);
					retInt = axTQ01001.RebootMachine();
					ShowMsg($@"RebootMachine return : {retInt.ToString()};");
					//WaitDelay(10000); // Valid to apply
					//WaitDelay(5000);  // Valid to apply
					WaitDelay(4300);  // Valid to apply - Best Solution
					//WaitDelay(3500);  // Valid to apply
					//WaitDelay(3000);  // Valid to apply
					//WaitDelay(2000);  // !! Not safe to apply
					//WaitDelay(1000);  // Invalid to apply
					//WaitDelay(450);	// Invalid to apply
				}

				ShowCurrentReadStatusState(null);
				/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

				ShowMsg($@".. read coin status ..");

				ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);

				if (chkRebootMachine.Checked)
					_lowCoinManager.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

				if (isCheckStatusSuccess)
                {
					_isRunningLowCoin10 = is10CentCoinLow;
					_isRunningLowCoin20 = is20CentCoinLow;
					_isRunningLowCoin50 = is50CentCoinLow;
				}

				if ((_isRunningLowCoin10 == false) && (_isRunningLowCoin20 == false) && (_isRunningLowCoin50 == false))
					_lowCoinManager.IsReqToStartApp = false;

				ShowMsg(_lowCoinManager.GetStatusMessage());
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
            finally
            {
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private void ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow)
        {
			isCheckStatusSuccess = false;
			is10CentCoinLow = false;
			is20CentCoinLow = false;
			is50CentCoinLow = false;

			object refObj = null;

			int lRetVal = axTQ01001.GetLowCoinColumnStatus(ref refObj);
			if (lRetVal == 1)
			{
				isCheckStatusSuccess = true;

				Boolean Errflg = false;
				Object[] myarray = (Object[])refObj;
				for (var i = 0; i < 3; i++)
				{
					switch (i)
					{
						case 0:
							is10CentCoinLow = Convert.ToBoolean(myarray[i]);
							break;
						case 1:
							is20CentCoinLow = Convert.ToBoolean(myarray[i]);
							break;
						case 2:
							is50CentCoinLow = Convert.ToBoolean(myarray[i]);
							break;
						default:
							ShowMsg("Coin Type Not Found [ GetLowCoinColumnStatus ]");
							Errflg = true;
							break;
					}
				}

				_lowCoinManager.UpdateLowCoinStates(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

				//if (!Errflg)
				//{
				//SetCoinDisHis();
				//ResetCoinStatusFlg();
				if (is10CentCoinLow || is20CentCoinLow || is50CentCoinLow)
				{
					if (is10CentCoinLow)
						ShowMsg("Low Coin : 10");

					if (is20CentCoinLow)
						ShowMsg("Low Coin : 20");

					if (is50CentCoinLow)
						ShowMsg("Low Coin : 50");
				}
				else
				{
					ShowMsg("Coin Status : Good");
				}

				ShowMsg(_lowCoinManager.GetStatusMessage());
				//}
			}
			else
			{
				ShowMsg("Error when Show Bin Status; " + axTQ01001.GetTransactLastError());
			}

			WaitDelay(100);
		}
		
		private void btnGetLowCoinColumnStatus_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMsg($@"========== ========== Start - Get Low Coin Column Status ========== ==========");

				double dblTempCoinBalc = 80;
				Boolean flg = true;
				object lowCoinColStatus = null;
				long lRetVal2 = 0;
				bool currentflg10 = false;
				bool currentflg20 = false;
				bool currentflg50 = false;
				double intTemp = 0;

				lRetVal2 = axTQ01001.GetLowCoinColumnStatus(ref lowCoinColStatus);

				if (lRetVal2 == 1)
				{
					Object[] myarray = (Object[])lowCoinColStatus;
					for (var i = 0; i < 3; i++)
					{
						switch (i)
						{
							case 0:
								currentflg10 = Convert.ToBoolean(myarray[i]);
								break;
							case 1:
								currentflg20 = Convert.ToBoolean(myarray[i]);
								break;
							case 2:
								currentflg50 = Convert.ToBoolean(myarray[i]);
								break;
							default:
								ShowMsg("Coin Type Not Found");
								flg = false;
								break;
						}
					}

					if (currentflg10 || currentflg20 || currentflg50)
					{
						if (currentflg10)
						{
							ShowMsg("Low Coin Status : 10");
							flg = false;
						}
						if (currentflg20)
						{
							ShowMsg("Low Coin Status : 20");
							flg = false;
						}
						if (currentflg50)
						{
							ShowMsg("Low Coin Status : 50");
							flg = false;
						}
					}
					else
					{
						ShowMsg("Coin Status : Good");
					}

				}
				else
				{
					ShowMsg("Error Return from Coin Machine : " + lRetVal2.ToString());
					flg = false;
				}
				lowCoinColStatus = null;
				//return flg;
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
            finally
            {
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		private bool _disposed = false;
		private void btnReleaseCOMObj_Click(object sender, EventArgs e)
		{
			//axTQ01001
			try
			{
				ShowMsg($@"========== ========== Start - Release COM Obj. ========== ==========");

				if (_disposed)
					throw new Exception("axTQ01001 has been disposed !!");

				axTQ01001.Dispose();
				_disposed = true;
				//int retRes = System.Runtime.InteropServices.Marshal.ReleaseComObject(axTQ01001);
				ShowMsg($@"axTQ01001 has disposed.");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
            {
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

        private void ShowCurrentReadStatusState(CheckBox chkBox)
        {
			chkClearMachine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			chkClearMachine.ForeColor = System.Drawing.Color.Black;

			chkClearSensor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			chkClearSensor.ForeColor = System.Drawing.Color.Black;

			chkClearColumnHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			chkClearColumnHistory.ForeColor = System.Drawing.Color.Black;

			chkResetMachine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			chkResetMachine.ForeColor = System.Drawing.Color.Black;

			chkRebootMachine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			chkRebootMachine.ForeColor = System.Drawing.Color.Black;

			if (chkBox != null)
            {
				chkBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				chkBox.ForeColor = System.Drawing.Color.Blue;
			}

			Application.DoEvents();
			Task.Delay(50).Wait();

			return;
		}

        private void btnGetMachineErrors_Click(object sender, EventArgs e)
        {
			
			try
			{
				ShowMsg($@"========== ========== Get Machine Errors ========== ==========");
				UpdateMachineErrors(out bool errorFound, out string errorMsg);
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"-----Done-----{"\r\n"}");
			}
		}

		public void UpdateMachineErrors(out bool errorFound, out string errorMsg)
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
									{
										lblME1.BackColor = Color.LightGreen;
									}
									else
									{
										lblME1.BackColor = Color.Red;
										UpdateError("--Coin Eject Timeout Error--");
									}
									break;
								case 5:
									if (StrValue == "0")
									{
										lblME2.BackColor = Color.LightGreen;
									}
									else
									{
										lblME2.BackColor = Color.Red;
										UpdateError("--Coin Sensor Failed Error--");
									}
									break;
								case 6:
									if (StrValue == "0")
									{
										lblME3.BackColor = Color.LightGreen;
									}
									else
									{
										lblME3.BackColor = Color.Red;
										UpdateError("--Config Mismatch Error--");
									}
									break;
							}
						}
						ShowMsg("Last Transaction Error (A) : " + axTQ01001.GetTransactLastError());
					}
					else
					{
						UpdateError("--Last Transaction Error (Coin Mach.B)--; " + axTQ01001.GetTransactLastError());
						ShowMsg("Last Transaction Error (B) : " + axTQ01001.GetTransactLastError());
					}
				}
				catch (Exception ex)
				{
					UpdateError("--Last Transaction Error (Coin Mach.C)--; " + ex.Message);
					ShowMsg("Unexpected Error (UpdateMachineErrors) : " + ex.Message);
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

        private void btnRetryDispenseOnFail_Click(object sender, EventArgs e)
        {
			//try
			//{
			//	ShowMsg($@"========== ========== Retry Dispense On Fail ========== ==========");

			//	RetryDispenseCoin();

			//	if (_pendingDispensePriceStr != null)
			//	{
			//		throw new Exception($@"-Retry coin dispense is failed--; Pending Dispense Price Str.: {_pendingDispensePriceStr}");
			//	}
			//}
			//catch (Exception ex)
			//{
			//	ShowMsg(ex.ToString());
			//}
			//finally
			//{
			//	ShowMsg($@"-----Done-----{"\r\n"}");
			//}
		}

		private void RebootRecoverSequence()
        {
			int retInt = -1;

			int minTryUpdateMachineErrorsCount = 3;
			int maxTryUpdateMachineErrorsCount = 6;

			ShowCurrentReadStatusState(chkRebootMachine);
			retInt = axTQ01001.RebootMachine();
			ShowMsg($@"RebootMachine return : {retInt.ToString()};");
			WaitDelay(4300);  // Valid to apply - Best Solution

			for (int upMCntInt = 0; upMCntInt < maxTryUpdateMachineErrorsCount; upMCntInt++)
			{
				UpdateMachineErrors(out bool errorFound, out string errorMsg);

				WaitDelay(3000);

				if (errorFound)
					ShowMsg(errorMsg);

				else if (upMCntInt >= minTryUpdateMachineErrorsCount)
					break;
			}
		}

		private void MaintainCoinMachine()
        {
			if (_lowCoinManager.IsRebootMachineRequested == false)
				return;

			if (_lowCoinManager.IsAllCoinEmpty == false)
            {
				int topRebootCount = 20;
				int maxRebootCount = 0;
				int rebootCounter = 0;

				_lowCoinManager.QueryLowCoin(out bool isLow10Cent, out bool isLow20Cent, out bool isLow50Cent);

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
					
					_lowCoinManager.QueryCoinStatus(singleLowCoinCent, out bool isLow, out bool isEmpty, out bool isEnableToDispense);

					if (isEmpty == false)
                    {
						int lowCoinCountX = 0;
						maxRebootCount = 3;

						for (int rebCount = 0; rebCount < maxRebootCount; rebCount++)
                        {
							if (rebootCounter > topRebootCount)
								break;

							rebootCounter++;
							ShowMsg($@"MaintainCoinMachine.RebootMachine (A) Reboot Coin Machine Count : {rebootCounter}");
							retInt = axTQ01001.RebootMachine();
							ShowMsg($@"MaintainCoinMachine.RebootMachine (A); RebootMachine return : {retInt.ToString()};");
							WaitDelay(4300);  // Valid to apply - Best Solution

							ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
							_lowCoinManager.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

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
								_lowCoinManager.UpdateEmptyCoinCounterAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

								if ((is10CentCoinLow == false) && (is20CentCoinLow == false) && (is50CentCoinLow == false))
								{
									ShowMsg($@"MaintainCoinMachine.RebootMachine (A) - Done");
									break;
								}
								else if (singleLowCoinCent == 10)
                                {
									_lowCoinManager.QueryCoinStatus(10, out bool is10CentLow, out bool is10CentEmpty, out bool is10CentEnableToDispense);

									if (is10CentEmpty)
									{
										ShowMsg($@"MaintainCoinMachine.RebootMachine (A) - 10 Cent Coin Bin is empty. Not able to dispense coin. Coin machine blocked");
										break;
									}
									else if (is10CentEnableToDispense == false)
										rebCount = 1;
								}
							}
						}

						ShowMsg($@"MaintainCoinMachine.RebootMachine (C); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {_lowCoinManager.IsDispensePossible}");
					}
				}

				//else if ((noOfLowCoin > 1) 
				//	&& (_lowCoinManager.IsAllCoinEmpty == false) 
				//	&& ((_lowCoinManager.IsDispensePossible == false) || (_lowCoinManager.IsRequestCriticalRebootSpinMotor == true)))

				else if ((noOfLowCoin > 1)
					&& (_lowCoinManager.IsRequestCriticalRebootSpinMotor == true))
				{
					int outStanding = 0;

					_lowCoinManager.QueryCoinStatus(10, out bool is10CentLow, out bool is10CentEmpty, out bool is10CentEnableToDispense);

					if (is10CentEmpty == false)
                    {
						_lowCoinManager.QueryCoinStatus(20, out bool is20CentLow, out bool is20CentEmpty, out bool is20CentEnableToDispense);
						_lowCoinManager.QueryCoinStatus(50, out bool is50CentLow, out bool is50CentEmpty, out bool is50CentEnableToDispense);

						if ((is20CentEmpty == false) || (is50CentEmpty == false))
                        {
							maxRebootCount = 6;

							for (int rebCount = 0; rebCount < maxRebootCount; rebCount++)
							{
								if (rebootCounter > topRebootCount)
									break;

								rebootCounter++;
								ShowMsg($@"MaintainCoinMachine.RebootMachine (B) Reboot Coin Machine Count : {rebootCounter}; Reboot Count: {rebootCounter}");
								retInt = axTQ01001.RebootMachine();
								ShowMsg($@"MaintainCoinMachine.RebootMachine (B); RebootMachine return : {retInt.ToString()};");
								WaitDelay(4300);  // Valid to apply - Best Solution

								ShowBinStatus(out bool isCheckStatusSuccess, out Boolean is10CentCoinLow, out Boolean is20CentCoinLow, out Boolean is50CentCoinLow);
								_lowCoinManager.UpdateLowStateAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

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
										ShowMsg($@"MaintainCoinMachine.RebootMachine (B) - All coin recovered.");
										break;
									}
									else
                                    {
										_lowCoinManager.UpdateEmptyCoinCounterAfterCoinMachReboot(is10CentCoinLow, is20CentCoinLow, is50CentCoinLow);

										outStanding = 0;
										_lowCoinManager.QueryCoinStatus(10, out bool is10CentLowX2, out bool is10CentEmptyX2, out bool is10CentEnableToDispenseX2);
										_lowCoinManager.QueryCoinStatus(20, out bool is20CentLowX2, out bool is20CentEmptyX2, out bool is20CentEnableToDispenseX2);
										_lowCoinManager.QueryCoinStatus(50, out bool is50CentLowX2, out bool is50CentEmptyX2, out bool is50CentEnableToDispenseX2);

										if (is10CentLowX2)
											outStanding++;

										if (is20CentLowX2)
											outStanding++;

										if (is50CentLowX2)
											outStanding++;

										if (is10CentEmptyX2)
										{
											ShowMsg($@"MaintainCoinMachine.RebootMachine (B) - 10 Cent is empty. Not able to dispense coin. Coin machine Blocked");
											break;
										}
										else if (is10CentEnableToDispenseX2 == false)
                                        {
											rebCount = 1;
										}
										else if (is20CentEmptyX2 && is50CentEmptyX2)
										{
											ShowMsg($@"MaintainCoinMachine.RebootMachine (B) - 20 Cent & 50 Cent are empty.");
											break;
										}
										else if (outStanding > 1)
										{
											rebCount = 1;
										}
										else if (is20CentEmptyX2 && (is50CentLowX2 == false))
										{
											ShowMsg($@"MaintainCoinMachine.RebootMachine (B) - 20 Cent is Empty; 50 Cent Bin has recovered.");
											break;
										}
										else if (is50CentEmptyX2 && (is20CentLowX2 == false))
										{
											ShowMsg($@"MaintainCoinMachine.RebootMachine (B) - 50 Cent is Empty; 20 Cent Bin has recovered.");
											break;
										}
									}
								}
							}
						}

						ShowMsg($@"MaintainCoinMachine.RebootMachine (C); End Coin Machine maintenance; Is Coin Machine Possible to Dispense : {_lowCoinManager.IsDispensePossible}");
					}
				}
			}
        }

		/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		private bool _proceedLoopTest = false;
		private void LoopTest()
        {
			int maxTestLoopCount = 50;

			_proceedLoopTest = true;
			for (int currLoopCount = 0; currLoopCount < maxTestLoopCount; currLoopCount++)
            {
				WaitDelay(1000);

				ShowMsg($@"----- Start Test({currLoopCount})-----");
				TryDispense("0.80");
				ShowMsg($@"----- -End- Test({currLoopCount})-----");

				if (_proceedLoopTest == false)
                {
					ShowMsg($@"------ End Test({currLoopCount}) Abnormally !!!!! -----");
					break;
                }
			}
			
		}

        private void btnStartTestingLoop_Click(object sender, EventArgs e)
        {
            try
            {
				ShowMsg($@"========== ========== Start Dispense Loop Test ========== ==========");

				btnStartTestingLoop.Enabled = false;
				Application.DoEvents();

				LoopTest();
			}
			catch(Exception ex)
            {
				ShowMsg(ex.ToString());
			}
			finally
            {
				btnStartTestingLoop.Enabled = true;

				ShowMsg($@"--------------- --------------- Done --------------- ---------------{"\r\n"}");
				Application.DoEvents();
			}
        }
    }
}
