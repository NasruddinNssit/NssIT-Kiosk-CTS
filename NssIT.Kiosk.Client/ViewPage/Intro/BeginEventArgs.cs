using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
    public class BeginEventArgs : EventArgs
    {
        public TransactionType TransactionType { get; private set; } = TransactionType.BuyTicket;

        public BeginEventArgs(TransactionType transactionType)
        {
            TransactionType = transactionType;
        }
    }

    public enum TransactionType
    {
        BuyTicket,
        CollectTicket
    }
}
