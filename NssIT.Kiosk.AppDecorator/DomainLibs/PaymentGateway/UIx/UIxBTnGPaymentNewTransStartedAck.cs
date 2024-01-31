using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentNewTransStartedAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck
    {
        public string BTnGSalesTransactionNo { get; set; } = string.Empty;
        public string Base64ImageQrCode { get; set; } = string.Empty;
        public string MerchantTransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0M;
        public string SnRId { get; set; } = string.Empty;

        public UIxBTnGPaymentNewTransStartedAck(Guid? refNetProcessId, string processId,
            string bTnGSalesTransactionNo, string base64ImageQrCode, string merchantTransactionNo, decimal amount, string snrId)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            BTnGSalesTransactionNo = bTnGSalesTransactionNo;
            Base64ImageQrCode = base64ImageQrCode;
            MerchantTransactionNo = merchantTransactionNo;
            Amount = amount;
            SnRId = snrId;
        }
    }
}
