//using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
//using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
//using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
//using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
//using NssIT.Kiosk.Log.DB;
//using NssIT.Kiosk.Tools.ThreadMonitor;
using System;
using System.Collections.Generic;
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

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for pgCreditCardPayWave.xaml; For IM20
    /// </summary>
    public partial class pgCreditCardPayWave : Page, IDisposable 
    {
        private Brush _activeButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x82, 0x20));
        private Brush _activeButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _deactiveButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Brush _deactiveButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        public event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        //public delegate void CompletePayECRCallBack(TrxCallBackEventArgs e);
        //public delegate void PayWaveProgress(InProgressEventArgs e);

        //public CompletePayECRCallBack _completeECRHandle;
        //public PayWaveProgress _ECRProgress;

        private string LogChannel = "CreditCardPaymentUI";
        private const string TransCanceledTag = "Transaction Canceled";

        //private DbLog _log = null;
        private string _currProcessId = "-";
        private decimal _amount = 0M;
        private string _currency = "";

        private string _disableTag = "DISABLED";
        private const int _defaultCountDelay = 72;
        private int _delayCountDown = _defaultCountDelay;
        private const int _maxRetryCount = 3;

        private string _bankRefNo = null;
        private int _tryWaveCount = 1;
        private string _comPort = "";

        private bool _earlyAborted = false;
        private bool _allowedRetry = false;
        private bool _cancelTrans = false;
        private bool _payWaveResponseFound = false;
        private bool _exitLastProcess = false;
        private bool _endTransactionFlag = false;

        //private PayECRAccess _payWaveAccs = null;
        //public ResponseInfo SaleResult { get; private set; } = null;
        //public ResponseInfo SettlementResult { get; private set; } = null;
        public bool IsSaleSuccess { get; private set; }
        public TransactionCategory TransactionCat { get; private set; } = TransactionCategory.Sale;

        private ResourceDictionary _languageResource = null;

        private Thread _timerThreadWorker = null;
        private bool _isTimerEnabled = false;

        //public DbLog Log
        //{
        //    get
        //    {
        //        return _log ?? (_log = DbLog.GetDbLog());
        //    }
        //}

        //private PayECRAccess PayWaveAccs
        //{
        //    get
        //    {
        //        return _payWaveAccs ?? (_payWaveAccs = NewPayECRAccess());

        //        PayECRAccess NewPayECRAccess()
        //        {
        //            PayECRAccess payWvAccs = PayECRAccess.GetPayECRAccess(_comPort, PayECRAccess.SaleMaxWaitingSec); /*, @"C:\eTicketing_Log\ECR_Receipts\", @"C:\eTicketing_Log\ECR_LOG", true, true);*/
        //            payWvAccs.OnCompleteCallback += OnCompletePayECRCallBack;
        //            payWvAccs.OnInProgressCall += OnPayECRInProgressCall;
        //            return payWvAccs;
        //        }
        //    }
        //}

        private static Lazy<pgCreditCardPayWave> _creditCardPayWavePage = new Lazy<pgCreditCardPayWave>(() => new pgCreditCardPayWave());
        public static pgCreditCardPayWave GetCreditCardPayWavePage()
        {
            return _creditCardPayWavePage.Value;
        }

        private pgCreditCardPayWave()
        {
            InitializeComponent();

            _isTimerEnabled = false;
            //_timerThreadWorker = new Thread(TimerThreadWorking);
            //_timerThreadWorker.IsBackground = true;
            //_timerThreadWorker.Start();
        }

        public void InitPaymentData(string currency, decimal amount, string refNo, string comport, ResourceDictionary languageResource)
        {
            _languageResource = languageResource;
            _currency = currency ?? "*";
            _amount = amount;
            _currProcessId = (refNo ?? "").Trim();
            _comPort = (comport ?? "").Trim();

            //_firstErrorMsg = null;
            _bankRefNo = null;
            IsSaleSuccess = false;
            TransactionCat = TransactionCategory.Sale;
            _delayCountDown = _defaultCountDelay;
            _tryWaveCount = 1;
            _earlyAborted = false;
            _allowedRetry = false;
            _cancelTrans = false;
            _payWaveResponseFound = false;
            _exitLastProcess = false;
            _endTransactionFlag = false;
            //SaleResult = null;

            _debugAlreadyTestSubmitPayment = false;

            //_completeECRHandle = new CompletePayECRCallBack(OnECRComplete);
            //_ECRProgress = new PayWaveProgress(ECRWorkProg);
        }

        public void ClearEvents()
        {
            if (OnEndPayment != null)
            {
                Delegate[] delgList = OnEndPayment?.GetInvocationList();
                foreach (EventHandler<EndOfPaymentEventArgs> delg in delgList)
                    try
                    {
                        OnEndPayment -= delg;
                    }
                    catch { }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //App.IsAutoTimeoutExtension = true;

            App.TimeoutManager.ResetTimeout();

            if (_languageResource != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_languageResource);
            }

            //Disable Cancel Button
            BdCancel.Background = _deactiveButtonBackgroungColor;
            TxtName.Foreground = _deactiveButtonForegroundColor;
            //_cancelButtonEnabled = false;
            BdCancel.Tag = _disableTag;
            //--------------------------------------------------------

            TxtMacBusy.Visibility = Visibility.Visible;
            TxtError.Visibility= Visibility.Collapsed;
            TxtError.Text = "";
            
            TxtPayAmount.Text = $@"{_currency} {_amount:#,###.00}";
            TxtPayBalance.Text = $@"{_currency} {_amount:#,###.00}";

            TxtMacBusy.Text = "Sentuh (masukkan) card anda / Tap (insert) your card";
            TxtInProgress.Text = "";

            TxtTimer.Text = $@"({(_delayCountDown - 10).ToString()})";

            TxtRefNo.Text = $@"Ref.No.: {_currProcessId}    Time: {DateTime.Now: HH:mm:ss}";
            ChangeScreenSize();
            //EnableCancelButton();

            //if (App.SysParam.PrmNoPaymentNeed == false)
            //{
            //    //CYA-DEMO
            //    _isTimerEnabled = true;

            //    Log.LogText(LogChannel, _currProcessId, $@"Initiate Card App. (Amount : {_amount:#,###.00})", "A05", "pgCreditCardPayWave.Page_Loaded", 
            //                        adminMsg: $@"========== ========== Initiate Card App. (Amount : {_amount:#,###.00})========== ========== ");

            //    PayWaveAccs.Pay(_currProcessId, $@"{_amount:#,###.00}", AccountType.CreditCard, _currProcessId, null, null);
            //}

            System.Windows.Forms.Application.DoEvents();

            this.Focus();
        }

        //private void TimerThreadWorking()
        //{
        //    //_disposed = false
        //    string defaultErrorMsg = "";
        //    while (_disposed == false)
        //    {
        //        bool endFlag = false;
        //        if (_isTimerEnabled)
        //        {
        //            try
        //            {
        //                if (_delayCountDown >= 1)
        //                {
        //                    App.MarkLog.CreditCard.LogMark($@"UITimer#{_delayCountDown}");

        //                    _delayCountDown -= 1;

        //                    string runChr = "";
        //                    int tmCount = _delayCountDown - 10;
        //                    if (tmCount < 0) tmCount = 0;

        //                    if (((_delayCountDown % 2) == 0) && (tmCount == 0))
        //                        runChr = " * ";

        //                    this.Dispatcher.Invoke(() => 
        //                    {
        //                        TxtTimer.Text = $@"({tmCount.ToString()}) {runChr}";
        //                    });

        //                    if ((_delayCountDown <= 10) && (_earlyAborted == false) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
        //                    {
        //                        if (_cancelButtonEnabled)
        //                        {
        //                            DisableCancelButton();
        //                        }

        //                        if (SaleResult == null)
        //                        {
        //                            this.Dispatcher.Invoke(() =>
        //                            {
        //                                TxtTimer.Text = $@"({tmCount.ToString()}) {runChr}";
        //                                TxtMacBusy.Text = "Timeout; No customer response.";
        //                                TxtInProgress.Text = "Timeout; No customer response.";
        //                                defaultErrorMsg = "Timeout; No customer response.";
        //                                _delayCountDown = 20;
        //                            });
        //                        }

        //                        _earlyAborted = true;
        //                        PayWaveAccs.ForceToCancel();

        //                        Log.LogText(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Timeout; No customer response.", "T01", "frmPayWave.TimerThreadWorking");

        //                        _exitLastProcess = true;
        //                    }
        //                    else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
        //                    {
        //                        if (_cancelButtonEnabled)
        //                        {
        //                            DisableCancelButton();
        //                        }

        //                        if (SaleResult == null)
        //                        {
        //                            this.Dispatcher.Invoke(() =>
        //                            {
        //                                TxtMacBusy.Text = "Timeout; No customer response.";
        //                                TxtInProgress.Text = "Timeout; No customer response.";
        //                                defaultErrorMsg = "Timeout; No customer response.";
        //                                _delayCountDown = 20;
        //                            });
        //                        }

        //                        PayWaveAccs.ForceToCancel();

        //                        Log.LogText(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Timeout; No customer response.", "T02", "frmPayWave.TimerThreadWorking");

        //                        _exitLastProcess = true;
        //                    }
        //                    else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == true))
        //                    {
        //                        this.Dispatcher.Invoke(() =>
        //                        {
        //                            TxtMacBusy.Text = "Local Timeout; Unable to get response from card server.";
        //                            TxtInProgress.Text = "Local Timeout; Unable to get response from card server.";
        //                        });

        //                        defaultErrorMsg = "Local Timeout; Unable to get response from card server.";

        //                        _delayCountDown = 20;
        //                        PayWaveAccs.ForceToCancel();

        //                        Log.LogText(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Local Timeout; Unable to get response from card server", "T03", "frmPayWave.TimerThreadWorking");

        //                        _exitLastProcess = true;
        //                    }
        //                    else if ((_delayCountDown == 0) && (_payWaveResponseFound == true))
        //                    {
        //                        endFlag = true;
        //                    }

        //                }
        //                else if (_exitLastProcess == true)
        //                {
        //                    endFlag = true;
        //                    App.MarkLog.CreditCard.LogMark($@"UITimer#ENDING");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                App.MarkLog.CreditCard.LogMark($@"UITimer#ERROR");

        //                Log.LogError(LogChannel, _currProcessId, new Exception("Error when PayWave UI Process. ", ex), "EX01", "frmPayWave.TimerThreadWorking");

        //                if (defaultErrorMsg.Length > 0)
        //                    defaultErrorMsg = $@"{defaultErrorMsg} & System Error when PayWave UI Process. {ex.Message}";
        //                else
        //                    defaultErrorMsg = $@"System Error when PayWave UI Process. {ex.Message}";
        //            }
        //            finally
        //            {
        //                if (!endFlag)
        //                {
        //                    App.MarkLog.CreditCard.LogMark($@"UITimer#Next");
        //                }
        //                else if (_allowedRetry)
        //                {
        //                    _isTimerEnabled = false;
        //                    App.MarkLog.CreditCard.LogMark($@"UITimer#QUIT", true);

        //                    this.Dispatcher.Invoke(() => {
        //                        RetryPayment();
        //                    });
        //                }
        //                else if (!_allowedRetry)
        //                {
        //                    _isTimerEnabled = false;
        //                    App.MarkLog.CreditCard.LogMark($@"UITimer#QUIT", true);

        //                    App.ShowDebugMsg("End PayTimer_Tick");

        //                    if (defaultErrorMsg.Length == 0)
        //                        defaultErrorMsg = "Fail to make paywave payment. Quit 01.";

        //                    if (SaleResult == null)
        //                        SaleResult = new ResponseInfo() { ResponseMsg = "SALE", StatusCode = "99", ErrorMsg = (_firstErrorMsg ?? defaultErrorMsg) };

        //                    if (SettlementResult == null)
        //                        SettlementResult = new ResponseInfo() { ResponseMsg = "SETTLEMENT", StatusCode = "99", ErrorMsg = "Fail to make paywave payment. Quit 01." };

        //                    Log.LogText(LogChannel, _currProcessId, "Ending Paywave (1/2)", "B01", "frmPayWave.TimerThreadWorking");

        //                    this.Dispatcher.Invoke(() =>
        //                    {
        //                        TxtMacBusy.Visibility = Visibility.Visible;
        //                        TxtMacBusy.Text = "Ending Transaction .. ";
        //                        TxtInProgress.Text = "Wait for Ending Transaction.";
        //                    });

        //                    PayWaveAccs.SoftShutDown();
        //                    Log.LogText(LogChannel, _currProcessId, "Ending Paywave (2/2)", "B01", "frmPayWave.TimerThreadWorking");

        //                    if (TransactionCat == TransactionCategory.Sale)
        //                    {
        //                        if (SaleResult?.StatusCode?.Equals(ResponseStatusCode.Success) == true)
        //                        {
        //                            RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Success, SaleResult);
        //                        }
        //                        else if (_cancelTrans == true)
        //                        {
        //                            RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Cancel, null);
        //                        }
        //                        else
        //                        {
        //                            RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Fail, SaleResult);
        //                        }
        //                    }
        //                    else if (TransactionCat == TransactionCategory.Settlement)
        //                    {
        //                        if (SettlementResult?.StatusCode?.Equals(ResponseStatusCode.Success) == true)
        //                        {
        //                            RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Success, SettlementResult);
        //                        }
        //                        else
        //                        {
        //                            RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Fail, SettlementResult);
        //                        }
        //                    }
        //                    else 
        //                    {
        //                        RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Fail, null);
        //                    }
        //                }
        //            }
        //        }

        //        //if (_isTimerEnabled)
        //        //    Thread.Sleep(200);
        //        //else
        //        Thread.Sleep(1000);
        //    }
        //}

        //private string _firstErrorMsg = null;
        //private void OnECRComplete(TrxCallBackEventArgs e)
        //{
        //    _endTransactionFlag = true;
        //    ResponseInfo currResult = e.Result;
        //    _allowedRetry = false;

        //    if (currResult != null)
        //    {
        //        if (currResult.ResponseMsg == "SALE")
        //        {
        //            TransactionCat = TransactionCategory.Sale;
        //            SaleResult = currResult;
        //        }
        //        else if (currResult.ResponseMsg == "SETTLEMENT")
        //        {
        //            TransactionCat = TransactionCategory.Settlement;
        //            SettlementResult = currResult;
        //        }
        //    }

        //    if ((e.IsSuccess)
        //        && ((currResult.ResponseMsg == "SALE") || (currResult.ResponseMsg == "SETTLEMENT"))
        //        )
        //    {
        //        if (currResult.ResponseMsg == "SALE")
        //        {
        //            TxtMacBusy.Text = "Sale is successful.";
        //            TxtInProgress.Text = "Sale is successful.";
        //            IsSaleSuccess = true;

        //            // Below logic only valid if no need immediate SETTLEMENT
        //            _exitLastProcess = true;
        //            _delayCountDown = 3;
        //            //--------------------------------

        //            string cdNo = (currResult.CardNo ?? "*").Trim();

        //            if (cdNo.Length >= 4)
        //                cdNo = cdNo.Substring((cdNo.Length - 4));

        //            Log.LogText(LogChannel, _currProcessId, $@"Card Transaction Success with last 4 digit numbers #{cdNo}#", "A05", "pgCreditCardPayWave.Page_Loaded",
        //                            adminMsg: $@"Card Transaction Success with last 4 digit numbers #{cdNo}#");

        //        }
        //        else if (currResult.ResponseMsg == "SETTLEMENT")
        //        {
        //            TxtInProgress.Text = "Settlement Completed";
        //            _exitLastProcess = true;
        //            _delayCountDown = 3;
        //        }
        //    }
        //    else
        //    {
        //        string errMsg = "";

        //        if (IsSaleSuccess)
        //        {
        //            TxtMacBusy.Text = "Sale is successful.";
        //            TxtInProgress.Text = "Sale is successful.";
        //        }
        //        else
        //        {
        //            if ((currResult != null) && ((currResult.StatusCode ?? "").Length > 0))
        //            {
        //                if ((_cancelTrans) && (currResult.StatusCode.Equals(ResponseStatusCode.TA)))
        //                    errMsg = TransCanceledTag;

        //                else if (currResult.StatusCode.Equals(ResponseStatusCode.Z1) || currResult.StatusCode.Equals(ResponseStatusCode.Z3))
        //                {
        //                    if (_tryWaveCount < _maxRetryCount)
        //                        _allowedRetry = true;

        //                    errMsg = currResult.ErrorMsg + $@". Try Wave Count : {_tryWaveCount.ToString()}";
        //                    currResult.ErrorMsg = errMsg;
        //                }
        //                else
        //                    errMsg = currResult.ErrorMsg;
        //            }
        //            else if ((e.Error != null) && ((e.Error.Message ?? "").Trim().Length > 0))
        //                errMsg = e.Error.Message;

        //            else
        //                errMsg = "System encounter error at the moment. Please try later.";

        //            _firstErrorMsg = _firstErrorMsg ?? errMsg;

        //            if (currResult != null)
        //                currResult.ErrorMsg = errMsg;

        //            TxtMacBusy.Visibility = Visibility.Collapsed;
        //            TxtError.Text = errMsg;
        //            TxtError.Visibility = Visibility.Visible;
        //            DisableCancelButton();
        //        }

        //        // Delay 12 seconds for exit.
        //        // Enable Cancel Button for exit immediately.
        //        _exitLastProcess = true;

        //        if (_allowedRetry)
        //            _delayCountDown = 5;
        //        else if (errMsg.Equals(TransCanceledTag))
        //            _delayCountDown = 3;
        //        else
        //            _delayCountDown = 10;
        //    }

        //}

        //private void ECRWorkProg(InProgressEventArgs e)
        //{
        //    if (_payWaveResponseFound == false)
        //    {
        //        if (_endTransactionFlag == false)
        //            _delayCountDown = PayECRAccess.SaleMaxWaitingSec + 30;

        //        _payWaveResponseFound = true;

        //        _cancelTrans = false;
        //        _exitLastProcess = false;
        //        _earlyAborted = false;
        //        _allowedRetry = false;
        //    }

        //    if (!IsSaleSuccess)
        //        TxtMacBusy.Text = "Processing ..";

        //    string msgX = e.Message ?? "..";

        //    TxtInProgress.Text = msgX;

        //    if (_cancelButtonEnabled)
        //    {
        //        DisableCancelButton();
        //    }
        //}

        //private void OnCompletePayECRCallBack(object sender, TrxCallBackEventArgs e)
        //{
        //    if (_completeECRHandle != null)
        //        this.Dispatcher.Invoke(_completeECRHandle, e);
        //}

        //private void OnPayECRInProgressCall(object sender, InProgressEventArgs e)
        //{
        //    if (_ECRProgress != null)
        //        this.Dispatcher.Invoke(_ECRProgress, e);
        //}

        private void Page_Unloaded(object sender, RoutedEventArgs e) 
        { 
        //{
        //    App.MarkLog.CreditCard.LogMark("#pgCard.PageUnloaded>>A01#", true);

        //    if (_payWaveAccs != null)
        //    {
        //        _payWaveAccs.OnCompleteCallback -= OnCompletePayECRCallBack;
        //        _payWaveAccs.OnInProgressCall -= OnPayECRInProgressCall;

        //        //_payWaveAccs.Dispose();
        //        _payWaveAccs = null;
        //    }

        //    App.IsAutoTimeoutExtension = false;
        }

        private void Cancel_Click(object sender, MouseButtonEventArgs e)
        {
            //if (IsCancelAlreadyEnabled)
            //{
            //    App.MarkLog.CreditCard.LogMark("#pgCard.CancelClick>>A01#");
            //    DisableCancelButton();
            //    _cancelTrans = true;
            //    PayWaveAccs.CancelRequest();

            //    if (App.SysParam.PrmNoPaymentNeed)
            //    {
            //        _delayCountDown = 3;
            //        RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Cancel, null);
            //    }
            //    else
            //    {
            //        _delayCountDown = 25;
            //    }
            //    App.MarkLog.CreditCard.LogMark("#pgCard.CancelClick>>A20#");
            //}
        }

        //private void RaiseOnEndPayment(AppDecorator.Common.PaymentResult paymentResult, ResponseInfo cardResponseResult)
        //{
        //    try
        //    {
        //        string bankRefNo = null;

        //        if (cardResponseResult != null)
        //            bankRefNo = cardResponseResult?.RRN;

        //        OnEndPayment?.Invoke(null, new EndOfPaymentEventArgs(_currProcessId, paymentResult, bankRefNo, SaleResult));
        //    }
        //    catch (Exception ex)
        //    {
        //        App.Log.LogText(LogChannel,
        //            _currProcessId, 
        //            new WithDataException($@"OnEndPayment is not handled; (EXIT10000930); {ex.Message}", ex, new List<object>(new object[] {paymentResult, cardResponseResult})), 
        //            "EX01", "pgCreditCardPayWave.RaiseOnEndPayment", AppDecorator.Log.MessageType.Error,
        //            adminMsg: $@"{ex.Message}; (EXIT10000930)");

        //        App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000930)");
        //    }
        //}

        //private void RetryPayment()
        //{
        //    //"Cuba lagi. Sentuh card anda / Try again. Tap your card";
        //    _tryWaveCount += 1;
        //    _delayCountDown = _defaultCountDelay;
        //    _exitLastProcess = false;
        //    _payWaveResponseFound = false;
        //    _bankRefNo = null;
        //    _earlyAborted = false;
        //    _allowedRetry = false;
        //    _cancelTrans = false;
        //    _endTransactionFlag = false;
        //    SaleResult = null;
        //    IsSaleSuccess = false;
        //    TransactionCat = TransactionCategory.Sale;

        //    DisableCancelButton();

        //    TxtError.Visibility = Visibility.Collapsed;
        //    TxtError.Text = "";
        //    EnableCancelButton();

        //    TxtPayAmount.Text = $@"{_currency} {_amount:#,###.00}";
        //    TxtPayBalance.Text = $@"{_currency} {_amount:#,###.00}";

        //    TxtMacBusy.Text = $"Cuba lagi. Sentuh (masukkan) card anda / Try again. Tap (insert) your card. Try:{_tryWaveCount.ToString()} of 3";
        //    TxtMacBusy.Visibility = Visibility.Visible;
        //    TxtInProgress.Text = "";

        //    TxtTimer.Text = $@"({(_delayCountDown - 10).ToString()})";

        //    System.Windows.Forms.Application.DoEvents();

        //    PayWaveAccs.Pay(_currProcessId, $@"{_amount:####.00}", AccountType.CreditCard, _currProcessId, null, null);

        //    _isTimerEnabled = true;
        //}

        //private bool IsCancelAlreadyEnabled
        //{
        //    get
        //    {
        //        bool ret = false;

        //        if (BdCancel.Tag is null)
        //            ret = true;
        //        else if (BdCancel.Tag.Equals(_disableTag))
        //            ret = false;
        //        else
        //            ret = true;
        //        return ret;
        //    }
        //}

        //private bool _cancelButtonEnabled = false;
        //private object _cancellingLock = new object();
        //private void DisableCancelButton()
        //{
        //    Exception err = null;

        //    App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A01#");
        //    RunThreadMan tMan = new RunThreadMan(new Action(() => 
        //    {
        //        lock (_cancellingLock)
        //        {
        //            App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A01#");

        //            try
        //            {
        //                App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A05#");

        //                this.Dispatcher.Invoke(() =>
        //                {
        //                    BdCancel.Background = _deactiveButtonBackgroungColor;
        //                    TxtName.Foreground = _deactiveButtonForegroundColor;
        //                    _cancelButtonEnabled = false;
        //                    BdCancel.Tag = _disableTag;
        //                    System.Windows.Forms.Application.DoEvents();
        //                });
        //                App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A10#");
        //            }
        //            catch (ThreadAbortException) { }
        //            catch (Exception ex)
        //            {
        //                App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>ErrA15#");
        //                err = ex;
        //            }
        //        }
        //    }), "pgCreditCardPayWave.DisableCancelButton", 20, LogChannel);
        //    tMan.WaitUntilCompleted(waitWithWindowsDoEvents : true, sleepInterval: 50);

        //    App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A100#", true);

        //    if (err != null)
        //        throw err;
        //}

        //private Thread _delayCancelActivationThreadWorker = null;
        //private void EnableCancelButton()
        //{
        //    App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A01#");

        //    _cancelButtonEnabled = true;
        //    EndOldCancelActivationThreadWorker();

        //    _delayCancelActivationThreadWorker = new Thread(new ThreadStart(delayCancelActivationThreadWorking));
        //    _delayCancelActivationThreadWorker.IsBackground = true;
        //    _delayCancelActivationThreadWorker.Start();

        //    return;

        //    /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        //    void delayCancelActivationThreadWorking()
        //    {
        //        App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A03#");

        //        DateTime startTime = DateTime.Now;
        //        DateTime endTime = startTime.AddSeconds(7);

        //        while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
        //        {
        //            Thread.Sleep(500);

        //            if (_payWaveAccs is null)
        //                break;
        //        }

        //        App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A05#");

        //        if (_payWaveAccs != null)
        //        {
        //            App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A10#");

        //            lock (_cancellingLock)
        //            {
        //                App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A15#");

        //                try
        //                {
        //                    if (_cancelButtonEnabled)
        //                    {
        //                        App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A20#");

        //                        this.Dispatcher.Invoke(new Action(() => {
        //                            BdCancel.Background = _activeButtonBackgroungColor;
        //                            TxtName.Foreground = _activeButtonForegroundColor;
        //                            BdCancel.Tag = "";
        //                            System.Windows.Forms.Application.DoEvents();
        //                        }));
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>ErrA25#");

        //                    App.Log.LogText(LogChannel, _currProcessId, ex, "EX01",
        //                        "pgCreditCardPayWave.delayCancelActivationThreadWorking", AppDecorator.Log.MessageType.Error);
        //                }

        //                App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A30#");
        //            }

        //            App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A100#", true);
        //        }
        //    }

        //    void EndOldCancelActivationThreadWorker()
        //    {
        //        if (_delayCancelActivationThreadWorker != null)
        //        {
        //            if (((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
        //                && ((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
        //            {
        //                try
        //                {
        //                    _delayCancelActivationThreadWorker.Abort();
        //                    Thread.Sleep(100);
        //                }
        //                catch { }
        //                finally
        //                {
        //                    _delayCancelActivationThreadWorker = null;
        //                }
        //            }
        //        }
        //    }
        //}

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        private bool _debugAlreadyTestSubmitPayment = false;
        private void Test_SubmitPayment(object sender, MouseButtonEventArgs e)
        {
            //if (App.SysParam.PrmNoPaymentNeed)
            //{
            //    //CYA-DEMO
            //    if (_debugAlreadyTestSubmitPayment == false)
            //    {
            //        SaleResult = null;
            //        _bankRefNo = DateTime.Now.ToString("yyyyMMddHHmmss");
            //        _debugAlreadyTestSubmitPayment = true;
            //        RaiseOnEndPayment(AppDecorator.Common.PaymentResult.Success, SaleResult);
            //    }
            //}
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        public enum TransactionCategory
        {
            Sale = 0,
            Settlement = 1
        }
        private int _changeScreenSizeCount = 0;

        private void ChangeScreenSize()
        {
            if(App.MainScreenControl.QueryWindowSize(out double winWidth, out double winHeight) == true) 
            {
                if( _changeScreenSizeCount <= 3)
                {
                    if(winHeight > 1500)
                    {
                        BdCancel.Height = 60;

                    }
                    _changeScreenSizeCount++;
                }

                

            }
        }
    }
}
