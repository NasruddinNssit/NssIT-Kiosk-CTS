using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public interface ITimeFilter
    {
        event EventHandler<TimeFilterEventArgs> OnTimeFilterChangedTrigger;
        event EventHandler<TimeFilterEventArgs> OnTimeFilterResetTrigger;

        bool IsFilterActived { get; set; }
        void ResetFilter();
        void InitFilter(ResourceDictionary languageResource);
    }
}
