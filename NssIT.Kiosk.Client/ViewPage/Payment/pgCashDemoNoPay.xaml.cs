using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for pgCashDemoNoPay.xaml
    /// </summary>
    public partial class pgCashDemoNoPay : Page, ICash
    {
        public event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        private string _logChannel = "CashPayment";
        private string _currProcessId = "-";
        private string _docNo = "-";
        //private string _lblPleasePayLableContentStr = "";
        private decimal _amount = 0.00M;
        private string _currency = "RM";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;

        //private INetMediaInterface _netInterface;
        //private NetClientCashPaymentService _cashMachineService = null;

        private Brush _BtnCancelSalesEnabledBackground = null;
        private Brush _BtnCancelSalesDisabledBackground = new SolidColorBrush(Color.FromRgb(128, 128, 128));

        private bool _allowedCancelSales = true;

        private Style _buttonEnabledStyle = null;
        private Style _buttonDisabledStyle = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgCashDemoNoPay(INetMediaInterface netInterface, NetClientCashPaymentService cashMachineService)
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");

            _buttonEnabledStyle = GrdMain.FindResource("btnCancel") as Style;
            _buttonDisabledStyle = GrdMain.FindResource("btnCancelDisabled") as Style;

            _BtnCancelSalesEnabledBackground = BtnCancel.Background;
            //_netInterface = netInterface;
            //_cashMachineService = cashMachineService;
            _log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
        }

        public void InitSalesPayment(string processId, decimal amount, string docNo, LanguageCode language, string currency)
        {
            _language = language;
            _amount = (amount < 0) ? 0.00M : amount;
            _currProcessId = processId ?? "-";
            _docNo = docNo ?? "-";
            _currency = currency;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => {

                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                ChangeScreenSize();
                InitApp();
                //_lblPleasePayLableContentStr = (string)lblPleasePayLable.Content;
            }), null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => {
                Shutdown();
            }), null);
        }

        public void ClearEvents()
        {
            if (OnEndPayment != null)
            {
                Delegate[] delgList = OnEndPayment.GetInvocationList();
                foreach (EventHandler<EndOfPaymentEventArgs> delg in delgList)
                    try
                    {
                        OnEndPayment -= delg;
                    }
                    catch { }
            }
        }

        private void InitApp()
        {
            //if (_netInterface != null)
            //{
            //try
            //{
            //    _netInterface.OnDataReceived -= _netInterface_OnDataReceived;
            //}
            //catch { }

            _demoPaid = false;
            _allowedCancelSales = true;
            BtnCancel.Style = _buttonEnabledStyle;

            TxtError.Visibility = Visibility.Collapsed;

            TxtTotalTicketAmount.Text = $@"RM {_amount:#,##0.00}";
            TxtPaidAmount.Text = $@"RM 0.00";
            TxtBalance.Text = $@"RM {_amount:#,##0.00}";
            TxtCountDown.Text = $@"(300)";
            TxtProgressMsg.Text = "Please wait, system in progress..";

            Img1RMTick.Visibility = Visibility.Visible;
            Img5RMTick.Visibility = Visibility.Visible;
            Img10RMTick.Visibility = Visibility.Visible;
            Img20RMTick.Visibility = Visibility.Visible;
            Img50RMTick.Visibility = Visibility.Visible;

            Img1RMXX.Visibility = Visibility.Collapsed;
            Img5RMXX.Visibility = Visibility.Collapsed;
            Img10RMXX.Visibility = Visibility.Collapsed;
            Img20RMXX.Visibility = Visibility.Collapsed;
            Img50RMXX.Visibility = Visibility.Collapsed;

            //_netInterface.OnDataReceived += _netInterface_OnDataReceived;
            //_cashMachineService.StartPayment(_currProcessId, _amount, _docNo);

            // ShowCountdownMessage
            //TxtCountDown.Text = $@"60";

            //}
        }

        //private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e?.ReceivedData?.Module == AppModule.UIPayment)
        //    {
        //        switch (e?.ReceivedData.Instruction)
        //        {
        //            case CommInstruction.UIPaymShowCountdownMessage:
        //                //ShowCountdownMessage(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowCustomerMessage:
        //                //ShowCustomerMessage(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowProcessingMessage:
        //                //ShowProcessingMessage(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowOutstandingPayment:
        //                //ShowOutstandingPayment(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowRefundPayment:
        //                //ShowRefundStatus(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowBanknote:
        //                //ShowBanknote(e.ReceivedData);
        //                break;
        //            case CommInstruction.ShowErrorMessage:
        //                //ShowErrorMessage(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymShowForm:
        //                //InitCashPaymentPage(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymHideForm:
        //                HideForm(e.ReceivedData);
        //                break;
        //            case CommInstruction.UIPaymSetCancelPermission:
        //                //SetCancelPermission(e.ReceivedData);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

        //private void ShowCountdownMessage(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UICountdown cntDown)
        //        {
        //            if (cntDown.CountdownMsg.IndexOf("*") >= 0)
        //                DisableCancelButton();

        //            else if (int.TryParse(cntDown.CountdownMsg, out int countD))
        //            {
        //                if (countD == 0)
        //                    DisableCancelButton();
        //            }

        //            TxtCountDown.Text = $@"({cntDown.CountdownMsg})";
        //        }
        //    }), null);
        //}

        //private void ShowCustomerMessage(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UICustomerMessage custMsg)
        //        {
        //            //if (custMsg.CustmerMsg != null)
        //            //    txtCustomerMsg.Text = custMsg.CustmerMsg;

        //            //if ((custMsg.DisplayCustmerMsg != UIVisibility.VisibleNotChanged))
        //            //    txtCustomerMsg.Visibility = (custMsg.DisplayCustmerMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;
        //        }
        //    }), null);
        //}

        //private void ShowProcessingMessage(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UIProcessingMessage procMsg)
        //        {
        //            if (procMsg.ProcessMsg != null)
        //                TxtProgressMsg.Text = procMsg.ProcessMsg;
        //        }
        //    }), null);
        //}

        //private void ShowOutstandingPayment(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UIOutstandingPayment outPay)
        //        {
        //            //if (outPay.CustmerMsg != null)
        //            //    txtCustomerMsg.Text = outPay.CustmerMsg;

        //            //if (outPay.ProcessMsg != null)
        //            //    txtProcessingMsg.Text = outPay.ProcessMsg;

        //            TxtPaidAmount.Text = $@"RM {outPay.PaidAmount:#,##0.00}";

        //            if (outPay.IsPaymentDone == false)
        //                TxtBalance.Text = $@"RM {outPay.OutstandingAmount:#,##0.00}";

        //            else if (outPay.IsRefundRequest)
        //                TxtBalance.Text = "-";

        //            else
        //                TxtBalance.Text = "-";

        //            if (outPay.IsPaymentDone)
        //            {
        //                DisableCancelButton();
        //            }
        //        }
        //    }), null);
        //}

        //private void ShowRefundStatus(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UIRefundPayment refundStt)
        //        {
        //            //if (refundStt.CustmerMsg != null)
        //            //    txtCustomerMsg.Text = refundStt.CustmerMsg;

        //            //if (refundStt.ProcessMsg != null)
        //            //    txtProcessingMsg.Text = refundStt.ProcessMsg;

        //            TxtPaidAmount.Text = $@"RM {refundStt.PaidAmount:#,##0.00}";

        //            //lblPleasePayLable.Content = "Refund Amount :";
        //            //lblPleasePay.Content = $@"RM {refundStt.RefundAmount:#,##0.00}";

        //            TxtProgressMsg.Text = $@"Refund Amount : RM {refundStt.RefundAmount:#,##0.00}";
        //        }
        //    }), null);
        //}

        //private void ShowBanknote(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UIAcceptableBanknote bankNote)
        //        {
        //            //txtAcceptedBanknote.Text = "";
        //            string acceptNoteMsg = "";
        //            string noteStr = "";
        //            if (_language == LanguageCode.Malay)
        //                acceptNoteMsg = _langMal["ONLY_ACCEPT_BILL_Label"]?.ToString();
        //            else
        //                acceptNoteMsg = _langEng["ONLY_ACCEPT_BILL_Label"]?.ToString();

        //            Img1RMTick.Visibility = Visibility.Visible;
        //            Img5RMTick.Visibility = Visibility.Visible;
        //            Img10RMTick.Visibility = Visibility.Visible;
        //            Img20RMTick.Visibility = Visibility.Visible;
        //            Img50RMTick.Visibility = Visibility.Visible;

        //            Img1RMXX.Visibility = Visibility.Collapsed;
        //            Img5RMXX.Visibility = Visibility.Collapsed;
        //            Img10RMXX.Visibility = Visibility.Collapsed;
        //            Img20RMXX.Visibility = Visibility.Collapsed;
        //            Img50RMXX.Visibility = Visibility.Collapsed;

        //            TxtBillMsg1.Text = string.Format(acceptNoteMsg, _currency, "1, 5, 10, 20, 50");
        //        }
        //    }), null);
        //}

        //private void ShowErrorMessage(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UIError err)
        //        {
        //            if (err.ErrorMessage != null)
        //                TxtError.Text = err.ErrorMessage;

        //            if (err.DisplayErrorMsg != UIVisibility.VisibleNotChanged)
        //                TxtError.Visibility = (err.DisplayErrorMsg == UIVisibility.VisibleEnabled) ? Visibility.Visible : Visibility.Collapsed;
        //        }
        //    }), null);
        //}

        //private void InitCashPaymentPage(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        //txtCustomerMsg.Visibility = Visibility.Visible;
        //        TxtError.Visibility = Visibility.Collapsed;
        //        _allowedCancelSales = true;
        //        BtnCancel.Style = _buttonEnabledStyle;

        //        if (netMsg.MsgObject is UINewPayment newPay)
        //        {
        //            //if (newPay.CustmerMsg != null)
        //            //    txtCustomerMsg.Text = newPay.CustmerMsg;

        //            if (newPay.ProcessMsg != null)
        //                TxtProgressMsg.Text = newPay.ProcessMsg;

        //            TxtTotalTicketAmount.Text = $@"RM {newPay.Price:#,##0.00}";
        //            TxtPaidAmount.Text = $@"RM 0.00";
        //            TxtBalance.Text = $@"RM {newPay.Price:#,##0.00}";
        //            TxtCountDown.Text = $@"{newPay.InitCountDown}";
        //            //_currProcessId = newPay.ProcessId;
        //        }

        //        //this.Show();
        //        //this.ShowDialog();
        //    }), null);
        //}

        private void HideForm(NetMessagePack netMsg)
        {
            App.PaymentScrImage.CaptureScreenImage(_currProcessId);
            this.Dispatcher.Invoke(new Action(() => {
                if (netMsg.MsgObject is UIHideForm hideFm)
                {
                    if (OnEndPayment is null)
                    {
                        App.Log.LogText(_logChannel, hideFm.ProcessId, "OnEndPayment is not handled; (EXIT10000901)", "EX01",
                            "pgCash.HideForm", AppDecorator.Log.MessageType.Error);
                        App.MainScreenControl.Alert(detailMsg: $@"OnEndPayment is not handled; (EXIT10000901)");
                    }
                    else
                    {
                        Shutdown();

                        OnEndPayment.Invoke(null, new EndOfPaymentEventArgs(hideFm.ProcessId, hideFm.ResultState,
                            hideFm.Cassette1NoteCount, hideFm.Cassette2NoteCount, hideFm.Cassette3NoteCount,
                            hideFm.RefundCoinAmount));
                    }
                }
            }), null);
        }

        //private void SetCancelPermission(NetMessagePack netMsg)
        //{
        //    this.Dispatcher.Invoke(new Action(() => {
        //        if (netMsg.MsgObject is UISetCancelPermission cancPerm)
        //        {
        //            if (cancPerm.IsCancelEnabled != UIAvailability.NotChanged)
        //            {
        //                _allowedCancelSales = (cancPerm.IsCancelEnabled == UIAvailability.Enabled) ? true : false;

        //                if (_allowedCancelSales == true)
        //                    BtnCancel.Style = _buttonEnabledStyle;
        //                else
        //                    BtnCancel.Style = _buttonDisabledStyle;
        //            }
        //        }
        //    }), null);
        //}

        private bool _demoPaid = false;
        private void Paid_Click(object sender, MouseButtonEventArgs e)
        {
            if (_demoPaid)
                return;

            e.Handled = true;
            //App.PaymentScrImage.CaptureScreenImage(_currProcessId);
            //this.Dispatcher.Invoke(new Action(() => {
            if (OnEndPayment is null)
                {
                    App.Log.LogText(_logChannel, _currProcessId, "Paid_Click is not handled; (EXIT10000901C)", "EX01",
                        "pgCash.Paid_Click", AppDecorator.Log.MessageType.Error);
                    App.MainScreenControl.Alert(detailMsg: $@"OnEndPayment is not handled; (EXIT10000901C)");
                }
                else
                {
                    Shutdown();

                    OnEndPayment.Invoke(null, new EndOfPaymentEventArgs(_currProcessId, PaymentResult.Success,
                        90, 90, 90,
                        0));

                    _demoPaid = true;
                }
            //}), null);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_allowedCancelSales)
            {
                try
                {
                    DisableCancelButton();
                    //_cashMachineService.CancelTransaction();

                    //this.Dispatcher.Invoke(new Action(() => {
                        if (OnEndPayment is null)
                        {
                            App.Log.LogText(_logChannel, _currProcessId, "OnEndPayment is not handled; (EXIT10000901B)", "EX01",
                                "pgCash.BtnCancel_Click", AppDecorator.Log.MessageType.Error);
                            App.MainScreenControl.Alert(detailMsg: $@"OnEndPayment is not handled; (EXIT10000901B)");
                        }
                        else
                        {
                            Shutdown();
                            OnEndPayment.Invoke(null, new EndOfPaymentEventArgs(_currProcessId, PaymentResult.Cancel,
                                90, 90, 90,
                                0));
                        }
                    //}), null);
                }
                catch (Exception ex)
                {
                    _log.LogError(_logChannel, _currProcessId, ex, "EX01", "wfmCashPayment.BtnCancel_Click");
                }
            }
        }

        private void DisableCancelButton()
        {
            this.Dispatcher.Invoke(new Action(() => {
                _allowedCancelSales = false;
                BtnCancel.Style = _buttonDisabledStyle;
            }));
        }

        private void Shutdown()
        {
            //try
            //{
            //    _netInterface.OnDataReceived -= _netInterface_OnDataReceived;
            //}
            //catch { }
        }

        private void Page_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.ShowDebugMsg("pgCashDemoNoPay->Page_MouseLeftButtonUp : Check Event Routing; MouseLeftButtonUp routed event found; ");
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
                        BtnCancel.Height = 60;
                    }
                    _changeScreenSizeCount++;
                }
            }
        }
    }
}
