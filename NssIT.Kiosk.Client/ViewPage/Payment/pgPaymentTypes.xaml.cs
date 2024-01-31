using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.Tools.Timer;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
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
using static NssIT.Kiosk.Client.ViewPage.Payment.uscPaymentGateway;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for pgPaymentTypes.xaml; refer to KTMB => pgPaymentInfo2
    /// </summary>
    public partial class pgPaymentTypes : Page, IPaymentTraffitController
    {
        private string _logChannel = "Payment";

        private Brush _activeButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x82, 0x20));
        private Brush _activeButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _deactiveButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Brush _deactiveButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        private StartCreditCardPaymentDelg _startCreditCardPaymentDelgHandle = null;
        private StartCashPaymentDelg _startCashPaymentDelgHandle = null;
        private StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;
        private CancelPaymentDelg _cancelPaymentDelgHandle = null;

        private ResourceDictionary _langResc = null;
        private WebImageCacheX _imageCache = null;

        private CountDownTimer _endTransCountDown = null;
        private bool _isPaymentAllowed = false;
        private bool _isCancelAllowed = false;
        private object _permissionToPayLock = new object();

        private DateTime _expirePaymentSelectionTime;

        public pgPaymentTypes()
        {
            InitializeComponent();
            _imageCache = new WebImageCacheX(12);

            _endTransCountDown = new CountDownTimer();
            _endTransCountDown.OnCountDown += _endTransCountDown_OnCountDown;
            _endTransCountDown.OnExpired += _endTransCountDown_OnExpired;
        }

        private RunThreadMan _loadBTnGThreadWorker = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SvPaymentType.ScrollToVerticalOffset(0);
                _imageCache.ClearCacheOnTimeout();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_langResc);

                ClearWrpPaymentGatewayContainer();

                BdLoadingeWallet.Visibility = Visibility.Collapsed;
                BdCash.Visibility = Visibility.Collapsed;
                BdDesignBoost.Visibility = Visibility.Collapsed;
                BdDesignTng.Visibility = Visibility.Collapsed;
                BdCredit.Visibility = Visibility.Collapsed;

                if (App.CheckIsPaymentTypeAvailable(PaymentType.Cash))
                    BdCash.Visibility = Visibility.Visible;

                if (App.CheckIsPaymentTypeAvailable(PaymentType.CreditCard))
                    BdCredit.Visibility = Visibility.Visible;

                if (App.CheckIsPaymentTypeAvailable(PaymentType.PaymentGateway))
                {
                    LoadBTnGOption();
                    BdLoadingeWallet.Visibility = Visibility.Visible;
                }

                _isPaymentAllowed = true;
                _isCancelAllowed = true;

                TxtExpiredTime.Text = $@"Exp.At : {_expirePaymentSelectionTime:hh:mm:ss tt}";

                ShowEnableCancelButton(true);
                ChangeScreenSize();

                _endTransCountDown.ChangeCountDown("ENDING", 30, 700);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error: {ex.Message}; pgPaymentInfo2.Page_Loaded");
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPaymentInfo2.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"Fail to start payment; (EXIT10000918)");
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void LoadBTnGOption()
            {
                _loadBTnGThreadWorker?.AbortRequest(out _, 500);
                _loadBTnGThreadWorker = new RunThreadMan(new ThreadStart(LoadBTnGOptionThreadWorking), "pgPaymentInfo2.LoadBTnGOption", 70, _logChannel);
            }

            void LoadBTnGOptionThreadWorking()
            {
                try
                {
                    bool dataFound = false;

                    if (App.NetClientSvc.BTnGService.QueryAllPaymentGateway(out BTnGGetPaymentGatewayResult payGateResult, out bool isServerResponded) == true)
                    {
                        if (payGateResult.Data?.PaymentGatewayList?.Length > 0)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                foreach (BTnGPaymentGatewayDetailModel payGate in payGateResult.Data.PaymentGatewayList)
                                {
                                    dataFound = true;

                                    uscPaymentGateway pgt = GetFreeUscPaymentGateway();
                                    pgt.InitBTnGPaymentGateway(payGate.PaymentGateway, payGate.PaymentGatewayName, payGate.Logo, _imageCache, payGate.PaymentMethod, _startBTngPaymentDelgHandle);
                                    WrpPaymentGatewayContainer.Children.Add(pgt);
                                }
                                System.Windows.Forms.Application.DoEvents();
                            }));
                        }
                    }

                    if (dataFound)
                        App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : Payment Gateway List found");
                    else
                        App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : No Payment Gateway has found");
                }
                catch (ThreadAbortException ex2)
                {
                    App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : Query Aborted");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg(ex.ToString());
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BdLoadingeWallet.Visibility = Visibility.Collapsed;
                    }));
                }
            }
        }

        private void _endTransCountDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            DoCancelPayment("On-CountDown-Expired");
        }

        private void _endTransCountDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                TxtTimer.Text = $@"({e?.TimeRemainderSec})";
            }));
        }

        public bool GetPermissionToPay()
        {
            if (_isPaymentAllowed == false)
                return false;

            bool retResult = false;

            Thread tWork = new Thread(new ThreadStart(new Action(() => 
            { 
                lock(_permissionToPayLock)
                {
                    if (_isPaymentAllowed == true) 
                    {
                        _isPaymentAllowed = false;
                        _isCancelAllowed = false;
                        retResult = true;
                    }
                }
            })));
            tWork.IsBackground = true;
            tWork.Start();
            tWork.Join();

            if (retResult)
            {
                try
                {
                    ShowEnableCancelButton(false);
                }
                catch { }
            }

            return retResult;
        }

        public void InitPaymentInfo(ResourceDictionary languageResc, DateTime expirePaymentSelectionTime,
            StartCashPaymentDelg startCashPaymentDelgHandle, StartBTngPaymentDelg startBTngPaymentDelgHandle,StartCreditCardPaymentDelg startCreditCardPaymentDelgHandle, CancelPaymentDelg cancelPaymentDelgHandle)
        {
            _langResc = languageResc;
            _expirePaymentSelectionTime = expirePaymentSelectionTime;
            _startCashPaymentDelgHandle = startCashPaymentDelgHandle;
            _startBTngPaymentDelgHandle = startBTngPaymentDelgHandle;
            _cancelPaymentDelgHandle = cancelPaymentDelgHandle;
            _startCreditCardPaymentDelgHandle = startCreditCardPaymentDelgHandle;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPaymentAllowed = false;
            _isCancelAllowed = false;
            _endTransCountDown.ResetCounter();
            /////_paymentInProgress = false;
        }

        private void Cash_Click(object sender, MouseButtonEventArgs e)
        {
            if (_startCashPaymentDelgHandle != null)
            {
                _startCashPaymentDelgHandle();
            }
        }

        private void Credit_Click(object sender, MouseButtonEventArgs e)
        {
            if(_startCreditCardPaymentDelgHandle!= null)
            {
                _startCreditCardPaymentDelgHandle();
            }
        }

        private List<uscPaymentGateway> _uscPaymentGatewayList = new List<uscPaymentGateway>();
        private uscPaymentGateway GetFreeUscPaymentGateway()
        {
            uscPaymentGateway retCtrl = null;
            if (_uscPaymentGatewayList.Count == 0)
                retCtrl = new uscPaymentGateway();
            else
            {
                retCtrl = _uscPaymentGatewayList[0];
                _uscPaymentGatewayList.RemoveAt(0);
            }
            return retCtrl;
        }
        private void ClearWrpPaymentGatewayContainer()
        {
            int nextCtrlInx = 0;
            do
            {
                if (WrpPaymentGatewayContainer.Children.Count > nextCtrlInx)
                {
                    if (WrpPaymentGatewayContainer.Children[nextCtrlInx] is uscPaymentGateway ctrl)
                    {
                        //if (ctrl.IsCreditCard == false)
                        //{
                        //ctrl.OnTextBoxGotFocus -= PassengerInfoTextBox_GotFocus;
                        WrpPaymentGatewayContainer.Children.RemoveAt(nextCtrlInx);
                        _uscPaymentGatewayList.Add(ctrl);
                        //}
                        //else
                        //    nextCtrlInx++;
                    }
                    else
                        nextCtrlInx++;
                }
            } while (WrpPaymentGatewayContainer.Children.Count > nextCtrlInx);
        }

        private void Cancel_Click(object sender, MouseButtonEventArgs e)
        {
            DoCancelPayment("On-Cancel-Click");
        }

        private void DoCancelPayment(string actionTag)
        {
            if (_isCancelAllowed == false)
                return;

            bool proceesCancel = false;

            Thread tWork = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_permissionToPayLock)
                {
                    if (_isCancelAllowed == true)
                    {
                        _isPaymentAllowed = false;
                        _isCancelAllowed = false;
                        proceesCancel = true;
                    }
                }
            })));
            tWork.IsBackground = true;
            tWork.Start();
            tWork.Join();

            if (proceesCancel)
            {
                _cancelPaymentDelgHandle?.Invoke();
            }
        }

        private void ShowEnableCancelButton(bool isEnable)
        {
            this.Dispatcher.Invoke(new Action(() => 
            { 
                if (isEnable)
                {
                    BdCancel.Background = _activeButtonBackgroungColor;
                    TxtCancelLabel.Foreground = _activeButtonForegroundColor;
                }
                else
                {
                    BdCancel.Background = _deactiveButtonBackgroungColor;
                    TxtCancelLabel.Foreground = _deactiveButtonForegroundColor;
                }
            }));
        }

        private int _changeScreenSizeCount = 0;
        private void ChangeScreenSize()
        {
            if (App.MainScreenControl.QueryWindowSize
                (out double winWidth, out double winHeight) == true)
            {
                if (_changeScreenSizeCount <= 3)
                {
                    if (winHeight > 1500)
                    {
                        BdCancel.Height = 60;
                    }
                    _changeScreenSizeCount++;
                }
            }
        }
    }
}
