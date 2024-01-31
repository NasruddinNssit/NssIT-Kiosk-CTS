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
    public class UIxBTnGPaymentMakeNewPaymentRequest : UIxKioskDataRequestBase, IUIxBTnGPaymentGroupAck
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseMany;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public decimal Price { get; private set; } = 0.00M;
        public string DocNo { get; private set; } = null;
        public string PaymentGateway { get; private set; } = null;
        public string FinancePaymentMethod { get; private set; } = null;
        public string Currency { get; private set; } = null; 
        public string CustomerFirstName { get; private set; } = null; 
        public string CustomerLastName { get; private set; } = null; 
        public string ContactNo { get; private set; } = null;

        public UIxBTnGPaymentMakeNewPaymentRequest(string processId, string docNo, decimal price,
            string paymentGateway, string currency, 
            string customerFirstName, string customerLastName, string contactNo, string financePaymentMethod) 
            : base(processId)
        {
            DocNo = docNo;
            Price = price;
            PaymentGateway = paymentGateway; 
            Currency = currency; 
            CustomerFirstName = customerFirstName;
            CustomerLastName = customerLastName;
            ContactNo = contactNo;
            FinancePaymentMethod = financePaymentMethod;
        }
    }
}
