using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Constant
{
    public static class PaymentStatus
    {
        public static string New { get; } = "new";
        public static string PaymentGatewayReqFail { get; } = "payment_gateway_req_fail";
        public static string Init { get; } = "init";
        public static string Paying { get; } = "paying";
        public static string Expired { get; } = "expired";
        public static string PaidFail { get; } = "paid_fail";
        public static string Paid { get; } = "paid";
        public static string CancelByApi { get; } = "cancel_by_api";
        public static string RefundedByFailedSales { get; } = "refunded_by_failed_sales";
        public static string RefundedByApi { get; } = "refunded_by_api";
        public static string RefundedByMerchant { get; } = "refunded_by_merchant";
        public static string RefundedByAdmin { get; } = "refunded_by_admin";
        public static string ManualRefund { get; } = "manual_refund";
        public static string Other { get; } = "other";
        public static string RecordNotFound { get; } = "record_not_found";

        public static string GetStatusDescription(string statusStr)
        {
            string sStr = statusStr;
            if (string.IsNullOrWhiteSpace(sStr))
                sStr = "*";

            if (sStr.Equals(New, StringComparison.InvariantCultureIgnoreCase))
                return "New transaction request";

            else if (sStr.Equals(PaymentGatewayReqFail, StringComparison.InvariantCultureIgnoreCase))
                return "Fail to initialize with payment gateway";

            else if (sStr.Equals(Init, StringComparison.InvariantCultureIgnoreCase))
                return "Initialized with payment gateway";

            else if (sStr.Equals(Paying, StringComparison.InvariantCultureIgnoreCase))
                return "In the process for user key in payment information";

            else if (sStr.Equals(Expired, StringComparison.InvariantCultureIgnoreCase))
                return "Transaction had been expired";

            else if (sStr.Equals(PaidFail, StringComparison.InvariantCultureIgnoreCase))
                return "User fail to pay";

            else if (sStr.Equals(Paid, StringComparison.InvariantCultureIgnoreCase))
                return "User paid successfully";

            else if (sStr.Equals(CancelByApi, StringComparison.InvariantCultureIgnoreCase))
                return "Transaction canceled (by commmand)";

            else if (sStr.Equals(RefundedByFailedSales, StringComparison.InvariantCultureIgnoreCase))
                return "Transaction had been refunded due to transaction expired/canceled";

            else if (sStr.Equals(RefundedByApi, StringComparison.InvariantCultureIgnoreCase))
                return "Refunded (by commmand)";

            else if (sStr.Equals(RefundedByMerchant, StringComparison.InvariantCultureIgnoreCase))
                return "Refunded from Merchant Portal";

            else if (sStr.Equals(RefundedByAdmin, StringComparison.InvariantCultureIgnoreCase))
                return "Refunded from Admin Portal";

            else if (sStr.Equals(ManualRefund, StringComparison.InvariantCultureIgnoreCase))
                return "Manual Refund perform by admin";

            else if (sStr.Equals(Other, StringComparison.InvariantCultureIgnoreCase))
                return "Other error";

            else if (sStr.Equals(RecordNotFound, StringComparison.InvariantCultureIgnoreCase))
                return "Record not found in System";

            else
                return "-~*~-";
        }

        public static PaymentResult ToPaymentResult(string statusStr)
        {
            string sStr = statusStr;
            if (string.IsNullOrWhiteSpace(sStr))
                sStr = "*";

            if (sStr.Equals(Paid, StringComparison.InvariantCultureIgnoreCase))
                return PaymentResult.Success;

            else
                return PaymentResult.Fail;
        }
    }
}
