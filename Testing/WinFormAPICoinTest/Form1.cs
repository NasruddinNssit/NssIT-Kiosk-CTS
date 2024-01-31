using NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi;
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

namespace WinFormAPICoinTest
{
    public partial class Form1 : Form
    {
        //private LibShowMessageWindow.MessageWindow _msgBox = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        private IAxCX2_3 _coinMach = null;
        private bool? _isCX2D3Valid = null; /* null: not ready; false: Shutdown; true: ready */

        private string _testLoopingPrice = "0.80";
        private bool _isStopTesting = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                ShowMsg("Starting.....");

                if (_coinMach != null)
                    ShowMsg("Coin Machine API already created");
                else
                {
                    if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                    {
                        if (isMachineQuitProcess == false)
                        { }
                        else if (isMachineQuitProcess == true)
                            throw new Exception("Fail to create Coin Machine API. Check machine connection");
                    }
                    else
                        throw new Exception("Coin Machine API already created");

                    _coinMach = new AxCX2_3();

                    if (_coinMach.ConnectDevice())
                    {
                        _isCX2D3Valid = true;
                        ShowMsg("Coin Machine API created successful.");
                    }
                    else
                    {
                        _isCX2D3Valid = false;
                        ShowMsg("Fail to connect Coin Machine API.");
                        EndCoinMach();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());

                if (_isCX2D3Valid.HasValue == false)
                {
                    ShowMsg("Coin Machine API not yet created");
                }
                else if (_isCX2D3Valid == false)
                {
                    ShowMsg("Fail to create Coin Machine API");
                }
                EndCoinMach();
            }
            finally
            {
                ShowMsg($@"-----Done-----{"\r\n"}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            EndCoinMach();
        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            try
            {
                ShowMsg($@"========== ========== Start - Reboot Coin Bin Machine ========== ==========");

                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                    else
                        throw new Exception("Coin Machine API already quit");
                }
                
                if (_coinMach.SpinMachineMotor(-1, "WinFormAPICoinTest.btnReboot_Click"))
                {
                    _coinMach.CheckLowCoinStatus(out _, out _, out _);
                }
                else
                {
                    ShowMsg($@"Error when spin motor");
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Please start coin machine");
            }
            finally
            {
                ShowMsg($@"-----Done-----{"\r\n"}");
            }
        }

        private void btnEndCoinMach_Click(object sender, EventArgs e)
        {
            EndCoinMach();
        }

        private void OnDispenseCents_Click(object sender, EventArgs e)
        {
            Button currButton = (Button)sender;
            ShowMsg($@"Button Tag is {(string)currButton.Tag}");

            ShowMsg($@"========== ========== Start - Dispense ========== ==========");
            TryDispense((string)currButton.Tag);
            ShowMsg($@"-----Done-----{"\r\n"}");
        }

        private bool CoinMachIsReady(out bool isMachineQuitProcess)
        {
            isMachineQuitProcess = false;

            if (_isCX2D3Valid.HasValue == false)
            {
                return false;
            }
            else if (_isCX2D3Valid == false)
            {
                isMachineQuitProcess = true;
                return false;
            }
            else
                return true;
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
                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                    {
                        throw new Exception("Coin machine not ready.");
                    }
                    else if (isMachineQuitProcess == true)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }
            
                if (_coinMach.LowCoinMan.IsReqToStartApp)
					throw new Exception("Request to Restart Application");

				if (_coinMach.LowCoinMan.IsDispensePossible == false)
					throw new Exception("Not possible to dispense coin");

                if (decimal.TryParse(priceStr, out amount) && (amount > 0))
                {
                    totalCent = Math.Floor(amount * 100M);

                    if (_coinMach.DispenseCoin(Convert.ToInt32(totalCent), out string notEnoughCoinMessage))
                    {
                        ShowMsg($@"{Convert.ToInt32(totalCent)} Cent has been dispensed successful");
                    }
                    else if (string.IsNullOrWhiteSpace(notEnoughCoinMessage))
                    {
                        ShowMsg("Unknown error when dispense coin");
                    }
                    else
                    {
                        ShowMsg($@"Error : {notEnoughCoinMessage}");
                    }
                }
			}
			catch (Exception ex)
			{
                ShowMsg(ex.ToString());
            }
		}

