using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment
{
    /// <summary>
    /// ClassCode:EXIT80.25
    /// </summary>
    public class pgCTPayment_CashPaymentProxy
    {
        private string _logChannel = "Payment";

        private ICash _cashPaymentPage = null;
        private Frame _frmPayInfo = null;
        private Frame _frmGoPay = null;
        private Frame _frmPrinting = null;
        private Border _bdGoPay;
        private ResourceDictionary _langRec = null;
        private LanguageCode _language = LanguageCode.English;

        private ShowPaymentTypeSelectionDelg _showPaymentTypeSelectionDelgHandle;
        private CancelPaymentDelg _cancelPaymentDelgHandle = null;

        //private Border _bdPay = null;
        private pgCTPrintTicket _printTicketPage = null;
        private pgCTPayment _paymentPage = null;

        private string _currency = null;
        private decimal _amount = 0.0M;
        private string _ticketNo = null;

        public pgCTPayment_CashPaymentProxy(pgCTPayment paymentPage, Frame frmGoPay, Frame frmPayInfo, Border bdGoPay, Frame frmPrinting,
            pgCTPrintTicket printTicketPage, ShowPaymentTypeSelectionDelg showPaymentTypeSelectionDelgHandle, CancelPaymentDelg cancelPaymentDelgHandle)
        {
            _frmGoPay = frmGoPay;
            _frmPrinting = frmPrinting;
            //_bdPay = bdPay;
            _frmPayInfo = frmPayInfo;
            _bdGoPay = bdGoPay;
            _paymentPage = paymentPage;
            _printTicketPage = printTicketPage;
            _showPaymentTypeSelectionDelgHandle = showPaymentTypeSelectionDelgHandle;
            _cancelPaymentDelgHandle = cancelPaymentDelgHandle;
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

        private Thread _printingThreadWorker = null;
        private Thread _endPaymentThreadWorker = null;
        /// <summary>
        /// FuncCode:EXIT80.2508
        /// </summary>
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
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgCTPayment_CashPayment._cashPaymentPage_OnEndPayment");
            }

            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.Log.LogText(_logChannel, _ticketNo, $@"End Payment for Collect Boarding Pass; Ticket No: {_ticketNo}", "A01", "pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking",
                        adminMsg: $@"End Payment for Collect Boarding Pass; Ticket No: {_ticketNo}");

                    App.ShowDebugMsg("_cashPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        // Complete Transaction Then Print Ticket
                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _frmPayInfo.Content = null;
                            _frmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_ticketNo, _language);
                            _cashPaymentPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            _frmGoPay.Content = null;
                            _frmGoPay.NavigationService.RemoveBackEntry();
                            _bdGoPay.Visibility = Visibility.Collapsed;

                            _frmPrinting.Content = null;
                            _frmPrinting.NavigationService.RemoveBackEntry();
                            _frmPrinting.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        //App.NetClientSvc.SalesService.SubmitSalesPayment(_ticketNo, _amount,
                        //    e.Cassette1NoteCount, e.Cassette2NoteCount, e.Cassette3NoteCount, e.RefundCoinAmount, out bool isServerResponded);

                        bool submitResult = App.NetClientSvc.CollectTicketService.SubmitCashPayment(_ticketNo, 
                            e.Cassette1NoteCount, e.Cassette2NoteCount, e.Cassette3NoteCount, e.RefundCoinAmount, 
                            out boardingcollectticket_status boradingTicketResult,  
                            out string errorMsg, out bool isServerResponded);


                        //DEBUG-Testing .. bool isServerResponded = false;

                        if ((isServerResponded == false) || (submitResult == false))
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                            string probMsg = null;
                            if (isServerResponded == false)
                                probMsg = "Local Server not responding (EXIT80.2508A.X01)";

                            else if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                probMsg = $@"{errorMsg}; (EXIT80.2508A.X02)";

                            else
                                probMsg = $@"Unknown error; (EXIT80.2508A.X03)";

                            probMsg = $@"{probMsg}; Ticket No.:{_ticketNo}";

                            App.Log.LogError(_logChannel, _ticketNo, new Exception(probMsg), "EX01", "pgCTPayment_CashPayment.OnEndPaymentThreadWorking", adminMsg: probMsg);

                            _paymentPage.PrintTicketError2(_ticketNo);

                            if (_paymentPage.IsPauseOnPrinting == false)
                                App.MainScreenControl.ShowWelcome();

                            //_printingThreadWorker = new RunThreadMan(new ThreadStart(PrintErrorThreadWorking), "pgCTPayment_CashPayment.OnEndPaymentThreadWorking(Fail Ticket Printing)", 60, _logChannel);
                        }
                        else
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: true, language: _language);

                            _paymentPage.PrintTicket(boradingTicketResult);

                            if (_paymentPage.IsPauseOnPrinting == false)
                                App.MainScreenControl.ShowWelcome();
                        }
                    }
                    else if (
                        (e.ResultState == AppDecorator.Common.PaymentResult.Cancel) ||
                        (e.ResultState == AppDecorator.Common.PaymentResult.Timeout) ||
                        (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        if ((App.AvailablePaymentTypeList?.Length == 1) && (App.CheckIsPaymentTypeAvailable(PaymentType.Cash)))
                            _cancelPaymentDelgHandle();
                        else
                            _showPaymentTypeSelectionDelgHandle();
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

                        _cancelPaymentDelgHandle();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgCTPayment_CashPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgCTPayment_CashPayment.OnEndPaymentThreadWorking");
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }

            //void PrintErrorThreadWorking()
            //{
            //    try
            //    {
            //        _paymentPage.PrintTicketError2(_ticketNo);
            //        App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgCTPayment_CashPayment.PrintErrorThreadWorking");

            //        // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
            //    }
            //    catch (ThreadAbortException)
            //    {
                    
            //    }
            //    catch (Exception ex)
            //    {
            //        App.ShowDebugMsg($@"{ex.Message}; EX02; pgCTPayment_CashPayment.PrintErrorThreadWorking");
            //        App.Log.LogError(_logChannel, "-", ex, "EX03", "pgCTPayment_CashPayment.PrintErrorThreadWorking");
            //    }
            //}
        }

        public void DeactivatePayment()
        {
            _cashPaymentPage?.ClearEvents();
        }

        public void StartCashPayment(string currency, decimal amount, string refNo, ResourceDictionary languageResource, LanguageCode language)
        {
            try
            {
                _cashPaymentPage = App.MainScreenControl.CashierPage;

                _langRec = languageResource;
                _language = language;
                _currency = currency;
                _amount = amount;
                _ticketNo = refNo;

                ClearPrintingThread();

                _cashPaymentPage.ClearEvents();
                _cashPaymentPage.OnEndPayment += _cashPaymentPage_OnEndPayment;

                _cashPaymentPage.InitSalesPayment(refNo, amount, refNo, language, currency);

                _bdGoPay.Visibility = Visibility.Visible;
                _frmGoPay.Content = null;
                _frmGoPay.NavigationService.RemoveBackEntry();
                _frmGoPay.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                /////App.CurrentTransStage = TicketTransactionStage.ETS;
                _frmGoPay.NavigationService.Navigate(_cashPaymentPage);
                //_bdPay.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment_CashPayment.StartCashPayment");
                App.MainScreenControl.Alert(detailMsg: $@"Error when try to start Cash Payment for Boarding Pass");
            }
        }
    }
}
