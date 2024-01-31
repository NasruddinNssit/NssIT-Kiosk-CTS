using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
    public class MenuItemPageNavigateEventArgs : EventArgs
    {
        private PageNavigateDirection _pageNav = PageNavigateDirection.Exit;
        public MenuItemPageNavigateEventArgs(PageNavigateDirection pageNav)
        {
            _pageNav = pageNav;
        }

        public PageNavigateDirection PageNavigateDirection
        {
            get
            {
                return _pageNav;
            }
        }
    }
}
