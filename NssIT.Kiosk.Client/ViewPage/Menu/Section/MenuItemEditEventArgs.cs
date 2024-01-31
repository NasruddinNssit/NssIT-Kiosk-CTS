using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
    public class MenuItemEditEventArgs : EventArgs
    {
        private TickSalesMenuItemCode _editItemCode = TickSalesMenuItemCode.FromStation;
        public MenuItemEditEventArgs(TickSalesMenuItemCode editItemCode)
        {
            _editItemCode = editItemCode;
        }

        public TickSalesMenuItemCode EditItemCode
        {
            get
            {
                return _editItemCode;
            }
        }
    }
}
