using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.PayECRApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Alert
{
    public interface IMaintenance
    {
        void InitMaintenance(PayWaveSettlementScheduler scheduler);
        void ProceedOutstandingMaintenance(UISalesCheckOutstandingCardSettlementAck outstandingCardSettlement);
        void CardSettlementStatusAcknowledge(UISalesCardSettlementStatusAck cardSettlementStatusAck);
        void MaintenanceScheduler_OnSettlementDone(object sender, SettlementDoneEventArgs e);

        void ShowProblemMessage(string problemMessage);

        PayWaveSettlement.RequestOutstandingSettlementInfo RequestOutstandingSettlementInfoHandler { get; }
        PayWaveSettlement.UpdateSettlementInfo UpdateSettlementInfoHandler { get; }
    }
}
