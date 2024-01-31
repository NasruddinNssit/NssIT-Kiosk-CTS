using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Info
{
    public interface IInfo
    {
        void ShowInfo(InfoCode info, LanguageCode language = LanguageCode.English);
        void ShowInfo(string demoInfo);
    }
}
