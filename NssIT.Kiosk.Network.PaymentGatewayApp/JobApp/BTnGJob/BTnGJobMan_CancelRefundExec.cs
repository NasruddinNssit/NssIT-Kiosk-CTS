using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Common.WebAPI;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam.BTnG;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.SignalRClient.API.Base.Extension;
using NssIT.Kiosk.Sqlite.DB.AccessDB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Works;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob
{
    /// <summary>
    /// ClassCode:EXIT60.04
    /// </summary>
    public class BTnGJobMan_CancelRefundExec
    {
        private const string _logChannel = "BTnGPaymentGateway_Schd";

        
        private BTnGJobMan _staffWorker = null;
        private string _deviceId = "#@#";
        private string _stationCode = "#@#";

        private DbLog Log => DbLog.GetDbLog();

        public BTnGJobMan_CancelRefundExec(BTnGJobMan staffWorker)
        {
            _staffWorker = staffWorker;
            _stationCode = Setting.GetSetting().StationCode ?? "#@#";
        }

        /// <summary>
        /// FuncCode:EXIT60.0403
        /// </summary>
        public void DoJob(WebAPIAgent webAPI, string salesTransactionNo, string bookingNo, string currency, string paymentGateway, decimal amount, 
            BTnGKioskVoidTransactionState voidTransactionState)
        {
            _deviceId = RegistrySetup.GetRegistrySetting().DeviceId ?? "#@#";

            // Update DB for Begining state
            if (voidTransactionState == BTnGKioskVoidTransactionState.CancelRefundRequest)
                DBUpdateCancelRefundState(salesTransactionNo, BTnGHeaderStatus.CANCEL_REQUEST, BTnGDetailStatus.w_cancel_refund_request);
            else
                DBUpdateCancelRefundState(salesTransactionNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_cancel_refund_request);
            
            // Do Web API CancelRefund process
            BTnGDetailStatus webUpdateState = WebCancelRefund(webAPI, salesTransactionNo, bookingNo, currency, paymentGateway, amount);

            // Update DB for after success process; Else throw error.
            if (voidTransactionState == BTnGKioskVoidTransactionState.CancelRefundRequest)
                DBUpdateCancelRefundState(salesTransactionNo, BTnGHeaderStatus.CANCEL, webUpdateState);
            else
                DBUpdateCancelRefundState(salesTransactionNo, BTnGHeaderStatus.FAIL, webUpdateState);
            
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT60.048A
            /// </summary>
            void DBUpdateCancelRefundState(string salesTransactionNoX, BTnGHeaderStatus headerStatus, BTnGDetailStatus detailStatus)
            {
                // Update Sale Status
                DatabaseAx dbAx = DatabaseAx.GetAccess();
                using (var newPayAx = new BTnGCancelRefundRequestAx<SuccessXEcho>(salesTransactionNoX, headerStatus, detailStatus))
                {
                    using (var transResult = (BTnGCancelRefundRequestAx<SuccessXEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                    {
                        if (transResult.ResultStatus.IsSuccess)
                        {
                            // By-pass
                        }
                        else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                        {
                            throw transResult.ResultStatus.Error;
                        }
                        else
                        {
                            throw new Exception("Unknown error when updating BTnG transaction status at Kiosk Server; (EXIT60.048A.X01)");
                        }
                    }
                }
            }

            /// <summary>
            /// FuncCode:EXIT60.048B
            /// </summary>
            BTnGDetailStatus WebCancelRefund(WebAPIAgent webAPIX, string salesTransactionNoX, string bookingNoX, string currencyX, string paymentGatewayX, decimal amountX)
            {
                BTnGCancelRefundReqInfo req = new BTnGCancelRefundReqInfo()
                {
                    SalesTransactionNo = salesTransactionNoX,
                    MerchantTransactionNo = bookingNoX,
                    MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId,
                    Currency = currencyX,
                    PaymentGateway = paymentGatewayX,
                    Amount = amountX
                };
                req.Signature = req.GetSignatureString();

                CancelRefundRequest parameters = new CancelRefundRequest()
                {
                    DeviceId = _deviceId, 
                    StationCode = _stationCode,
                    CancelRefundRequestInfo = req
                };

                dynamic apiRes = webAPIX.Post<BTnGCancelRefundResult>(parameters, @"PaymentGateway/cancelRefundSale", $@"BTnGJobMan_CancelRefundExec.WebCancelRefund").GetAwaiter().GetResult();

                Log.LogText(_logChannel, bookingNoX, apiRes, "B01", "BTnGJobMan_CancelRefundExec.WebCancelRefund", extraMsg: "MsgObj: BTnGCancelRefundResult OR WepApiException");

                if (apiRes is BTnGCancelRefundResult result)
                {
                    BTnGDetailStatus? detState = null;
                    bool? isValidSignature = null;

                    if (result.Status == true)
                    {
                        detState = result.Data.Status.ToBTnGDetailStatus();

                        if (detState == BTnGDetailStatus.unknown_fail_status)
                            throw new Exception($@"Unrecognized Cancel/Refund result status; Status: {result.Data.Status}; (EXIT60.048B.X05)");
                        
                        else if (detState == BTnGDetailStatus.w_cancel_and_refund_by_api_already_done)
                            isValidSignature = true;

                        else if (result.Data is null)
                            throw new Exception($@"Invalid Cancel/Refund result data from web; (EXIT60.048B.X06)");

                        else
                            isValidSignature = result.Data.CheckSignature();
                    }

                    if ((result.Status == true) && (detState.HasValue)
                        && (detState.Value != BTnGDetailStatus.@new)
                        && (detState.Value != BTnGDetailStatus.paid)
                        && (detState.Value != BTnGDetailStatus.init)
                        && (detState.Value != BTnGDetailStatus.paying)
                        && (detState.Value != BTnGDetailStatus.other)
                        && (detState.Value != BTnGDetailStatus.unknown_fail_status)
                        && (isValidSignature == true)
                        )
                    {
                        return detState.Value;
                    }

                    else
                    {
                        WebApiException ex2 = null;
                        string btngDetailStatus = (detState.HasValue) ? detState.Value.ToString(): "-NULL-";

                        if ((isValidSignature == false) && (result.Status == true))
                        {
                            ex2 = new WebApiException(new Exception($@"Invalid signature for CancelRefund result; (EXIT60.048B.X10); BTnG Detail Cancel Result Status : {btngDetailStatus}"));
                        }
                        else
                            ex2 = new WebApiException(new Exception($@"No valid data found; (EXIT60.048B.X11); Error Code : {result.Code}; BTnG Detail Cancel Result Status : {btngDetailStatus}"));

                        Log.LogText(_logChannel, $@"{bookingNoX}-{salesTransactionNoX}", result, "A03", @"BTnGJobMan_CancelRefundExec.WebCancelRefund", 
                            AppDecorator.Log.MessageType.Error,
                            extraMsg: $@"No valid data found; (EXIT60.048B.X10A); Error Code : {result.Code}; BTnG Detail Cancel Result Status : {btngDetailStatus}; MsgObj: BTnGCancelRefundResult");

                        throw ex2;
                    }
                }
                else if (apiRes is WebApiException wex)
                    throw wex;
                else
                    throw new Exception(@"Unexpected error occurred when executing Web command PaymentGateway/cancelRefundSale; (EXIT60.048B.X15)");
            }
        }
    }
}
