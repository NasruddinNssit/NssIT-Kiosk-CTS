﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public interface IPaymentTraffitController
    {
        bool GetPermissionToPay();
    }
}
