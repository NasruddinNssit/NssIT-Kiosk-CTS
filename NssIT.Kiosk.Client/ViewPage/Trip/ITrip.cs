using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public interface ITrip
    {
        void InitData(UserSession session);
        //void LoadTripData(LanguageCode language, TripMode tripMode, string departureStationDesc, string destinationeStationDesc, DateTime selectedDay, TripDetailEnt[] tripDataList = null);
        void UpdateDepartTripList(UIDepartTripListAck uiTravelDate, UserSession session);

        void InitTimeFilterPage(ITimeFilter timeFilterPage);
        DateTime SelectedDay { get; }
        string SelectedTripId { get; }
        TripMode TripMode { get; }
        void UpdateShieldErrorMessage(string message);
        void ResetPageAfterError();
    }
}
