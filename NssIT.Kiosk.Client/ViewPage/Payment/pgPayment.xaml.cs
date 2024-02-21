using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
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
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using Microsoft.Reporting.WinForms;
using System.Data;
using System.IO;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using static NssIT.Kiosk.Client.ViewPage.Payment.pgPaymentTypes;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using System.Data.Entity.Infrastructure;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public delegate void StartCashPaymentDelg();

    public delegate void StartCreditCardPaymentDelg();
    public delegate void StartBTngPaymentDelg(string paymentGateway, string logoUrl, string paymentMethod);
    public delegate bool ShowPaymentTypeSelectionDelg();
    public delegate void CancelPaymentDelg();

    /// <summary>
    /// Interaction logic for pgPayment.xaml
    /// </summary>
    public partial class pgPayment : Page, IPayment
    {
        private string _logChannel = "Payment";

        //private string _terminalLogoPath = @"file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo.jpg";
        //private string _terminalVerticalLogoPath = @"file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo_Vertical.jpg";
        private pgCreditCardPayWave _pgCreditCardPayWave = null;

        private pgPaymentInfo _paymentInfoPage = null;
        private ICash _cashPaymentPage = null;
        private pgPayment_BTnGPayment _bTnGPaymentStaff = null;
        private pgPaymentTypes _paymentTypesPage = null; 
        private pgPrintTicket2 _printTicketPage = null;
        private IPaymentTraffitController _paymentTraffitController = null;
        private CreditCardResponse _lastCreditCardAnswer = null;
        private decimal _departTotalPricePerTicket = 0M;
        private string _currency = "RM";
        private string _passgName = "-Bus Passenger-";
        private string _passgContact = "111111111";
        private int _noOfPssg = 0;
        private decimal _departTotalAmount = 0M;
        private decimal _returnTotalAmount = 0M;
        private decimal _totalAmount = 0M;
        private string _transactionNo = "";
        private PaymentType _lastPaymentType = PaymentType.Unknown;
        private string _lastPaymentMethodString = "C";

        private Thread _printingThreadWorker = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private ResourceDictionary _currentLang = null;

        private StartCashPaymentDelg _startCashPaymentDelgHandle = null;
        private StartCreditCardPaymentDelg _startCreditCardPaymentDelg = null;

        private StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;
        private ShowPaymentTypeSelectionDelg _showPaymentTypeSelectionDelg = null;
        private CancelPaymentDelg _cancelPaymentDelgHandle = null;

        private bool _isFirstTimesPaymentTypeSelection = false;
        private int _maxTimeUsedPerPaymentMinutes = 10;
        private int _maxPaymentTimeMinutes = 19;
        private DateTime _expirePaymentTime = DateTime.MinValue;

        public pgPayment()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");
            _currentLang = _langEng;

            _startCashPaymentDelgHandle = new StartCashPaymentDelg(StartCashPaymentDelgWorking);
            _startBTngPaymentDelgHandle = new StartBTngPaymentDelg(StartBTnGPaymentDelgWorking);
            _showPaymentTypeSelectionDelg = new ShowPaymentTypeSelectionDelg(ShowPaymentTypeSelection);
            _cancelPaymentDelgHandle = new CancelPaymentDelg(CancelPaymentDelgWorking);

            _startCreditCardPaymentDelg = new StartCreditCardPaymentDelg(StartCreditCardPaymentDelgWorking);

            if (App.SysParam.PrmNoPaymentNeed)
            {
                _cashPaymentPage = new pgCashDemoNoPay(App.NetClientSvc.NetInterface, App.NetClientSvc.CashPaymentService);
            }
            else
            {
                _cashPaymentPage = new pgCash(App.NetClientSvc.NetInterface, App.NetClientSvc.CashPaymentService);
            }

            _paymentTypesPage = new pgPaymentTypes();
            _printTicketPage = new pgPrintTicket2();
            _paymentInfoPage = new pgPaymentInfo();
            _pgCreditCardPayWave = pgCreditCardPayWave.GetCreditCardPayWavePage();
            _paymentTraffitController = _paymentTypesPage;

            _bTnGPaymentStaff = new pgPayment_BTnGPayment(this, FrmGoPay, FrmPayInfo, _printTicketPage, _showPaymentTypeSelectionDelg);

            _printTicketPage.OnDoneClick += _printTicketPage_OnDoneClick;
            _printTicketPage.OnPauseClick += _printTicketPage_OnPauseClick;
        }

        public ICash GetCashierPage()
        {
            return _cashPaymentPage;
        }

        public IBTnG GetBTnGCounterPage()
        {
            return _bTnGPaymentStaff.BTnGCounter;
        }

        private string TerminalLogoPath
        {
            get
            {
                if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
                    return $@"file://{App.ExecutionFolderPath}\Resources\LarkinSentral-Logo.png";
                else if (App.SysParam.PrmAppGroup == AppGroup.Gombak)
                    return $@"file://{App.ExecutionFolderPath}\Resources\TerminalBersepaduGombak-logo.png";
                else if (App.SysParam.PrmAppGroup == AppGroup.Klang)
                    return $@"file://{App.ExecutionFolderPath}\Resources\Klang Sentral Terminal 00.jpeg";
                else
                    return $@"file://{App.ExecutionFolderPath}\Resources\MelSenLogo.jpg";
            }
        }

        private string BCImagePathPath
        {
            get => $@"file://{App.ExecutionFolderPath}\Resources\BC.gif";
        }

        private string TerminalVerticalLogoPath
        {
            get
            {
                if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
                    return $@"file://{App.ExecutionFolderPath}\Resources\LarkinSentral-Logo_Vertical.png";

                else if (App.SysParam.PrmAppGroup == AppGroup.Gombak)
                    return $@"file://{App.ExecutionFolderPath}\Resources\TerminalBersepaduGombak-logo_Vertical.png";

                else if (App.SysParam.PrmAppGroup == AppGroup.Klang)
                    return $@"file://{App.ExecutionFolderPath}\Resources\Klang Sentral Terminal 00 - Vertical.png";

                else
                    return $@"file://{App.ExecutionFolderPath}\Resources\MelSenLogo_Vertical.jpg";
            }
        }

        private string TicketReportSourceName
        {
            get
            {
                if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
                    return "TicketLarkinSentral";
                else if (App.SysParam.PrmAppGroup == AppGroup.Gombak)
                    return "TicketTerminalBersepaduGombak";
                else if (App.SysParam.PrmAppGroup == AppGroup.Klang)
                    return "TicketKlangSentral";
                else
                    return "TicketMelakaSentralX1";
            }
        }

        private string TicketSkyWayTicketSourceName
        {
            get
            {
                return "TicketSkyWay";
            }
        }

        private string TicketErrorReportSourceName
        {
            get
            {
                if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
                    return "TicketLarkinSentralErrorMessage";
                else if (App.SysParam.PrmAppGroup == AppGroup.Gombak)
                    return "TicketTerminalBersepaduGombakErrorMessage";
                else if (App.SysParam.PrmAppGroup == AppGroup.Klang)
                    return "TicketKlangSentralErrorMessage";
                else
                    return "TicketMelakaSentralErrorMessage";
            }
        }

        private void _printTicketPage_OnDoneClick(object sender, EventArgs e)
        {
            try
            {
                // ClearPrintingThread();
                App.MainScreenControl.ShowWelcome();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment._printTicketPage_OnDoneClick");
            }
        }

        private bool _isPauseOnPrinting = false;
        private void _printTicketPage_OnPauseClick(object sender, EventArgs e)
        {
            try
            {
                _isPauseOnPrinting = true;
                //App.NetClientSvc.SalesService.PauseCountDown(out bool isServerResponded);
                SubmitPause();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment._printTicketPage_OnPauseClick");
            }
        }

        private void SubmitPause()
        {
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.PauseCountDown(out bool isServerResponded);

                    //if (isServerResponded == false)
                    //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000209)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000210)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10000210)", ex), "EX01", "pgPayment._printTicketPage_OnPauseClick");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowDebugMsg("pgPayment->Page_Loaded : Check Event Routing; Loaded event found; ");

                _isFirstTimesPaymentTypeSelection = true;
                _expirePaymentTime = DateTime.Now.AddMinutes(_maxPaymentTimeMinutes);

                _lastPaymentType = PaymentType.Unknown;
                _lastPaymentMethodString = "C";
                _isPauseOnPrinting = false;

                ClearPrintingThread();

                this.Resources.MergedDictionaries.Clear();

                if (_language == LanguageCode.Malay)
                    _currentLang = _langMal;
                else
                    _currentLang = _langEng;

                this.Resources.MergedDictionaries.Add(_currentLang);

                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();

                System.Windows.Forms.Application.DoEvents();

                _paymentInfoPage.InitPaymentInfo(_currency, _transactionNo, _departTotalPricePerTicket, _noOfPssg, _departTotalAmount, _totalAmount, _language);
                FrmPayInfo.NavigationService.Navigate(_paymentInfoPage);

                //_cashPaymentPage.InitSalesPayment(_transactionNo, _totalAmount, _transactionNo, _language, _currency);
                //FrmGoPay.NavigationService.Navigate(_cashPaymentPage);

                ChangeScreenSize();

                if ((App.AvailablePaymentTypeList?.Length == 1) && (App.CheckIsPaymentTypeAvailable(PaymentType.Cash)))
                    DoStartCashPayment();
                else
                    ShowPaymentTypeSelection();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.Page_Loaded");
            }
        }

        private bool _pageLoaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageLoaded = false;

                _cashPaymentPage?.ClearEvents();
                
                _bTnGPaymentStaff.DeactivatePayment();

                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.Page_Unloaded");
            }
        }

        private bool ShowPaymentTypeSelection()
        {
            bool isDone = false;
            try
            {
                if ((_isFirstTimesPaymentTypeSelection) || (_expirePaymentTime.Subtract(DateTime.Now).TotalMinutes >= _maxTimeUsedPerPaymentMinutes))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        FrmGoPay.Content = null;
                        FrmGoPay.NavigationService.RemoveBackEntry();

                        System.Windows.Forms.Application.DoEvents();

                        _paymentTypesPage.InitPaymentInfo(_currentLang, _expirePaymentTime.Subtract(new TimeSpan(0, _maxTimeUsedPerPaymentMinutes, 0)), 
                            _startCashPaymentDelgHandle, _startBTngPaymentDelgHandle,_startCreditCardPaymentDelg, _cancelPaymentDelgHandle);

                        FrmGoPay.NavigationService.Navigate(_paymentTypesPage);
                        isDone = true;
                        _isFirstTimesPaymentTypeSelection = false;
                    }));
                }
                else
                {
                    CancelPaymentDelgWorking();
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCashPaymentDelgWorking");
            }
            return isDone;
        }

        private void StartCashPaymentDelgWorking()
        {
            try
            {
                if (_paymentTraffitController.GetPermissionToPay())
                {
                    DoStartCashPayment();
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCashPaymentDelgWorking");
            }
        }

        private void StartCreditCardPaymentDelgWorking()
        {
            try
            {
                if (_paymentTraffitController.GetPermissionToPay())
                {
                    DoStartCreditCardPayment();
                }
            }catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCreditCardPaymentDelgWorking");

            }
        }

        private void DoStartCashPayment()
        {
            _bTnGPaymentStaff.DeactivatePayment();

            _cashPaymentPage.ClearEvents();
            _cashPaymentPage.OnEndPayment += _cashPaymentPage_OnEndPayment;

            this.Dispatcher.Invoke(new Action(() =>
            {
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();

                System.Windows.Forms.Application.DoEvents();

                _lastPaymentType = PaymentType.Cash;
                _lastPaymentMethodString = "C";
                _cashPaymentPage.InitSalesPayment(_transactionNo, _totalAmount, _transactionNo, _language, _currency);
                FrmGoPay.NavigationService.Navigate(_cashPaymentPage);
            }));
        }

        private void DoStartCreditCardPayment()
        {
            _cashPaymentPage.ClearEvents();
            _bTnGPaymentStaff.DeactivatePayment();

            this.Dispatcher.Invoke(new Action(() =>
            {
                ResourceDictionary langRec = (_language == LanguageCode.Malay) ? _langMal : _langEng;
                _pgCreditCardPayWave.ClearEvents();
                _pgCreditCardPayWave.InitPaymentData(_currency, _totalAmount, _transactionNo, App.SysParam.PrmPayWaveCOM, langRec);
                _pgCreditCardPayWave.OnEndPayment += _pgCreditCardPayWave_OnEndPayment;
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                _lastPaymentType = PaymentType.CreditCard;
                _lastPaymentMethodString = "D";

                FrmGoPay.NavigationService.Navigate(_pgCreditCardPayWave);
                System.Windows.Forms.Application.DoEvents();


            }));
        }

        private void _pgCreditCardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            string bankRefNo = "";
            bool isAllowAlert = true;
            try
            {
                bankRefNo = e.BankReferenceNo;

                //CYA-DEMO
                if (App.SysParam.PrmNoPaymentNeed)
                    bankRefNo = DateTime.Now.ToString("MMddHHmmss");

                if (_endPaymentThreadWorker != null)
                {
                    if ((_endPaymentThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    {
                        try
                        {
                            _endPaymentThreadWorker.Abort();
                            Thread.Sleep(300);
                        }
                        catch { }
                    }
                }

                isAllowAlert = false;
                _endPaymentThreadWorker = new Thread(new ThreadStart(OnEndPaymentThreadWorking));
                _endPaymentThreadWorker.IsBackground = true;
                _endPaymentThreadWorker.Start();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, _transactionNo,
                   new WithDataException($@"{ex.Message}; (EXIT10001126)", ex, e), "EX02", "pgPayment._cashPaymentPage_OnEndPayment",
                   adminMsg: $@"{ex.Message}; (EXIT10001126)");

                if (isAllowAlert)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001126)");
                }
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void OnEndPaymentThreadWorking()
            {
                try
                {
                    if(e.ResultState == PaymentResult.Success)
                    {
                        //reset counter
                        ResponseInfo respInfo = e.CardResponseResult;
                        _lastCreditCardAnswer = new CreditCardResponse()
                        {
                            adat = respInfo.AdditionalData,
                            aid = respInfo.AID,
                            apvc = respInfo.ApprovalCode,
                            bcam = respInfo.BatchCurrencyAmount,
                            bcno = respInfo.BatchNumber,
                            btct = respInfo.BatchCount,
                            camt = respInfo.CurrencyAmount,
                            cdnm = respInfo.CardholderName,
                            cdno = respInfo.CardNo,
                            cdty = respInfo.CardType,
                            erms = respInfo.ErrorMsg,
                            hsno = respInfo.HostNo,
                            mcid = respInfo.MachineId,
                            mid = respInfo.MID,
                            rmsg = respInfo.ResponseMsg,
                            rrn = respInfo.RRN,
                            stcd = respInfo.StatusCode,
                            tid = respInfo.TID,
                            trcy = respInfo.TC,
                            trdt = DateTime.Now,
                            ttce = respInfo.TransactionTrace,
                            CardToken = respInfo.CardToken
                        };

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            FrmPayInfo.Content = null;
                            FrmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_transactionNo, _language);

                            _pgCreditCardPayWave.ClearEvents();

                            FrmGoPay.Content = null;
                            FrmGoPay.NavigationService.RemoveBackEntry();
                            FrmPayInfo.NavigationService.Navigate(_printTicketPage);
                            System.Windows.Forms.Application.DoEvents();

                        }));

                        //call API
                        //App.NetClientSvc.SalesService.SubmitSalesPayment(_transactionNo, _totalAmount)
                        App.HostNumberForSettlementsTesting.Add(_lastCreditCardAnswer.hsno);

                        App.MainScreenControl.ShowWelcome();

                    }else if((e.ResultState == PaymentResult.Cancel) || (e.ResultState == PaymentResult.Fail))
                    {
                        if ((App.AvailablePaymentTypeList?.Length == 1) && (App.CheckIsPaymentTypeAvailable(PaymentType.Cash)))
                            CancelPaymentDelgWorking();
                        else
                            ShowPaymentTypeSelection();
                    }
                    else
                    {
                        CancelPaymentDelgWorking();
                    }
                }catch(ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.OnEndPaymentThreadWorking");

                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }

        }

        private void StartBTnGPaymentDelgWorking(string paymentGateWay, string paymentGatewayLogoUrl, string paymentMethod)
        {
            try
            {
                if (_paymentTraffitController.GetPermissionToPay())
                {
                    _cashPaymentPage.ClearEvents();

                    _lastPaymentType = PaymentType.PaymentGateway;
                    _lastPaymentMethodString = paymentMethod;

                    ResourceDictionary langRec = (_language == LanguageCode.Malay) ? _langMal : _langEng;

                    _bTnGPaymentStaff.StartBTnGPayment("MYR", _totalAmount, _transactionNo, paymentGateWay, _passgName, _passgName, _passgContact,
                        paymentGatewayLogoUrl, paymentMethod,
                        langRec, _language);
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartBTnGPayment");
            }
        }

        private Thread _endPaymentThreadWorker = null;
        private void _cashPaymentPage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            try
            {
                if (_endPaymentThreadWorker != null)
                {
                    if (_endPaymentThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
                    {
                        try
                        {
                            _endPaymentThreadWorker.Abort();
                            Thread.Sleep(300);
                        }
                        catch { }
                    }
                }
                _endPaymentThreadWorker = new Thread(new ThreadStart(OnEndPaymentThreadWorking));
                _endPaymentThreadWorker.IsBackground = true;
                _endPaymentThreadWorker.Start();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment._cashPaymentPage_OnEndPayment");
            }

            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.Log.LogText(_logChannel, _transactionNo, $@"End Payment for *****Ticket Selling; Booking No: {_transactionNo}", "A01", "pgPayment.OnEndPaymentThreadWorking",
                        adminMsg: $@"End Payment for *****Ticket Selling; Booking No: {_transactionNo}");

                    App.ShowDebugMsg("_cashPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        // Complete Transaction Then Print Ticket
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            FrmPayInfo.Content = null;
                            FrmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_transactionNo, _language);

                            _cashPaymentPage.ClearEvents();
                            
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            FrmGoPay.Content = null;
                            FrmGoPay.NavigationService.RemoveBackEntry();
                            FrmPayInfo.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        App.NetClientSvc.SalesService.SubmitSalesPayment(_transactionNo, _totalAmount,
                            e.Cassette1NoteCount, e.Cassette2NoteCount, e.Cassette3NoteCount, e.RefundCoinAmount, out bool isServerResponded);

                        //DEBUG-Testing .. bool isServerResponded = false;

                        if (isServerResponded == false) 
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                            string probMsg = "Local Server not responding (EXIT10000914)";
                            probMsg = $@"{probMsg}; Transation No.:{_transactionNo}";

                            App.Log.LogError(_logChannel, _transactionNo, new Exception(probMsg), "EX01", "pgPayment.OnEndPaymentThreadWorking");

                            _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                            _printingThreadWorker.IsBackground = true;
                            _printingThreadWorker.Start();
                            //PrintTicketError(_transactionNo);
                            //App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.OnEndPaymentThreadWorking");
                            //App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                        }
                    }
                    else if (
                        (e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || 
                        (e.ResultState == AppDecorator.Common.PaymentResult.Timeout) ||
                        (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        if ((App.AvailablePaymentTypeList?.Length == 1) && (App.CheckIsPaymentTypeAvailable(PaymentType.Cash)))
                            CancelPaymentDelgWorking();
                        else
                            ShowPaymentTypeSelection();
                    }
                    else
                    {
                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Cancel
                        // AppDecorator.Common.PaymentResult.Fail
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        //App.NetClientSvc.SalesService.RequestSeatRelease(_transactionNo);
                        //if (isServerResponded == false)
                        //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000913)");

                        //if (_isPauseOnPrinting == false)
                        //App.MainScreenControl.ShowWelcome();

                        CancelPaymentDelgWorking();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.OnEndPaymentThreadWorking");
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }

            void PrintErrorThreadWorking()
            {
                try
                {
                    PrintTicketError2(_transactionNo);
                    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.OnEndPaymentThreadWorking");

                    // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                }
                catch (ThreadAbortException) 
                {
                    /////PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.PrintErrorThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX03", "pgPayment.PrintErrorThreadWorking");
                }
            }
        }

        private void CancelPaymentDelgWorking()
        {
            try
            {
                App.ShowDebugMsg("pgPayment.CancelPaymentDelgWorking ..");
                try
                {
                    App.NetClientSvc.SalesService.RequestSeatRelease(_transactionNo);
                }
                catch { }
                App.MainScreenControl.ShowWelcome();
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.CancelPaymentDelgWorking");
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.CancelPaymentDelgWorking");
            }
        }

        public void InitPayment(UserSession session)
        {
            try
            {
                ClearPrintingThread();

                _language = session.Language;

                _departTotalPricePerTicket = session.DepartAdultPrice + session.DepartInsurance + session.DepartOnlineQrCharge + session.DepartTerminalCharge;
                _currency = string.IsNullOrWhiteSpace(session.DepartCurrency) ? "RM" : session.DepartCurrency.Trim();
                _noOfPssg = 0;

                if (session.PassengerSeatDetailList?.Length > 0)
                    _noOfPssg = session.PassengerSeatDetailList.Length;

                _departTotalAmount = session.DepartTotalAmount;
                _returnTotalAmount = 0M;
                _totalAmount = _departTotalAmount + _returnTotalAmount;
                _transactionNo = session.DepartSeatConfirmTransNo;

                _passgName = "-Bus Passenger-";
                _passgContact = "111111111";
                if (session.PassengerSeatDetailList?.Length > 0)
                {
                    _passgName = session.PassengerSeatDetailList[0].CustName ?? "-Bus Passenger-";
                    _passgContact = session.PassengerSeatDetailList[0].Contact ?? "111111111";
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.InitPayment");
            }
        }

        private void ClearPrintingThread()
        {
            if ((_printingThreadWorker != null) && (_printingThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
            {
                try
                {
                    _printingThreadWorker.Abort();
                }
                catch { }
                Task.Delay(300).Wait();
                _printingThreadWorker = null;
            }
        }

        public void UpdateTransCompleteStatus(UICompleteTransactionResult uiCompltResult)
        {
            if (_pageLoaded == false)
                return;

            App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
                //App.MainScreenControl.ExecMenu.HideMiniNavigator();
            }));

            _printingThreadWorker = new Thread(new ThreadStart(PrintThreadWorking));
            _printingThreadWorker.IsBackground = true;
            _printingThreadWorker.Start();

            void PrintThreadWorking()
            {
                try
                {
                    App.Log.LogText(_logChannel, (uiCompltResult?.Session?.DepartSeatConfirmTransNo) ?? "-", uiCompltResult, "A01", "pgPayment.UpdateTransCompleteStatus",
                        extraMsg: "Start - UpdateTransCompleteStatus; MsgObj: UICompleteTransactionResult");

                    //CYA-TEST .. if (uiCompltResult.ProcessState != ProcessResult.Success)
                    if (uiCompltResult.ProcessState == ProcessResult.Success)
                    {
                        if ((_lastPaymentType == PaymentType.PaymentGateway) && (string.IsNullOrWhiteSpace(uiCompltResult.Session?.PaymentRefNo) == false))
                            App.NetClientSvc.BTnGService.SendSuccessPaymentGatewayLog(uiCompltResult.Session.PaymentRefNo);

                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: true, language: _language);

                        //DEBUG-Testing 
                        //transcomplete_status stt = (transcomplete_status)uiCompltResult.MessageData;
                        //transcomplete_status stt = ReportDataQuery.GetTicketTestData01();
                        //UICompleteTransactionResult uiCompltResult2 = new UICompleteTransactionResult(uiCompltResult.RefNetProcessId, uiCompltResult.ProcessId, DateTime.Now, stt, ProcessResult.Success);
                        //uiCompltResult2.UpdateSession(uiCompltResult.Session);
                        //uiCompltResult = uiCompltResult2;
                        //----------------------------------------------------------------------------------------

                        System.Windows.Forms.Application.DoEvents();
                        App.ShowDebugMsg("Printing Ticket ..; pgPayment.UpdateTransCompleteStatus");

                        PrintTicket(uiCompltResult);

                        if (_isPauseOnPrinting == false)
                            App.MainScreenControl.ShowWelcome();
                    }
                    else
                    {
                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                        System.Windows.Forms.Application.DoEvents();

                        string probMsg = (string.IsNullOrWhiteSpace(uiCompltResult.ErrorMessage) == false) ? uiCompltResult.ErrorMessage : "Fail Complete Payment Transaction";
                        probMsg = $@"{probMsg}; Transation No.:{_transactionNo}";

                        App.Log.LogError(_logChannel, _transactionNo, new Exception(probMsg), "EX01", "pgPayment.UpdateTransCompleteStatus", adminMsg: probMsg);

                        PrintTicketError(uiCompltResult);
                        App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.UpdateTransCompleteStatus");

                        if (_isPauseOnPrinting == false)
                            App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) 
                {
                    /////PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error: {ex.Message}; pgPayment.PrintThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.PrintThreadWorking");
                    //App.MainScreenControl.Alert(detailMsg: $@"Unable to read Transaction Status; (EXIT10000911)");
                }
            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentStaff.BTnGShowPaymentInfo(kioskMsg);
        }

        List<SkyWayTicketHardCode> _skyWayTicketHardCodes = new List<SkyWayTicketHardCode>();


        public void TestPrint()
        {



            _skyWayTicketHardCodes.Add(new SkyWayTicketHardCode()
            {
                Address = "LOT 1988/488, JALAN SEGAMBUT TENGAH, 51200 KUALA LUMPUR",
                ServiceTaxId = "C11-1808-320000003",
                TransactionNo = "000000000000SKY200000002",
                CompanyRegNo = "199201017704",
                TicketPrice = 10M,
                ValidFrom = DateTime.Now.ToShortDateString(),
                ValidTo = DateTime.Now.AddDays(1).ToShortDateString()
            });

            // Adding another ticket
            _skyWayTicketHardCodes.Add(new SkyWayTicketHardCode()
            {
                Address = "LOT 123, JALAN BUKIT BINTANG, 55100 KUALA LUMPUR",
                ServiceTaxId = "C11-1234-567890",
                TransactionNo = "000000000000SKY200000003",
                CompanyRegNo = "201001011234",
                TicketPrice = 15M,
                ValidFrom = DateTime.Now.AddDays(2).ToShortDateString(),
                ValidTo = DateTime.Now.AddDays(3).ToShortDateString()
            });


            Stream[] streamSkywayTicketList = null;
            ReportImageSize skyWayTicketSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);

            if (_skyWayTicketHardCodes.Count > 0)
            {
                var hardCodes = _skyWayTicketHardCodes.ToArray();

                LocalReport skyWayRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketSkyWayTicketSourceName}.rdlc",
                new ReportDataSource[] { new ReportDataSource("DataSet1", new List<SkyWayTicketHardCode>(_skyWayTicketHardCodes)) });
                streamSkywayTicketList = RdlcImageRendering.Export(skyWayRep, skyWayTicketSize);

            }

            ImagePrintingTools.InitService();

            if (_skyWayTicketHardCodes.Count > 0)
                ImagePrintingTools.AddPrintDocument(streamSkywayTicketList, "", skyWayTicketSize);

            ImagePrintingTools.ExecutePrinting("");

            _skyWayTicketHardCodes.Clear();
        }

        private void PrintTicket(UICompleteTransactionResult uiCompltResult)
        {
            UserSession session = uiCompltResult.Session;
            string transactionNo = "-";

            //Reports.RdlcRendering rpRen = null;

           
            try
            {
                transactionNo = session.DepartSeatConfirmTransNo;
                transcomplete_status transCompStt = (transcomplete_status)uiCompltResult.MessageData;

                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                //file://C:\\dev\\RND\\MyRnD\\WFmRdlcReport5\\bin\\Debug\\Resource\\MelSenLogo.jpg
                //file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo.jpg
                ////TerminalVerticalLogoPath//. TerminalLogoPath
                DsMelakaCentralTicket ds = ReportDataQuery.ReadToTicketDataSet(transactionNo, transCompStt, TerminalLogoPath, BCImagePathPath, out bool errorFound);

                //DEBUG-Testing .. errorFound = true;

                if (errorFound == false)
                {
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 1 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    /////TicketFolder
                    //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketPDFRender, App.ExecutionFolderPath + @"\Reports", 
                    //    TicketReportSourceName, transactionNo, "DataSet1", ds.Tables[0], null);

                    //App.Log.LogText(_logChannel, transactionNo, "Start to print ticket", "A02", classNMethodName: "pgPayment.PrintTicket",
                    //    adminMsg: "Start to print ticket");
                    //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);

                    //App.ShowDebugMsg("Should be printed; pgPayment.PrintTicket");

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 2 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketReportSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", ds.Tables[0]) });
                    ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);


                    ReportImageSize skyWayTicketSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);

                    Stream[] streamSkywayTicketList = null;
                    if (_skyWayTicketHardCodes.Count > 0)
                    {
                        var hardCodes = _skyWayTicketHardCodes.ToArray();

                        LocalReport skyWayRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketSkyWayTicketSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", new List<SkyWayTicketHardCode>(hardCodes)) });
                        streamSkywayTicketList = RdlcImageRendering.Export(skyWayRep, ticketSize);

                    }

                    ImagePrintingTools.InitService();
                    ImagePrintingTools.AddPrintDocument(streamticketList, transactionNo, ticketSize);

                    if(_skyWayTicketHardCodes.Count > 0)
                        ImagePrintingTools.AddPrintDocument(streamSkywayTicketList, transactionNo, ticketSize);


                    App.Log.LogText(_logChannel, transactionNo, "Start to print ticket", "A02", classNMethodName: "pgPayment.PrintTicket",
                        adminMsg: "Start to print receipt");

                    App.ShowDebugMsg("Should be printed; pgPayment.PrintTicket");

                    ImagePrintingTools.ExecutePrinting(transactionNo);
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                }
                else
                {
                    _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                    DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 1 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports", 
                    //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                    //App.Log.LogText(_logChannel, transactionNo, "Start to print error transaction", "A02", classNMethodName: "pgPayment.PrintTicket",
                    //    adminMsg: "Start to print error transaction");
                    //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                    //App.ShowDebugMsg("Should be printed (Error Msg); PrintTicket.PrintTicket");

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 2 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                    ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                    ImagePrintingTools.InitService();
                    ImagePrintingTools.AddPrintDocument(streamticketList, transactionNo, ticketSize);

                    App.Log.LogText(_logChannel, transactionNo, "Start to print error note (A)", "A02", classNMethodName: "pgPayment.PrintTicket",
                        adminMsg: "Start to print error note (A)");

                    App.ShowDebugMsg("Should be printed; PrintTicket.PrintTicket");

                    ImagePrintingTools.ExecutePrinting(transactionNo);
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                }

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicket; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.PrintTicket",
                    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        private void PrintTicketError(UICompleteTransactionResult uiCompltResult)
        {
            UserSession session = uiCompltResult.Session;
            string transactionNo = "-";

            //Reports.RdlcRendering rpRen = null;

            try
            {
                transactionNo = session.DepartSeatConfirmTransNo;
                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 1 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports", 
                //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError",
                //        adminMsg: "Start to print fail transaction notice");
                //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //App.ShowDebugMsg("Should be printed (Error Msg); PrintTicket.PrintTicketError");

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 2 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, transactionNo, ticketSize);

                App.Log.LogText(_logChannel, transactionNo, "Start to print error note (B)", "A02", classNMethodName: "pgPayment.PrintTicketError",
                    adminMsg: "Start to print error note (B)");

                App.ShowDebugMsg("Should be printed; pgPayment.PrintTicketError");

                ImagePrintingTools.ExecutePrinting(transactionNo);
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicketError; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.PrintTicketError-1",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }

        public void PrintTicketError2(string transactionNo)
        {
            //Reports.RdlcRendering rpRen = null;
            try
            {
                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 1 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports",
                //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError2",
                //        adminMsg: "Start to print fail transaction notice");

                //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //App.ShowDebugMsg("Should be printed (Error Msg); PrintTicket.PrintTicketError2");

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 2 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, transactionNo, ticketSize);

                App.Log.LogText(_logChannel, transactionNo, "Start to print error note (C)", "A02", classNMethodName: "pgPayment.PrintTicketError2",
                    adminMsg: "Start to print error note (C)");

                App.ShowDebugMsg("Should be printed; pgPayment.PrintTicketError2");

                ImagePrintingTools.ExecutePrinting(transactionNo);
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicketError2; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.PrintTicketError2",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
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
                        BdStacker1.Height = 800;
                    }
                    _changeScreenSizeCount++;
                }
            }
        }
    }
}
