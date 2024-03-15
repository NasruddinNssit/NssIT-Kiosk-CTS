using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Info
{
    /// <summary>
    /// Interaction logic for pgInfo.xaml
    /// </summary>
    public partial class pgInfo : Page, IInfo
    {
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgInfo()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Info\rosInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Info\rosInfoEnglish.xaml");
        }

        public void ShowInfo(InfoCode info, LanguageCode language = LanguageCode.English)
        {
            if (info == InfoCode.OriginInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["ORIGIN_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["ORIGIN_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Where is your departure location ?";
            }

            else if (info == InfoCode.DestinationInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["DESTINATION_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["DESTINATION_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Where are you travelling to ?";
            }

            else if (info == InfoCode.TravelDateInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["TRAVEL_DATE_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["TRAVEL_DATE_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "When do you want to travel ?";
            }

            else if (info == InfoCode.DepartTripInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["TRAVEL_DEPART_TIME_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["TRAVEL_DEPART_TIME_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Please choose a departing trip";
            }

            else if (info == InfoCode.DepartSeatInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["DEPART_SEAT_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["DEPART_SEAT_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Please pick seat(s)";
            }

            else if(info == InfoCode.SkyWaySelectInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = "";
                else
                    TxtInfo.Text = "";
            }

            else if(info == InfoCode.InsuranceSelectInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["INSURANCE_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["INSURANCE_INFO_Label"]?.ToString();
            }

            else if (info == InfoCode.DepartPickupNDrop)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["DEPART_PICKUP_DROPOFF_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["DEPART_PICKUP_DROPOFF_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Please select your desired Pick Up and Drop Off locations";
            }

            else if (info == InfoCode.PassengerInfo)
            {
                if (language == LanguageCode.Malay)
                    TxtInfo.Text = _langMal["PASSENGER_INFO_Label"]?.ToString();
                else
                    TxtInfo.Text = _langEng["PASSENGER_INFO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(TxtInfo.Text))
                    TxtInfo.Text = "Please provide your info";
            }


        }

        public void ShowInfo(string demoInfo)
        {
            TxtInfo.Text = demoInfo;
        }
    }
}
