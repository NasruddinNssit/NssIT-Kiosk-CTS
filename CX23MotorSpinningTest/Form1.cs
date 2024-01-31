using NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CX23MotorSpinningTest
{
    public partial class Form1 : Form
    {
        private IAxCX2_3 _coinMach = null;
        private bool? _isCX2D3Valid = null; /* null: not ready; false: Shutdown; true: ready */

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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

        
    }
}
