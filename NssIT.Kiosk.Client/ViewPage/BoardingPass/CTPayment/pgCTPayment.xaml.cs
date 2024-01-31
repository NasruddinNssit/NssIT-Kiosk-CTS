using Microsoft.Reporting.WinForms;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Base.Time;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Common.WebService.KioskWebService;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment
{
    /// <summary>
    /// ClassCode:EXIT80.24; Interaction logic for pgCTPayment.xaml
    /// </summary>
    public partial class pgCTPayment : Page, ICTPayment
    {
        private string _logChannel = "Payment";

        private pgCTPaymentInfo _paymentInfoPage = null;
        private pgCTPayment_BTnGPaymentProxy _bTnGPayProxy = null;
        private pgCTPayment_CashPaymentProxy _cashPayProxy = null;
        private pgCTPrintTicket _printTicketPage = null;
        private pgPaymentTypes _paymentTypesPage = null;
        private IPaymentTraffitController _paymentTraffitController = null;

        private string _currency = "RM";

        private BoardingTicket _boardingTicket = null;

        private PaymentType _lastPaymentType = PaymentType.Unknown;
        private string _lastPaymentMethodString = "C";
        private bool _pageLoaded = false;

        private Thread _printingThreadWorker = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private ResourceDictionary _currentRosLang = null;
        private StartCreditCardPaymentDelg _startCreditCardPaymentDelg = null;

        private StartCashPaymentDelg _startCashPaymentDelgHandle = null;
        private StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;
        private ShowPaymentTypeSelectionDelg _showPaymentTypeSelectionDelg = null;
        private CancelPaymentDelg _cancelPaymentDelgHandle = null;

        private bool _isFirstTimesPaymentTypeSelection = false;
        private int _maxTimeUsedPerPaymentMinutes = 10;
        private int _maxPaymentTimeMinutes = 30;
        private DateTime _expirePaymentTime = DateTime.MinValue;

        /// <summary>
		/// FuncCode:EXIT80.2401
		/// </summary>
        public pgCTPayment()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentEnglish.xaml");
            _currentRosLang = _langEng;

            _startCashPaymentDelgHandle = new StartCashPaymentDelg(StartCashPaymentDelgWorking);
            _startBTngPaymentDelgHandle = new StartBTngPaymentDelg(StartBTnGPaymentDelgWorking);
            _showPaymentTypeSelectionDelg = new ShowPaymentTypeSelectionDelg(ShowPaymentTypeSelection);
            _cancelPaymentDelgHandle = new CancelPaymentDelg(CancelPaymentDelgWorking);

            _paymentTypesPage = new pgPaymentTypes();
            _printTicketPage = new pgCTPrintTicket();
            _paymentInfoPage = new pgCTPaymentInfo();
            _paymentTraffitController = _paymentTypesPage;

            _bTnGPayProxy = new pgCTPayment_BTnGPaymentProxy(this, FrmGoPay, FrmPayInfo, BdGoPay, FrmPrinting, _printTicketPage, _showPaymentTypeSelectionDelg);
            _cashPayProxy = new pgCTPayment_CashPaymentProxy(this, FrmGoPay, FrmPayInfo, BdGoPay, FrmPrinting, _printTicketPage, _showPaymentTypeSelectionDelg, _cancelPaymentDelgHandle);

            _printTicketPage.OnDoneClick += _printTicketPage_OnDoneClick;
            _printTicketPage.OnPauseClick += _printTicketPage_OnPauseClick;
        }

        /// <summary>
		/// FuncCode:EXIT80.2405
		/// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowDebugMsg("pgCTPayment->Page_Loaded : Check Event Routing; Loaded event found; ");

                _isFirstTimesPaymentTypeSelection = true;
                _expirePaymentTime = DateTime.Now.AddMinutes(_maxPaymentTimeMinutes);

                _lastPaymentType = PaymentType.Unknown;
                _lastPaymentMethodString = "C";
                _isPauseOnPrinting = false;

                ClearPrintingThread();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                BdGoPay.Visibility = Visibility.Collapsed;

                FrmPrinting.Content = null;
                FrmPrinting.NavigationService.RemoveBackEntry();

                System.Windows.Forms.Application.DoEvents();

                _paymentInfoPage.InitPaymentInfo(_boardingTicket, _language);
                FrmPayInfo.NavigationService.Navigate(_paymentInfoPage);

                //_cashPaymentPage.InitSalesPayment(_transactionNo, _totalAmount, _transactionNo, _language, _currency);
                //FrmGoPay.NavigationService.Navigate(_cashPaymentPage);

                if ((App.AvailablePaymentTypeList?.Length == 1) && (App.CheckIsPaymentTypeAvailable(PaymentType.Cash)))
                    _cashPayProxy.StartCashPayment(_currency, _boardingTicket.TotalChargableAmount, _boardingTicket.TicketNo, _currentRosLang, _language);
                else
                    ShowPaymentTypeSelection();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.Page_Loaded");
            }
        }

        public bool IsPauseOnPrinting => _isPauseOnPrinting;

        /// <summary>
		/// FuncCode:EXIT80.2410
		/// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageLoaded = false;

                _cashPayProxy.DeactivatePayment();
                _bTnGPayProxy.DeactivatePayment();

                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                BdGoPay.Visibility = Visibility.Collapsed;

                FrmPrinting.Content = null;
                FrmPrinting.NavigationService.RemoveBackEntry();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.Page_Unloaded");
            }
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

        /// <summary>
		/// FuncCode:EXIT80.2415
		/// </summary>
        private void _printTicketPage_OnDoneClick(object sender, EventArgs e)
        {
            try
            {
                // ClearPrintingThread();
                App.MainScreenControl.ShowWelcome();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment._printTicketPage_OnDoneClick");
            }
        }

        private bool _isPauseOnPrinting = false;
        /// <summary>
		/// FuncCode:EXIT80.2420
		/// </summary>
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
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment._printTicketPage_OnPauseClick");
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2425
		/// </summary>
        private void SubmitPause()
        {
            System.Windows.Forms.Application.DoEvents();

            if (App.CollectBoardingPassCountDown.ChangeCountDown("pgCTPayment.SubmitPause", CollectTicketCountDown.ColTickCountDownCode.Printing_Pause, 2 * 24 * 60 * 60) == false)
            {
                /////App.MainScreenControl.ShowWelcome();
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2430
		/// </summary>
        private bool ShowPaymentTypeSelection()
        {
            bool isDone = false;
            try
            {
                if ((_isFirstTimesPaymentTypeSelection) || (_expirePaymentTime.Subtract(DateTime.Now).TotalMinutes >= _maxTimeUsedPerPaymentMinutes))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BdGoPay.Visibility = Visibility.Visible;
                        FrmGoPay.Content = null;
                        FrmGoPay.NavigationService.RemoveBackEntry();

                        System.Windows.Forms.Application.DoEvents();

                        _paymentTypesPage.InitPaymentInfo(_currentRosLang, _expirePaymentTime.Subtract(new TimeSpan(0, _maxTimeUsedPerPaymentMinutes, 0)),
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
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.StartCashPaymentDelgWorking");
            }
            return isDone;
        }

        /// <summary>
		/// FuncCode:EXIT80.2435
		/// </summary>
        private void StartCashPaymentDelgWorking()
        {
            try
            {
                if (_paymentTraffitController.GetPermissionToPay())
                {
                    _bTnGPayProxy.DeactivatePayment();

                    _lastPaymentType = PaymentType.Cash;
                    _lastPaymentMethodString = "C";

                    _cashPayProxy.StartCashPayment(_currency, _boardingTicket.TotalChargableAmount, _boardingTicket.TicketNo, _currentRosLang, _language);
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.StartCashPaymentDelgWorking");
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2440
		/// </summary>
        private void StartBTnGPaymentDelgWorking(string paymentGateWay, string paymentGatewayLogoUrl, string paymentMethod)
        {
            try
            {
                if (_paymentTraffitController.GetPermissionToPay())
                {
                    _cashPayProxy.DeactivatePayment();

                    _lastPaymentType = PaymentType.PaymentGateway;
                    _lastPaymentMethodString = paymentMethod;

                    _bTnGPayProxy.StartBTnGPayment("MYR", _boardingTicket.TotalChargableAmount, _boardingTicket.TicketNo, paymentGateWay, 
                        _boardingTicket.PassengerName, _boardingTicket.PassengerName, _boardingTicket.PassengerContact,
                        paymentGatewayLogoUrl, paymentMethod, _currentRosLang, _language);
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.StartBTnGPayment");
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2445
		/// </summary>
        private void CancelPaymentDelgWorking()
        {
            try
            {
                App.ShowDebugMsg("pgCTPayment.CancelPaymentDelgWorking ..");
                //CYA-DEBUG ..outstanding.. Send cancel sales transaction to local server
                App.MainScreenControl.ShowWelcome();
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; EX02; pgCTPayment.CancelPaymentDelgWorking");
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgCTPayment.CancelPaymentDelgWorking");
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2450
		/// </summary>
        public void InitData(IKioskMsg kioskMsg)
        {
            try
            {
                if ((kioskMsg is IUserSession usrSession) && (usrSession.Session != null))
                {
                    /////ClearPrintingThread();
                    _language = usrSession.Session.Language;
                    _currency = "RM";
                    _boardingTicket = usrSession.Session.TicketCollection.Duplicate();
                    if (_language == LanguageCode.Malay)
                        _currentRosLang = _langMal;
                    else
                        _currentRosLang = _langEng;
                }
                else
                    throw new Exception("Unknown error; no user Session found when try to start payment; ()");
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment.InitData");
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2455
		/// </summary>
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

        /// <summary>
		/// FuncCode:EXIT80.2460
		/// </summary>
        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPayProxy.BTnGShowPaymentInfo(kioskMsg);
        }

        /// <summary>
        /// FuncCode:EXIT80.2465
        /// </summary>
        public void PrintTicket(boardingcollectticket_status uiCompltResult)
        {
            string ticketNo = "-";

            //Reports.RdlcRendering rpRen = null;

            try
            {
                ticketNo = _boardingTicket.TicketNo;

                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                //file://C:\\dev\\RND\\MyRnD\\WFmRdlcReport5\\bin\\Debug\\Resource\\MelSenLogo.jpg
                //file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo.jpg
                ////TerminalVerticalLogoPath//. TerminalLogoPath
                DsMelakaCentralTicket ds = ReportDataQuery.ReadToTicketDataSet(ticketNo, uiCompltResult, TerminalLogoPath, BCImagePathPath, out bool errorFound);

                //DEBUG-Testing .. errorFound = true;

                if (errorFound == false)
                {
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 1 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    /////TicketFolder
                    //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketPDFRender, App.ExecutionFolderPath + @"\Reports", 
                    //    TicketReportSourceName, transactionNo, "DataSet1", ds.Tables[0], null);

                    //App.Log.LogText(_logChannel, transactionNo, "Start to print ticket", "A02", classNMethodName: "pgCTPayment.PrintTicket",
                    //    adminMsg: "Start to print ticket");
                    //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);

                    //App.ShowDebugMsg("Should be printed; pgCTPayment.PrintTicket");

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 2 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketReportSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", ds.Tables[0]) });
                    ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                    ImagePrintingTools.InitService();
                    ImagePrintingTools.AddPrintDocument(streamticketList, ticketNo, ticketSize);

                    App.Log.LogText(_logChannel, ticketNo, "Start to print ticket", "A02", classNMethodName: "pgCTPayment.PrintTicket",
                        adminMsg: "Start to print receipt");

                    App.ShowDebugMsg("Should be printed; pgCTPayment.PrintTicket");

                    ImagePrintingTools.ExecutePrinting(ticketNo);
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                }
                else
                {
                    _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                    DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(ticketNo, TerminalVerticalLogoPath);

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 1 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports", 
                    //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                    //App.Log.LogText(_logChannel, transactionNo, "Start to print error transaction", "A02", classNMethodName: "pgCTPayment.PrintTicket",
                    //    adminMsg: "Start to print error transaction");
                    //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                    //App.ShowDebugMsg("Should be printed (Error Msg); pgCTPayment.PrintTicket");

                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    ///// Phase 2 Print Solution
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                    ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                    ImagePrintingTools.InitService();
                    ImagePrintingTools.AddPrintDocument(streamticketList, ticketNo, ticketSize);

                    App.Log.LogText(_logChannel, ticketNo, "Start to print error note (A)", "A02", classNMethodName: "pgCTPayment.PrintTicket",
                        adminMsg: "Start to print error note (A)");

                    App.ShowDebugMsg("Should be printed; pgCTPayment.PrintTicket");

                    ImagePrintingTools.ExecutePrinting(ticketNo);
                    /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                }

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCTPayment.PrintTicket; EX01");
                App.Log.LogError(_logChannel, ticketNo, ex, "EX01", "pgCTPayment.PrintTicket",
                    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.2470
        /// </summary>
        private void PrintTicketError(UICompleteTransactionResult uiCompltResult)
        {
            UserSession session = uiCompltResult.Session;
            string ticketNo = "-";

            //Reports.RdlcRendering rpRen = null;

            try
            {
                ticketNo = session.TicketCollection.TicketNo;
                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(ticketNo, TerminalVerticalLogoPath);

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 1 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports", 
                //    TicketErrorReportSourceName, ticketNo, "DataSet1", dsX.Tables[0], null);

                //App.Log.LogText(_logChannel, ticketNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgCTPayment.PrintTicketError",
                //        adminMsg: "Start to print fail transaction notice");
                //PDFTools.PrintPDFs(resultFileName, ticketNo, App.MainScreenControl.MainFormDispatcher);
                //App.ShowDebugMsg("Should be printed (Error Msg); PrintTicket.PrintTicketError");

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 2 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, ticketNo, ticketSize);

                App.Log.LogText(_logChannel, ticketNo, "Start to print error note (B)", "A02", classNMethodName: "pgCTPayment.PrintTicketError",
                    adminMsg: "Start to print error note (B)");

                App.ShowDebugMsg("Should be printed; pgCTPayment.PrintTicketError");

                ImagePrintingTools.ExecutePrinting(ticketNo);
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCTPayment.PrintTicketError; EX01");
                App.Log.LogError(_logChannel, ticketNo, ex, "EX01", "pgCTPayment.PrintTicketError-1",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.2475
        /// </summary>
        public void PrintTicketError2(string ticketNo)
        {
            //Reports.RdlcRendering rpRen = null;
            try
            {
                //rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(ticketNo, TerminalVerticalLogoPath);

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 1 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.LandscapeTicketErrorPDFRender, App.ExecutionFolderPath + @"\Reports",
                //    TicketErrorReportSourceName, ticketNo, "DataSet1", dsX.Tables[0], null);

                //App.Log.LogText(_logChannel, ticketNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgCTPayment.PrintTicketError2",
                //        adminMsg: "Start to print fail transaction notice");

                //PDFTools.PrintPDFs(resultFileName, ticketNo, App.MainScreenControl.MainFormDispatcher);
                //App.ShowDebugMsg("Should be printed (Error Msg); PrintTicket.PrintTicketError2");

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 2 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", dsX.Tables[0]) });
                ReportImageSize ticketSize = new ReportImageSize(8.0M, 3.0M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, ticketNo, ticketSize);

                App.Log.LogText(_logChannel, ticketNo, "Start to print error note (C)", "A02", classNMethodName: "pgCTPayment.PrintTicketError2",
                    adminMsg: "Start to print error note (C)");

                App.ShowDebugMsg("Should be printed; pgCTPayment.PrintTicketError2");

                ImagePrintingTools.ExecutePrinting(ticketNo);
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCTPayment.PrintTicketError2; EX01");
                App.Log.LogError(_logChannel, ticketNo, ex, "EX01", "pgCTPayment.PrintTicketError2",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }
    }
}
