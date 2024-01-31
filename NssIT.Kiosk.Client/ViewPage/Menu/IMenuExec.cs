using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Client.ViewPage.Trip;

namespace NssIT.Kiosk.Client.ViewPage.Menu
{
	public interface IMenuExec
	{
		event EventHandler<MenuItemEditEventArgs> OnEditMenuItem;
		event EventHandler<MenuItemPageNavigateEventArgs> OnPageNavigateChanged;

		void SetLanguage(LanguageCode language);

		void ShieldMenu();
		void UnShieldMenu();

		// IsEditAllowed property
		bool IsEditAllowedFromStation { get; set; }
		bool IsEditAllowedToStation { get; set; }
		bool IsEditAllowedDepartDate { get; set; }
		bool IsEditAllowedDepartOperator { get; set; }
		bool IsEditAllowedDepartSeat { get; set; }
		bool IsEditAllowedReturnDate { get; set; }
		bool IsEditAllowedReturnOperator { get; set; }
		bool IsEditAllowedReturnSeat { get; set; }
		bool IsEditAllowedPassenger { get; set; }
		bool IsEditAllowedPayment { get; set; }

		// Set Data
		void SetFromStationData(string departureStationName);
		void SetToStationData(string destinationName);
		void SetDepartDateData(string departTimeString);
		void SetDepartOperatorData(string operatorName);
		void SetDepartSeatData(string seatDesc);
		void SetReturnDateData(DateTime? departTime);
		void SetReturnOperatorData(string operatorName);
		void SetReturnSeatData(string seatDesc);
		void SetPaymentData(string paymentTypeDesc);
		void AddPassengerData(string passengerName, string passengerId);

		// Show Data
		void ShowFromStationData(bool? isAllowEdit = null);
		void ShowToStationData(bool? isAllowEdit = null);
		void ShowDepartDateData(bool? isAllowEdit = null);
		void ShowDepartOperatorData(bool? isAllowEdit = null);
		void ShowDepartSeatData(bool? isAllowEdit = null);
		void ShowReturnDateData(bool? isAllowEdit = null);
		void ShowReturnOperatorData(bool? isAllowEdit = null);
		void ShowReturnSeatData(bool? isAllowEdit = null);
		void ShowPassengerData(bool? isAllowEdit = null);
		void ShowPaymentData(bool? isAllowEdit = null);

		// Reset Section
		void ResetFromStation();
		void ResetToStation();
		void ResetDepartDate();
		void ResetDepartOperator();
		void ResetDepartSeat();
		void ResetReturnDate();
		void ResetReturnOperator();
		void ResetReturnSeat();
		void ResetPassenger();
		void ResetPayment();

		// To Edit
		void ActivateEditMenuItem(TickSalesMenuItemCode menuItemCode);

		void HideMiniNavigator();
		void ShowMiniNavigator();

		void InitTimeFilterPage(ITimeFilter timeFilterPage);
		bool IsTimeFilterVisibled { get; set; }
	}
}