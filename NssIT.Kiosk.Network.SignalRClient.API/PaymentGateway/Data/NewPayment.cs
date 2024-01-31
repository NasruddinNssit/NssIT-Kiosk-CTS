using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data
{
    public class NewPayment
    {
        /// <summary>
        /// Project Account Code; For KTMB Kiosk will use "KTMB"
        /// </summary>
        public string MerchantId { get; set; } = "";
        public string PaymentGateway { get; set; } = "";
        /// <summary>
        /// Transaction Reference No.
        /// </summary>
        public string MerchantTransactionNo { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; } = 0;
        public string Signature { get; set; } = "";
        public string RedirectUrl { get; set; } = "";
        public string DeepLinkUrl { get; set; } = "";
        public string NotificationUrl { get; set; } = "";
        /// <summary>
        /// Any extra field needed when the api return response data; BookingId#TryPaymentCountNo
        /// </summary>
        public string CustomField { get; set; } = "";
        public string Remark { get; set; } = "";
        public string OrderTitle { get; set; } = "";
        public string DisplayName { get; set; } = "";
        /// <summary>
        /// "Web", "MobileBrowser", "MobileApp" and "Kiosk"
        /// </summary>
        public string TerminalType { get; set; } = "";
        public int ExpirySecond { get; set; } = 0;
        public PassengerSummaryInfo PayerInfo { get; set; } = null;
    }
}
