using NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK;
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

namespace WinFormAccessSDKCoinTest
{
    public partial class Form1 : Form
    {
        private CX2D3Access _coinMach = null;
        private bool? _isCX2D3Valid = null; /* null: not ready; false: Shutdown; true: ready */

        private string _testLoopingPrice = "0.80";
        private bool _isStopTesting = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            EndCoinMach();
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

                    _coinMach = new CX2D3Access();

                    WaitDelay(3000);
                    _isCX2D3Valid = true;
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

        private void btnEndCoinMach_Click(object sender, EventArgs e)
        {
            EndCoinMach();
        }

        private void OnDispenseCents_Click(object sender, EventArgs e)
        {
            Button currButton = (Button)sender;
            ShowMsg($@"Button Tag is {(string)currButton.Tag}");

            ShowMsg($@"========== ========== Start - Dispense ========== ==========");
            TryDispense((string)currButton.Tag, out _, out _);
            ShowMsg($@"-----Done-----{"\r\n"}");
        }

        private void TestLoopingPrice_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender is RadioButton rbt) && (rbt.Tag is string tagStr) && (string.IsNullOrWhiteSpace(tagStr) == false))
            {
                _testLoopingPrice = tagStr.Trim();
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
                int maxTestLoopCount = 500;
                for (int currLoopCount = 0; currLoopCount < maxTestLoopCount; currLoopCount++)
                {
                    if (_isStopTesting)
                    {
                        ShowMsg($@"Stop testing by force");
                        break;
                    }

                    Application.DoEvents();
                    WaitDelay(1000);

                    ShowMsg($@"----- Start Test({currLoopCount})-----; {currLoopCount}");
                    TryDispense(_testLoopingPrice, out string isStopDispenseMsg, out bool isBusy);
                    ShowMsg($@"----- -End- Test({currLoopCount})-----");

                    if (string.IsNullOrWhiteSpace(isStopDispenseMsg) == false)
                    {
                        ShowMsg($@"------ End Test({currLoopCount}) !!!!! ; {isStopDispenseMsg} -----");
                        break;
                    }
                    else if (isBusy)
                    {
                        currLoopCount--;
                        ShowMsg($@".. coin machine busy. ..wait 3 sec..");
                        WaitDelay(3000);
                    }
                }
            }
        }

        private void btnStopTesting_Click(object sender, EventArgs e)
        {
            _isStopTesting = true;
        }

        private void TryDispense(string priceStr, out string isStopDispenseMsg, out bool isBusy)
        {
            isStopDispenseMsg = null;
            isBusy = false;
            decimal amount = 0.00M;
            decimal totalCent = 0.00M;
            bool flg = true;
            bool flgCoin = false;
            bool needToCheckCoinStatus = false;
            string processId = $@"PID{DateTime.Now: MMddHHmmss-fff}";

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

                bool isCoinMachReady = _coinMach.CheckMachineIsReady(out bool isMachQuitProcess, out bool isAppShuttingDown, out bool isLowCoin, 
                    out bool isAccessSDKBusy, out bool isRecoveryInProgressAfterDispenseCoin, 
                    out string errorMessage);

                if (!isCoinMachReady)
                {
                    if (isMachQuitProcess)
                    {
                        isStopDispenseMsg = "Coin Machine already quit process.";
                        throw new Exception(isStopDispenseMsg);
                    }
                    else if (isAppShuttingDown)
                    {
                        isStopDispenseMsg = "Coin Machine already shutdown.";
                        throw new Exception(isStopDispenseMsg);
                    }
                    else if (isLowCoin)
                    {
                        isStopDispenseMsg = $@"Coin Machine not able to dispense coin. {errorMessage}";
                        throw new Exception(isStopDispenseMsg);
                    }
                    else if (isAccessSDKBusy)
                    {
                        isBusy = true;
                        throw new Exception("Coin Machine Access SDK busy.");
                    }
                    else if (string.IsNullOrWhiteSpace(errorMessage) == false)
                        throw new Exception(errorMessage);

                    else
                        throw new Exception("Coin Machine not ready.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(errorMessage) == false)
                        ShowMsg(errorMessage);
                }

                if (_coinMach.GetDispensePossibility(processId, 0.80M, out string lowCoinMsg, out string machOutOfSvcMsg))
                {
                    if (machOutOfSvcMsg != null)
                        throw new Exception(machOutOfSvcMsg);

                    if (string.IsNullOrWhiteSpace(lowCoinMsg) == false)
                        ShowMsg($@"TryDispense message --> {lowCoinMsg}");
                }
                else
                    throw new Exception("Coin Machine is not ready");

                if (decimal.TryParse(priceStr, out amount) && (amount > 0))
                {
                    totalCent = Math.Floor(amount * 100M);

                    _coinMach.Dispense(processId, amount, processId);

                    ShowMsg($@"{Convert.ToInt32(totalCent)} Cent should be dispensed");
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
                {
                    _coinMach.Dispose();
                    WaitDelay(2000);
                }
                _coinMach = null;
                _isCX2D3Valid = null;

                ShowMsg("Coin machine ended API process");
            }
            catch (Exception ex)
            {
                ShowMsg("Please start coin machine");
            }
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

        private void ShowMsg(string msg)
        {
            msg = msg ?? "--";
            txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss.fff")} - {msg}{"\r\n"}");
            txtMsg.Refresh();
            Application.DoEvents();
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

        
    }
}
