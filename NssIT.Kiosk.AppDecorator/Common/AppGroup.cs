using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    public enum AppGroup
    {
        Unknown = 0,
        MelakaSentral = 1, /* Default Value */
        Larkin = 2,
        Gombak = 3,
        Klang = 4
    }

    public static class AppStationCode
    {
        public static string Unknown => "XXX";
        public static string MelakaSentral => "MEL";
        public static string Gombak => "TBG";
        public static string Larkin => "LAR";
        public static string Klang => "KLG";

        public static void GetStationInfo(AppGroup appGroup, out string stationCode, out string stationName)
        {
            stationCode = Unknown;
            stationName = Unknown;

            if (appGroup == AppGroup.Gombak)
            {
                stationCode = Gombak;
                stationName = "TERMINAL BERSEPADU GOMBAK";
            }
            else if (appGroup == AppGroup.Larkin)
            {
                stationCode = Larkin;
                stationName = "JB-LARKIN";
            }
            else if (appGroup == AppGroup.Klang)
            {
                stationCode = Klang;
                stationName = "KLANG";
            }
            else
            {
                stationCode = MelakaSentral;
                stationName = "MELAKA SENTRAL";
            }
        }
    }
}