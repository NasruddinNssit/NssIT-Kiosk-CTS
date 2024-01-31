using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
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
    /// ClassCode:EXIT80.26
    /// </summary>
    public class pgCTPayment_BTnGPaymentProxy
    {
        private string _logChannel = "Payment";

        private IBTnG _bTnGPaymentPage = null;
        private Frame _frmPayInfo = null;
        private Frame _frmGoPay = null;
        private Frame _frmPrinting = null;
        private Border _bdGoPay;
        private ResourceDictionary _langRec = null;
        private LanguageCode _language = LanguageCode.English;
        private ShowPaymentTypeSelectionDelg _showPaymentTypeSelectionDelgHandle;
        //private Border _bdPay = null;
        private pgCTPrintTicket _printTicketPage = null;
        private pgCTPayment _paymentPage = null;

        private string _currency = null;
        private decimal _amount = 0.0M;
        private string _ticketNo = null;
        private string _paymentGateway = null;
        private string _paymentMethod = null;
        private string _firstName = null;
        private string _lastName = null;
        private string _contactNo = null;
        private string _paymentGatewayLogoUrl = null;

        public string LastBTnGSaleTransNo { get; private set; } = null;

        public pgCTPayment_BTnGPaymentProxy(pgCTPayment paymentPage, Frame frmGoPay, Frame frmPayInfo, Border bdGoPay, Frame frmPrinting,
            pgCTPrintTicket printTicketPage, ShowPaymentTypeSelectionDelg showPaymentTypeSelectionDelgHandle)
        {
            _frmGoPay = frmGoPay;
            _frmPrinting = frmPrinting;
            //_bdPay = bdPay;
            _frmPayInfo = frmPayInfo;
            _bdGoPay = bdGoPay;
            _paymentPage = paymentPage;
            _printTicketPage = printTicketPage;
            _showPaymentTypeSelectionDelgHandle = showPaymentTypeSelectionDelgHandle;
        }

        private void ClearPrintingThread()
        {
            //_printingThreadWorker?.AbortRequest(out _, 500);
            //_printingThreadWorker = null;
        }

        //private RunThreadMan _printingThreadWorker = null;
        private RunThreadMan _endPaymentThreadWorker = null;
        /// <summary>
        /// FuncCode:EXIT80.2608
        /// </summary>
        private void _bTnGPaymentPage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            try
            {
                LastBTnGSaleTransNo = e.BTnGSaleTransactionNo;

                App.Log.LogText(_logChannel, _ticketNo, e,
                    "A01", "pgCTPayment_BTnGPayment._bTnGPaymentPage_OnEndPayment", extraMsg: "MsgObj: EndOfPaymentEventArgs");

                _endPaymentThreadWorker?.AbortRequest(out _, 500);
                _endPaymentThreadWorker = new RunThreadMan(new ThreadStart(OnEndPaymentThreadWorking), "pgCTPayment_BTnGPayment._bTnGPaymentPage_OnEndPayment", 120, _logChannel, isLogReq: true);
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgCTPayment_BTnGPayment._cashPaymentPage_OnEndPayment");
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            /// <summary>
            /// FuncCode:EXIT80.2608A
            /// </summary>
            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.Log.LogText(_logChannel, _ticketNo, $@"End Payment for Collect Boarding Pass; Ticket No: {_ticketNo}", "A01", "pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking", 
                        adminMsg: $@"End Payment for Collect Boarding Pass; Ticket No: {_ticketNo}");

                    App.ShowDebugMsg("Collect Ticket; _bTnGPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        ///// App.BookingTimeoutMan.ResetCounter();

                        // Complete Transaction Then Print Ticket
                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _frmPayInfo.Content = null;
                            _frmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_ticketNo, _language);

                            _bTnGPaymentPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            _frmGoPay.Content = null;
                            _frmGoPay.NavigationService.RemoveBackEntry();
                            _frmGoPay.NavigationService.Content = null;
                            _bdGoPay.Visibility = Visibility.Collapsed;

                            //_bdPay.Visibility = Visibility.Collapsed;

                            _frmPrinting.Content = null;
                            _frmPrinting.NavigationService.RemoveBackEntry();
                            _frmPrinting.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        bool submitResult = App.NetClientSvc.CollectTicketService.SubmitEWalletPayment(_ticketNo,
                            e.PaymentMethod, PaymentType.PaymentGateway, e.BTnGSaleTransactionNo, out boardingcollectticket_status boradingTicketResult, 
                            out string errorMsg, out bool isServerResponded);

                        if ((isServerResponded == false) || (submitResult == false))
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                            string probMsg = null;
                            if (isServerResponded == false)
                                probMsg = "Local Server not responding (EXIT80.2608A.X01)";
                        
                            else if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                probMsg = $@"{errorMsg}; (EXIT80.2608A.X02)";
                        
                            else
                                probMsg = $@"Unknown error; (EXIT80.2608A.X03)";
                        
                            probMsg = $@"{probMsg}; Ticket No.:{_ticketNo}";

                            App.Log.LogError(_logChannel, _ticketNo, new Exception(probMsg), "EX01", "pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking", adminMsg: probMsg);

                            _paymentPage.PrintTicketError2(_ticketNo);

                            if (_paymentPage.IsPauseOnPrinting == false)
                                App.MainScreenControl.ShowWelcome();

                            //_printingThreadWorker = new RunThreadMan(new ThreadStart(PrintErrorThreadWorking), "pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking(Fail Ticket Printing)", 60, _logChannel);
                        }
                        else
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: true, language: _language);

                            _paymentPage.PrintTicket(boradingTicketResult);

                            if (_paymentPage.IsPauseOnPrinting == false)
                                App.MainScreenControl.ShowWelcome();
                        }
                    }
                    else if ((e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == AppDecorator.Common.PaymentResult.Fail) || (e.ResultState == AppDecorator.Common.PaymentResult.Timeout))
                    {
                        /////App.TimeoutManager.ResetTimeout();

                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _bTnGPaymentPage.ClearEvents();
                            _frmGoPay.Content = null;
                            _frmGoPay.NavigationService.RemoveBackEntry();
                            _frmGoPay.NavigationService.Content = null;
                            //_bdPay.Visibility = Visibility.Collapsed;
                            _bdGoPay.Visibility = Visibility.Collapsed;

                            System.Windows.Forms.Application.DoEvents();
                            if (_showPaymentTypeSelectionDelgHandle != null)
                            {
                                _showPaymentTypeSelectionDelgHandle.Invoke();
                            }
                        }));
                    }
                    else
                    {
                        ///// App.BookingTimeoutMan.ResetCounter();

                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Fail
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        //App.NetClientSvc.SalesService.RequestSeatRelease(_transactionNo);
                        //if (isServerResponded == false)
                        //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000913)");

                        //if (_isPauseOnPrinting == false)
                        App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgCTPayment_BTnGPayment.OnEndPaymentThreadWorking");
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
            //        App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgCTPayment_BTnGPayment.PrintErrorThreadWorking");

            //        // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");

            //        if (_paymentPage.IsPauseOnPrinting == false)
            //            App.MainScreenControl.ShowWelcome();
            //    }
            //    catch (ThreadAbortException)
            //    {
            //        //PDFTools.KillAdobe("AcroRd32");
            //    }
            //    catch (Exception ex)
            //    {
            //        App.ShowDebugMsg($@"{ex.Message}; EX02;pgCTPayment_BTnGPayment.PrintErrorThreadWorking");
            //        App.Log.LogError(_logChannel, "-", ex, "EX03", "pgCTPayment_BTnGPayment.PrintErrorThreadWorking");
            //    }
            //}
        }

        public void DeactivatePayment()
        {
            _bTnGPaymentPage?.ClearEvents();
        }

        public void StartBTnGPayment(string currency, decimal amount, string refNo, string paymentGateway, string firstName, string lastName, string contactNo,
            string paymentGatewayLogoUrl, string paymentMethod,
            ResourceDictionary languageResource, LanguageCode language)
        {
            try
            {
                _bTnGPaymentPage = App.MainScreenControl.BTnGCounter;

                _langRec = languageResource;
                _language = language;

                LastBTnGSaleTransNo = null;
                _currency = currency;
                _amount = amount;
                _ticketNo = refNo;
                _paymentGateway = paymentGateway;
                _paymentMethod = paymentMethod;
                _firstName = firstName;
                _lastName = lastName;
                _contactNo = contactNo;
                _paymentGatewayLogoUrl = paymentGatewayLogoUrl;

                ClearPrintingThread();

                _bTnGPaymentPage.ClearEvents();
                _bTnGPaymentPage.OnEndPayment += _bTnGPaymentPage_OnEndPayment;

                _bTnGPaymentPage.InitPaymentData(
                    currency, amount, _ticketNo, paymentGateway,
                    firstName, lastName, contactNo, paymentGatewayLogoUrl, _paymentMethod,
                    languageResource);

                _bdGoPay.Visibility = Visibility.Visible;
                _frmGoPay.Content = null;
                _frmGoPay.NavigationService.RemoveBackEntry();
                _frmGoPay.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                /////App.CurrentTransStage = TicketTransactionStage.ETS;
                _frmGoPay.NavigationService.Navigate(_bTnGPaymentPage);
                //_bdPay.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgCTPayment_BTnGPayment.StartBTnGPayment");
                App.MainScreenControl.Alert(detailMsg: $@"Error when try to start eWallet Payment for Boarding Pass");
            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentPage.BTnGShowPaymentInfo(kioskMsg);
        }
    }
}
