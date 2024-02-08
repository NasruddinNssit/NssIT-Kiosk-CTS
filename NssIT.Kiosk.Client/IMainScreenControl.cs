using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.ViewPage.Alert;
using NssIT.Kiosk.Client.ViewPage.Info;
using NssIT.Kiosk.Client.ViewPage.Menu;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Device.PAX.IM20.PayECRApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client
{
	public interface IMainScreenControl
	{
		/// <summary>
		/// Execution Menu
		/// </summary>
		IMenuExec ExecMenu { get; set; }

		IInfo UserInfo { get; set; }

		Dispatcher MainFormDispatcher { get;  }

		ICash CashierPage { get; }
		IBTnG BTnGCounter { get; }

		void ShowWelcome();
        void InitiateMaintenance(PayWaveSettlementScheduler cardSettScheduler);

        void InitForOperationTimeScheduler(AppOperationHandler appOperationHandler);
		void ChooseLanguage(AppModule module);
		void ChooseOriginStation(UIOriginListAck uiOrig, UserSession session);
		void ChooseDestinationStation(UIDestinationListAck uiDest, UserSession session);
		void ChooseTravelDates(UITravelDatesEnteringAck uiTravelDate, UserSession session);
		void ShowInitDepartTrip(UIDepartTripInitAck tripInit, UserSession session);
		void UpdateDepartTripList(UIDepartTripListAck uiTravelDate, UserSession session);
		void UpdateDepartTripSubmitError(UIDepartTripSubmissionErrorAck uiTripSubErr, UserSession session);
		void ChooseDepartSeat(UIDepartSeatListAck uiDepartSeatList, UserSession session);
		void ChoosePickupNDrop(UIDepartPickupNDropAck uiIDepartPickupNDrop);
		void ChooseInsurance(UIInsuranceAck uiInsurnace);

		void ChooseSkyWay(UISkyWayAck uiSkyWay);
		void EnterPassengerInfo(UICustInfoAck uiCustInfo);
		void MakeTicketPayment(UISalesPaymentProceedAck uiSalesPayment);
		void UpdateTransactionCompleteStatus(UICompleteTransactionResult uiCompltResult);
		void BTnGShowPaymentInfo(IKioskMsg kioskMsg);
		void UpdateDepartDate(DateTime newDepartDate);

		void Alert(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "");

		//void StartSelling(LanguageCode langCode);
		void ToTopMostScreenLayer();
		void ToNormalScreenLayer();

		#region - Collect Ticket -
		void CTChooseBusCompany(IKioskMsg kioskMsg);
		void CTChooseDepartureDate(IKioskMsg kioskMsg);
		void CTEnterTicketNo(IKioskMsg kioskMsg);
		void CTShowTicketNumberNotFound(IKioskMsg kioskMsg);
		void CTEnterPassengerInfo(IKioskMsg kioskMsg);
		void CTMakeTicketPayment(IKioskMsg kioskMsg);

		#endregion - End - Collect Ticket- 

		bool QueryWindowSize(out double windowWidth, out double windowHeight);
	}
}
