using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx
{
    [Serializable]
    public class UIxSubmitCollectTicketPaymentRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string Paymentmethod { get; private set; }
        public PaymentType TypeOfPayment { get; private set; }
        public int Cassette1NoteCount { get; private set; }
        public int Cassette2NoteCount { get; private set; }
        public int Cassette3NoteCount { get; private set; }
        public int RefundCoinAmount { get; private set; }
        public string PaymentRefNo { get; private set; }

        //Pay by cash
        public UIxSubmitCollectTicketPaymentRequest(string processId,
            int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount)
            : base(processId)
        {
            Paymentmethod = "C";
            TypeOfPayment = PaymentType.Cash;
            Cassette1NoteCount = cassette1NoteCount;
            Cassette2NoteCount = cassette2NoteCount;
            Cassette3NoteCount = cassette3NoteCount;
            RefundCoinAmount = refundCoinAmount;
            PaymentRefNo = "";
        }

        //Pay by eWallet
        public UIxSubmitCollectTicketPaymentRequest(string processId,
            string paymentmethod, PaymentType typeOfPayment, string paymentRefNo)
            : base(processId)
        {
            Paymentmethod = paymentmethod;
            TypeOfPayment = typeOfPayment;
            PaymentRefNo = (paymentRefNo ?? "").Trim();

            Cassette1NoteCount = 0;
            Cassette2NoteCount = 0;
            Cassette3NoteCount = 0;
            RefundCoinAmount = 0;
        }
    }
}
