using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data
{
    public class MerchantSaleInfo
    {
        public string SalesTransactionNo { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string DeepLinkUrl { get; set; } = string.Empty;
        public string Base64ImageQrCode { get; set; } = string.Empty;
        public string MerchantTransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0M;
        public string Signature { get; set; } = string.Empty;
        public string CustomField { get; set; } = string.Empty;
    }
}
