using Newtonsoft.Json;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NssIT.Kiosk.Network.SignalRClient.API.Base.Extension
{
    public static class BTnGAPIExtension
    {
        public static string GetSignatureString(this NewPayment saleReq)
        {
            string formatedHashMsg = $@"{saleReq.MerchantId}{saleReq.MerchantTransactionNo}{saleReq.PaymentGateway}{saleReq.Currency}{saleReq.Amount.ToString("0.##")}{saleReq.ExpirySecond}{DateTime.Now.ToString("yyyyMMdd")}";
            return PaymentGuard.HashRSAKey(formatedHashMsg);
        }
        public static string GetSignatureString(this BTnGCancelRefundReqInfo cancelRefund)
        {
            // {MerchantId}{MerchantTransactionNo}{PaymentGateway}{Currency}{Amount.Value.ToString("0.##")}{Today.ToString("yyyyMMdd")}
            string formatedHashMsg = $@"{cancelRefund.MerchantId}{cancelRefund.MerchantTransactionNo}{cancelRefund.PaymentGateway}{cancelRefund.Currency}{cancelRefund.Amount.ToString("0.##")}{DateTime.Today.ToString("yyyyMMdd")}";
            return PaymentGuard.HashRSAKey(formatedHashMsg);
        }

        public static bool CheckSignature(this MerchantSaleInfo saleInfo)
        {
            string formatedHashMsg = $@"{saleInfo.SalesTransactionNo}{saleInfo.MerchantTransactionNo}{saleInfo.Amount.ToString("0.##")}{DateTime.Today.ToString("yyyyMMdd")}";

            return PaymentGuard.VerifyRSASignedHash(formatedHashMsg, saleInfo.Signature);
        }

        public static bool CheckSignature(this BTnGCancelRefundModel cancelRefund)
        {
            string formatedHashMsg = $@"{cancelRefund.Status}{cancelRefund.Description}{cancelRefund.MerchantTransactionNo}{DateTime.Today.ToString("yyyyMMdd")}";

            return PaymentGuard.VerifyRSASignedHash(formatedHashMsg, cancelRefund.Signature);
        }

    }
}
