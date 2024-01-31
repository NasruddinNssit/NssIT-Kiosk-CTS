using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public partial class Form1 : Form
	{
		private Boolean isRunningLowCoin10 = false;
		private Boolean isRunningLowCoin20 = false;
		private Boolean isRunningLowCoin50 = false;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{ }

		private void ShowMsg(string msg)
		{
			if (_isThisFormWasActivated)
			{
				msg = msg ?? "--";
				txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {msg}{"\r\n"}");
				//txtMsg.Refresh();
				Application.DoEvents();
			}
		}

		private bool _isThisFormWasActivated = false;
		private void Form1_Activated(object sender, EventArgs e)
		{
			_isThisFormWasActivated = true;
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			ConnectDevice();
		}

		public bool ConnectDevice()
		{
			int lRetVal = -1;
			string strOcxVersion = "";
			strOcxVersion = "";

			ShowMsg(""); ShowMsg(""); ShowMsg("");

			try
			{
				ShowMsg($@"Start Coin Machine ..");

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
				}
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}

			return (lRetVal == 1) ? true : false;
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
			TryDispense((string)currButton.Tag);
		}

		public bool TryDispense(string priceStr)
		{
			decimal amount = 0.00M;
			decimal totalCent = 0.00M;
			bool flg = true;
			bool flgCoin = false;

			try
			{
				ShowMsg("Start Dispense ..");
				if (decimal.TryParse(priceStr, out amount) && (amount > 0))
				{
					totalCent = Math.Floor(amount * 100M);

					ShowMsg($@"Dispense Total : {totalCent.ToString()} cents");

					flgCoin = CheckRefundPossibility(totalCent, flg);

					ShowMsg($@"Possibility of coin refund : {(flgCoin ? "Yes" : "No")}");

					if (flgCoin)
					{
						string notEnoughCoinMessage = null;

						string dispenseCode = GetCoinDispenseCode(Convert.ToInt32(totalCent), out notEnoughCoinMessage);

						if (notEnoughCoinMessage == null)
						{
							ShowMsg($@"Coin Dispense Code : {dispenseCode}; ** 10 cents : {dispenseCode.Substring(0, 1)} pcs ; ** 20 cents : {dispenseCode.Substring(1, 1)} pcs ; ** 50 cents : {dispenseCode.Substring(2, 1)} pcs ;");

							flgCoin = false;
							flgCoin = axTQ01001.DispenseByColumn(dispenseCode);
							if (flgCoin)
								ShowMsg($@"Dispense coin command sent successfully.");
							else
								ShowMsg($@"Dispense coin completed with fail state.");

							Task.Delay(300).Wait();
						}
						else
							ShowMsg(notEnoughCoinMessage);

					}
					else
					{
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
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
				ShowMsg(""); ShowMsg("");
			}

			return flgCoin;
		}

		private void btnResetCoinMachine_Click(object sender, EventArgs e)
		{
			object pVarStatus = null;
			int retInt = -1;
			bool retRes = false;

			ShowMsg(""); ShowMsg(""); ShowMsg("");

			try
			{
				ShowMsg("Start Reset-Coin-Machine ..");

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
				Task.Delay(500).Wait();

				object tempObj = null;
				retInt = axTQ01001.EnableBuzzerSound(false, ref tempObj);
				ShowMsg($@"Set EnableBuzzerSound to false; return : {retInt}; Out obj : {JsonConvert.SerializeObject(tempObj)}");

				ShowMsg($@"Reset Coin Machine --- DONE");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}
		}

		private void btnSpinCoinBox1_Click(object sender, EventArgs e)
		{
			ShowMsg(""); ShowMsg(""); ShowMsg("");

			try
			{
				ShowMsg("Start Spin Coin Box1 ..");

				if (axTQ01001.SpinByBin(1))
					ShowMsg($@"Spin Coin Box1 Completed");
				else
					ShowMsg($@"Spin Coin Box1 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}
		}

		private void btnSpinCoinBox2_Click(object sender, EventArgs e)
		{
			ShowMsg(""); ShowMsg(""); ShowMsg("");

			try
			{
				ShowMsg("Start Spin Coin Box2 ..");

				if (axTQ01001.SpinByBin(2))
					ShowMsg($@"Spin Coin Box2 Completed");
				else
					ShowMsg($@"Spin Coin Box2 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}
		}

		private void btnSpinCoinBox3_Click(object sender, EventArgs e)
		{
			ShowMsg(""); ShowMsg(""); ShowMsg("");

			try
			{
				ShowMsg("Start Spin Coin Box3 ..");

				if (axTQ01001.SpinByBin(3))
					ShowMsg($@"Spin Coin Box3 Completed");
				else
					ShowMsg($@"Spin Coin Box3 Error; Return : false");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
			finally
			{
				ShowMsg($@"Done");
			}
		}

		private void btnGetLastDispenseHistory_Click(object sender, EventArgs e)
		{
			ShowMsg(""); ShowMsg(""); ShowMsg("");

			object refObj = null;
			int lRetVal = -1;

			int flg10 = 0;
			int flg20 = 0;
			int flg50 = 0;

			try
			{
				ShowMsg("Start - Get Accumulated Dispensed History ..; History is refer to last switch on time;");

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
				ShowMsg($@"Done");
			}
		}

		private string GetCoinDispenseCode(int dblTempCoinCent, out string notEnoughCoinMessage)
		{
			notEnoughCoinMessage = null;

			int intCoin50 = 0;
			int intCoin20 = 0;
			int intCoin10 = 0;

			string possibleDispenseCoins = "";

			if (!isRunningLowCoin10)
			{
				if ((!isRunningLowCoin50) && (dblTempCoinCent > 0))
				{
					intCoin50 = Convert.ToInt32(Math.Floor((decimal)dblTempCoinCent / 50M));

					if (intCoin50 > 9)
						intCoin50 = 9;

					dblTempCoinCent -= (intCoin50 * 50);
					possibleDispenseCoins += (possibleDispenseCoins.Length > 0 ? " , " : "") + $@"## 50 cents: {intCoin50} pcs";
				}
				if ((!isRunningLowCoin20) && (dblTempCoinCent > 0))
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
			else if ((!isRunningLowCoin50) && (!isRunningLowCoin20))
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
				if ((!isRunningLowCoin50) && (dblTempCoinCent > 0))
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
				if ((!isRunningLowCoin20) && (dblTempCoinCent > 0))
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

		public Boolean CheckRefundPossibility(decimal dblTempCoinBalc, Boolean flg)
		{
			long lRetVal2 = 0;
			isRunningLowCoin10 = false;
			isRunningLowCoin20 = false;
			isRunningLowCoin50 = false;
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
							isRunningLowCoin10 = Convert.ToBoolean(myarray[i]);
							if (isRunningLowCoin10) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "10 cents";
							break;
						case 1:
							isRunningLowCoin20 = Convert.ToBoolean(myarray[i]);
							if (isRunningLowCoin20) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "20 cents";
							break;
						case 2:
							isRunningLowCoin50 = Convert.ToBoolean(myarray[i]);
							if (isRunningLowCoin50) lowCoinTypeStr += ((lowCoinTypeStr.Length > 0) ? ", " : "") + "50 cents";
							break;
						default:
							ShowMsg("Coin Type Not Found");
							flg = false;
							break;
					}
				}

				if (lowCoinTypeStr.Length > 0)
					ShowMsg($@"Low Coin Status Triggered for {lowCoinTypeStr}");

				if ((!isRunningLowCoin10) && (!isRunningLowCoin20) && (!isRunningLowCoin50))
				{
					ShowMsg("Normal Coin Status for 10 cents, 20 cents, and 50 cents");
				}
				else if (isRunningLowCoin10 && isRunningLowCoin20 && isRunningLowCoin50)
				{
					ShowMsg($@"Low Coin Status for 10 cents, 20 cents, and 50 cents; Unable to dispense total of {dblTempCoinBalc} cents");
					flg = false;
				}
				else if (isRunningLowCoin10 && isRunningLowCoin20)
				{
					intTemp = dblTempCoinBalc - (Convert.ToInt32(Math.Floor(dblTempCoinBalc / 50M)) * 50M);
					if (intTemp > 0)
					{
						ShowMsg($@"Low Coin Status for 10 cents and 20 cents; Unable to dispense total of {dblTempCoinBalc} cents");
						flg = false;
					}
				}
				else if (isRunningLowCoin10 && isRunningLowCoin50)
				{
					intTemp = dblTempCoinBalc - (Convert.ToInt32(Math.Floor(dblTempCoinBalc / 20M)) * 20M);
					if (intTemp > 0)
					{
						ShowMsg($@"Low Coin Status for 10 cents, 50 cents; Unable to dispense total of {dblTempCoinBalc} cents");
						flg = false;
					}
				}
				else if (isRunningLowCoin10)
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
			return flg;
		}

		private void btnReleaseCOMObj_Click(object sender, EventArgs e)
		{
			//axTQ01001
			try
			{
				this.CloseCX2_3();
				//int retRes = System.Runtime.InteropServices.Marshal.ReleaseComObject(axTQ01001);
				ShowMsg($@"axTQ01001 has disposed.");
			}
			catch (Exception ex)
			{
				ShowMsg(ex.ToString());
			}
		}

		public void CloseCX2_3()
		{
			axTQ01001.Dispose();
		}
	}
}