        private void EndCoinMach()
        {
            try
            {
                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }

                if (_coinMach != null)
                    _coinMach.CloseDevice();

                _coinMach = null;
                _isCX2D3Valid = null;

                ShowMsg("Coin machine ended API process");
            }
            catch (Exception ex)
            {
                ShowMsg("Please start coin machine");
            }
        }

        private void btnSpinCoinBox1_Click(object sender, EventArgs e)
        {
            try
            {
                ShowMsg($@"========== ========== Start - Spin Coin Box1 (10 cent) ========== ==========");

                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                    {
                        throw new Exception("Coin machine not ready.");
                    }
                    else if (isMachineQuitProcess == true)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }

                if (_coinMach.SpinDispenseByBin(NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command.CX2D3SpinIndex.One))
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
                ShowMsg($@"========== ========== Start - Spin Coin Box1 (10 cent) ========== ==========");

                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                    {
                        throw new Exception("Coin machine not ready.");
                    }
                    else if (isMachineQuitProcess == true)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }

                if (_coinMach.SpinDispenseByBin(NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command.CX2D3SpinIndex.Two))
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

        private void btnSpinCoinBox3_Click(object sender, EventArgs e)
        {
            try
            {
                ShowMsg($@"========== ========== Start - Spin Coin Box1 (10 cent) ========== ==========");

                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                    {
                        throw new Exception("Coin machine not ready.");
                    }
                    else if (isMachineQuitProcess == true)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }

                if (_coinMach.SpinDispenseByBin(NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command.CX2D3SpinIndex.Three))
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

        private void btnStartTestingLoop_Click(object sender, EventArgs e)
        {
            try
            {
                ShowMsg($@"========== ========== Start - Spin Coin Box1 (10 cent) ========== ==========");

                if (CoinMachIsReady(out bool isMachineQuitProcess) == false)
                {
                    if (isMachineQuitProcess == false)
                    {
                        throw new Exception("Coin machine not ready.");
                    }
                    else if (isMachineQuitProcess == true)
                        throw new Exception("Fail to create Coin Machine API. Check machine connection");
                }

                LoopTest();
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
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void LoopTest()
            {
                _isStopTesting = false;
                int maxTestLoopCount = 50;
                for (int currLoopCount = 0; currLoopCount < maxTestLoopCount; currLoopCount++)
                {
                    if (_isStopTesting)
                    {
                        ShowMsg($@"Stop testing by force");
                        break;
                    }

                    Application.DoEvents();
                    WaitDelay(1000);

                    ShowMsg($@"----- Start Test({currLoopCount})-----");
                    TryDispense(_testLoopingPrice);
                    ShowMsg($@"----- -End- Test({currLoopCount})-----");

                    if (_coinMach.LowCoinMan.IsReqToStartApp == true)
                    {
                        ShowMsg($@"------ End Test({currLoopCount}) Abnormally !!!!! ; Application needed to be restarted -----");
                        break;
                    }
                    else if (_coinMach.LowCoinMan.IsDispensePossible == false)
                    {
                        ShowMsg($@"------ End Test({currLoopCount}) !!!!! -----");
                        break;
                    }
                }

            }
        }

        private void btnStopTesting_Click(object sender, EventArgs e)
        {
            _isStopTesting = true;
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
        
        private void TestLoopingPrice_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender is RadioButton rbt) && (rbt.Tag is string tagStr) && (string.IsNullOrWhiteSpace(tagStr) == false))
            {
                _testLoopingPrice = tagStr.Trim();
            }
        }

        private void ShowMsg(string msg)
        {
            msg = msg ?? "--";
            txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {msg}{"\r\n"}");
            txtMsg.Refresh();
            Application.DoEvents();
        }
    }
}