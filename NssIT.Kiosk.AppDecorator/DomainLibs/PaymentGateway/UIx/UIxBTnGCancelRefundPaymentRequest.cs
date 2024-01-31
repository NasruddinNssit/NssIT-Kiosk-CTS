using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGCancelRefundPaymentRequest : UIxKioskDataRequestBase, IUIxBTnGPaymentGroupAck
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOnly;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public string ProcessId { get; private set; } = null;
        public string BTnGSalesTransactionNo { get; private set; } = null;
        public string DocNo { get; private set; } = null;
        public string Currency { get; private set; } = null;
        public string PaymentGateway { get; private set; } = null;
        public decimal Amount { get; private set; } = 0.0M;
        //public BTnGKioskVoidTransactionState RequestTransactionState { get; private set; } = BTnGKioskVoidTransactionState.CancelRefundRequest;

        public UIxBTnGCancelRefundPaymentRequest(
            string processId, string bTnGSalesTransactionNo, string docNo, string currency, 
            string paymentGateway, decimal amount) 
            : base(processId)
        {
            ProcessId = processId;
            BTnGSalesTransactionNo = bTnGSalesTransactionNo;
            DocNo = docNo;
            Currency = currency;
            PaymentGateway = paymentGateway;
            Amount = amount; 
        }
    }
}
