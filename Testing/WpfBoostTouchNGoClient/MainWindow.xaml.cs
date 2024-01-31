using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.Tools.Timer;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBoostTouchNGoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;

        public (string SalesTransactionNo, string MerchantTransactionNo, decimal Amount, string SnRId, string PaymentGateway, string Currency, bool? IsSuccess) _saleTrans =
            (SalesTransactionNo: null, MerchantTransactionNo: null, Amount: 0.0M, SnRId: null, PaymentGateway: null, Currency: null, IsSuccess: null);

        private bool _isTransactionEnd = false;
        private object _transEndLock = new object();
        private CountDownTimer _endTransCountDown = null;

        public MainWindow()
        {
            InitializeComponent();

            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

            _endTransCountDown = new CountDownTimer();
            _endTransCountDown.OnCountDown += _endTransCountDown_OnCountDown;
            _endTransCountDown.OnExpired += _endTransCountDown_OnExpired;
        }

        /// <summary>
        /// Return true when success set the _isTransactionEnd flag. Else return false.
        /// </summary>
        /// <param name="transHasBeenEnded">Return true when _isTransactionEnd has already set to true in previous stage</param>
        /// <returns></returns>
        private bool SetEndTransaction(out bool transHasBeenEnded)
        {
            transHasBeenEnded = true;

            bool retResult = false;
            bool transHasBeenEndedX = _isTransactionEnd;

            if (_isTransactionEnd == false)
            {
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_transEndLock)
                    {
                        transHasBeenEndedX = _isTransactionEnd;

                        if (_isTransactionEnd == false)
                        {
                            _isTransactionEnd = true;
                            retResult = true;
                        }
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Start();
                tWorker.Join();
            }

            transHasBeenEnded = transHasBeenEndedX;
            return retResult;
        }

        private void ResetPaymentTransData()
        {
            _saleTrans.MerchantTransactionNo = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.Amount = 0.0M;
            _saleTrans.Currency = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.SalesTransactionNo = null;
            _saleTrans.SnRId = null;
            _saleTrans.IsSuccess = null;
        }

        private void _endTransCountDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtCountDown.Text = "0";
            }));

            //Quit Payment
        }

        private void _endTransCountDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtCountDown.Text = e.TimeRemainderSec.ToString();
            }));
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            try
            {
                var payData = kioskMsg.GetMsgData();
                if (payData is UIxBTnGPaymentNewTransStartedAck newTrans)
                {
                    _saleTrans.Amount = newTrans.Amount;
                    _saleTrans.MerchantTransactionNo = newTrans.MerchantTransactionNo;
                    _saleTrans.SalesTransactionNo = newTrans.BTnGSalesTransactionNo;
                    _saleTrans.SnRId = newTrans.SnRId;

                    if (_endTransCountDown.Activated)
                        _endTransCountDown.ResetCounter();

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtBTnGSaleTransNo.Text = newTrans.BTnGSalesTransactionNo;

                        BtnNewSale.IsEnabled = false;
                        BtnNewDocNo.IsEnabled = false;
                        BtnShowSvrTime.IsEnabled = true;
                        BtnSendEcho.IsEnabled = true;
                        BtnCancelSale.IsEnabled = true;

                        ShowBarCode(newTrans.Base64ImageQrCode);

                        TxtSnRClientId.Text = (_saleTrans.SnRId ?? "").Trim();

                        TxtInProgressMsg.Text = "Waiting for 2D Barcode scanning";
                    }));

                    //_countDown?.ChangeCountDown(_countDownCode_WaitForScanning, 75, 500);
                }
                else if (payData is UIxBTnGPaymentCountDownAck cntD)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (_isTransactionEnd == false)
                        {
                            if ((_endTransCountDown.Activated) && (cntD.CountDown > 0))
                                _endTransCountDown.ResetCounter();

                            TxtCountDown.Text = cntD.CountDown.ToString();
                        }
                    }));
                }
                else if (payData is UIxBTnGPaymentCustomerMsgAck custMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (custMsg.Message != null)
                            TxtMessage.Text = custMsg.Message.Trim();
                    }));
                }
                else if (payData is UIxBTnGPaymentInProgressMsgAck prgMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if ((prgMsg.Message?.Contains("SignalREchoMessage;") == true)
                            || (prgMsg.Message?.Contains("ReadServerTime;") == true))
                        {
                            if (prgMsg.Message != null)
                                _msg.ShowMessage(prgMsg.Message.Trim());
                        }
                        else
                            if (prgMsg.Message != null)
                            TxtInProgressMsg.Text = prgMsg.Message.Trim();

                        if (prgMsg.IsCancelAllowed.HasValue) { }
                    }));
                }
                else if (payData is UIxBTnGPaymentErrorAck errMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (errMsg.ErrorMsg != null)
                            TxtError.Text = errMsg.ErrorMsg.Trim();

                        TxtInProgressMsg.Text = "Error found ..";
                    }));
                }
                else if (payData is UIxBTnGPaymentEndAck endMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        EndSale();

                        if (endMsg.ResultState == PaymentResult.Success)
                        {
                            /////CYA-DEBUG ---------------------------------------------------------------------------------------
                            ////Task.Factory.StartNew(new Action(() => 
                            ////{
                            ////    Thread.Sleep(5 * 1000);
                            ////    if (SetEndTransaction(out _))
                            ////    {
                            ////        _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                            ////        this.Dispatcher.Invoke(new Action(() => 
                            ////        {
                            ////            TxtInProgressMsg.Text = "Payment Success";
                            ////            TxtMessage.Text = "Payment Success";
                            ////        }));
                            ////    }
                            ////    else
                            ////        _msg.ShowMessage($@"Unable to set sale to success since transaction has already end");
                            ////}));
                            /////--------------------------------------------------------------------------------------------------

                            if (SetEndTransaction(out _))
                            {
                                _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                                TxtInProgressMsg.Text = "Payment Success";
                                TxtMessage.Text = "Payment Success";
                                _saleTrans.IsSuccess = true;
                            }
                            else
                                _msg.ShowMessage($@"Unable to set sale to success since transaction has already end");
                        }
                        else if (endMsg.ResultState == PaymentResult.Cancel)
                        {
                            if (SetEndTransaction(out _))
                            {
                                TxtInProgressMsg.Text = "Payment Transaction has been canceled";
                                TxtMessage.Text = "Transaction canceled";
                                _saleTrans.IsSuccess = false;

                                _endTransCountDown.ChangeCountDown("ENDING", 7, 700);
                            }
                            else
                                _endTransCountDown.ChangeCountDown("ENDING", 10, 700);
                        }
                        else
                        {
                            _endTransCountDown.ChangeCountDown("ENDING", 10, 700);

                            if (SetEndTransaction(out _))
                            {
                                //if (string.IsNullOrWhiteSpace(_saleTrans.SalesTransactionNo) == false)
                                //{
                                //    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                                //        _saleTrans.MerchantTransactionNo, _saleTrans.SalesTransactionNo,
                                //        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                                //        _saleTrans.PaymentGateway, _saleTrans.Amount);
                                //}

                                _saleTrans.IsSuccess = false;

                                string endMessage = (endMsg.ErrorMsg ?? "").Trim();

                                if (string.IsNullOrWhiteSpace(endMessage) == false)
                                    endMessage = endMsg.Message ?? "";

                                if (endMsg.ResultState == PaymentResult.Timeout)
                                {
                                    TxtInProgressMsg.Text = $@"Timeout; Fail Payment Transaction; {endMessage}";

                                    TxtMessage.Text = "Timeout";
                                }
                                else if (endMsg.ResultState == PaymentResult.Fail)
                                {
                                    TxtInProgressMsg.Text = $@"Fail Payment Transaction; {endMessage}";
                                    TxtMessage.Text = "Fail Transaction";
                                }
                                else
                                {
                                    TxtInProgressMsg.Text = $@"Fail Payment Transaction with unknown error; {endMessage}";
                                    TxtMessage.Text = "Fail Transaction (II)";
                                }
                            }
                        }
                        TxtCountDown.Text = "END";
                    }));
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"MainWindow:BTnGShowPaymentInfo => Error : {ex.ToString()}");
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void ShowBarCode(string base64String)
            {
                byte[] imgBytes = Convert.FromBase64String(base64String);

                // Image object 
                //this.Dispatcher.Invoke(new Action(() => {
                BitmapImage bitmapImage = new BitmapImage();
                MemoryStream ms = new MemoryStream(imgBytes);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                imgBarcode.Source = bitmapImage;
                System.Windows.Forms.Application.DoEvents();
                //}));
                //----------------------------------------------------------
            }
        }

        private RunThreadMan _getAllServiceThreadWorker = null;
        private void GetAllService_Click(object sender, RoutedEventArgs e)
        {
            _getAllServiceThreadWorker?.AbortRequest(out _, 500);

            CboPaymentService.Items.Clear();
            CboPaymentServiceXCancel.Items.Clear();

            _getAllServiceThreadWorker = new RunThreadMan(new ThreadStart(GetAllServiceThreadWorker), "MainWindow.GetAllService_Click", 90, "WpfBoostTouchNGoClient");

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void GetAllServiceThreadWorker()
            {
                try
                {
                    _msg.ShowMessage("");
                    bool dataFound = false;

                    if (App.NetClientSvc.BTnGService.QueryAllPaymentGateway(out BTnGGetPaymentGatewayResult payGateResult, out bool isServerResponded) == true)
                    {
                        this.Dispatcher.Invoke(new Action(() => {
                            foreach (BTnGPaymentGatewayDetailModel payGate in payGateResult.Data.PaymentGatewayList)
                            {
                                dataFound = true;
                                CboPaymentService.Items.Add(new ComboBoxItem() { Content = $@"{payGate.PaymentGatewayName} - {payGate.PaymentGateway}", Uid = payGate.PaymentGateway });
                                CboPaymentServiceXCancel.Items.Add(new ComboBoxItem() { Content = $@"{payGate.PaymentGatewayName} - {payGate.PaymentGateway}", Uid = payGate.PaymentGateway });
                            }
                        }));
                    }

                    if (dataFound)
                        _msg.ShowMessage("GetAllServiceThreadWorker : Payment Gateway List found");
                    else
                        _msg.ShowMessage("GetAllServiceThreadWorker : No Payment Gateway has found");
                }
                catch (ThreadAbortException ex2)
                {
                    _msg.ShowMessage("GetAllServiceThreadWorker : Query Aborted");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
                finally
                {
                    _msg.ShowMessage("");
                }
            }
        }

        private void GetSvrTime_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                App.NetClientSvc.BTnGService.TestGetServerTimeRequest(_saleTrans.MerchantTransactionNo);
                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private void SendEcho_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                App.NetClientSvc.BTnGService.TestSendEchoRequest(_saleTrans.MerchantTransactionNo, TxtEchoMessage.Text);
                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private void CancelSale_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _msg.ShowMessage($@"CancelSale_Click .. ..");

                if (SetEndTransaction(out _))
                {
                    _saleTrans.IsSuccess = false;

                    _endTransCountDown.ChangeCountDown("ENDING", 30, 700);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMessage.Text = "CANCELED";
                        TxtMessage.Visibility = Visibility.Visible;
                        TxtInProgressMsg.Text = "Payment Transaction has been canceled";
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                        _saleTrans.MerchantTransactionNo, _saleTrans.SalesTransactionNo,
                        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                        _saleTrans.PaymentGateway, _saleTrans.Amount);

                    TxtCountDown.Text = "END";
                    EndSale();
                }
                else
                    _msg.ShowMessage($@"Unable to cancel sale. Sale has paid successfully");
                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);

                _msg.ShowMessage($@"CancelSale_Click .. #");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private void EndSale()
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    BtnNewSale.IsEnabled = true;
                    BtnNewDocNo.IsEnabled = true;
                    BtnShowSvrTime.IsEnabled = false;
                    BtnSendEcho.IsEnabled = false;
                    //BtnCancelSale.IsEnabled = false;

                    imgBarcode.Source = null;
                    System.Windows.Forms.Application.DoEvents();
                }));

                _msg.ShowMessage($@".. End Payment");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"{ex.ToString()} ");
            }
        }


        private RunThreadMan _createNewSaleThreadWorker = null;
        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            _isTransactionEnd = false;
            _endTransCountDown.ResetCounter();

            TxtCountDown.Text = "..";
            _endTransCountDown.ChangeCountDown("ENDING", 90, 700);

            _createNewSaleThreadWorker?.AbortRequest(out _, 300);
            _createNewSaleThreadWorker = new RunThreadMan(new ThreadStart(CreateNewSaleThreadWorking), "MainWindow.CreateNewSale_Click", 60, "WpfBoostTouchNGoClient");

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void CreateNewSaleThreadWorking()
            {
                string docNo = null;
                decimal amount = 0.0M;
                string payGate = null;
                string currency = "MYR";
                string firstName = null;
                string lastName = null;
                string contactNo = null;

                _msg.ShowMessage("");
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (CboPaymentService.SelectedItem is ComboBoxItem cItm)
                        {
                            /*By Pass*/
                        }
                        else
                            throw new Exception("Please select a Payment Service");

                        _msg.ShowMessage("Payment Start ..");

                        CboPaymentServiceXCancel.SelectedIndex = CboPaymentService.SelectedIndex;

                        docNo = TxtDocNo.Text.Trim();
                        amount = decimal.Parse(TxtAmount.Text);
                        payGate = cItm.Uid;
                        firstName = "Testing Chong";
                        lastName = "Testing Chong";
                        contactNo = "0171234567";
                        currency = "MYR";

                        BtnCancelSale.IsEnabled = false;
                        TxtSnRClientId.Text = "";
                        TxtInProgressMsg.Text = "Starting eWallet request ..";
                        TxtCountDown.Text = "..";

                        TxtError.Text = "";
                        TxtError.Visibility = Visibility.Collapsed;

                        TxtMessage.Visibility = Visibility.Visible;
                        TxtMessage.Text = "Request BTnG Payment..";

                        System.Windows.Forms.Application.DoEvents();
                    }));

                    ResetPaymentTransData();
                    _saleTrans.PaymentGateway = payGate;
                    _saleTrans.Currency = "MYR";

                    if ((App.NetClientSvc.BTnGService.PaymentMakeNewPaymentRequest(
                        docNo, amount, payGate, currency,
                        firstName, lastName, contactNo,
                        out bool isServerResponded, 50) == true)
                        )
                    {
                        _msg.ShowMessage("Payment should be started now ..");
                    }
                    else
                        throw new Exception("Unable to start 'Boost/Touch n Go'; (EXITxx)");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _isTransactionEnd = true;
                        //TxtCountDown.Text = "FAIL";

                        TxtMessage.Text = "-";
                        TxtMessage.Visibility = Visibility.Collapsed;
                        TxtError.Text = $@"{ex.Message}; (EXITxx)";
                        TxtError.Visibility = Visibility.Visible;
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    _endTransCountDown.ChangeCountDown("ENDING", 10, 700);
                }
                finally
                {
                    _msg.ShowMessage("");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtDocNo.Text = $@"MyDocNo_X{DateTime.Now:MMdd_HHmmxss}";
        }

        private void NewDocNo_Click(object sender, RoutedEventArgs e)
        {
            TxtDocNo.Text = $@"MyDocNo_X{DateTime.Now:MMdd_HHmmxss}";
        }

        private void BtnDuplicateSaleX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtDocNoXCancel.Text = TxtDocNo.Text.Trim();
                TxtBTnGSaleTransNoXCancel.Text = TxtBTnGSaleTransNo.Text.Trim();
                TxtAmountXCancel.Text = TxtAmount.Text.Trim();
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error; {ex.ToString()}");
            }
        }

        private void CancelSaleXCancel_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                decimal cancelAmount = 0.0M;

                if (CboPaymentServiceXCancel.SelectedItem is ComboBoxItem cItm)
                { /*By Pass*/ }
                else
                    throw new Exception("Please select a Payment Service for Manual Cancel sale transaction");

                if (string.IsNullOrWhiteSpace(TxtDocNoXCancel.Text))
                    throw new Exception("Please select enter document number");

                else if (string.IsNullOrWhiteSpace(TxtBTnGSaleTransNoXCancel.Text))
                    throw new Exception("Please select enter BTnG Sale Trans. No.");

                else if (decimal.TryParse(TxtAmountXCancel.Text, out cancelAmount) == false)
                    throw new Exception("Please enter valid currenct amount");

                else if (cancelAmount <= 0)
                    throw new Exception("Please enter valid currenct amount in positive value");

                _msg.ShowMessage($@"Manual cancel sale transaction .. ..");

                _msg.ShowMessage($@"ProcessId: {TxtBTnGSaleTransNoXCancel.Text.Trim()}; Doc.No.: {TxtDocNoXCancel.Text.Trim()}; BTnG Sale Trans.No.: {TxtBTnGSaleTransNoXCancel.Text.Trim()}; Currency: {"MYR"}; Payment Gateway: {cItm.Uid}; Amount: {cancelAmount}");

                App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                    TxtDocNoXCancel.Text.Trim(), TxtBTnGSaleTransNoXCancel.Text.Trim(),
                    TxtDocNoXCancel.Text.Trim(), "MYR",
                    cItm.Uid, cancelAmount);

                _msg.ShowMessage($@"Command of Manual Cancel Sale Transaction has been sent");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when Manual Cancel Sale Transaction. {ex.ToString()}");
            }
            return;
        }
    }

}