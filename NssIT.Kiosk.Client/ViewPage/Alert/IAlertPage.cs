using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Alert
{
    public interface IAlertPage
    {
        void ShowAlertMessage(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "");
    }
}
