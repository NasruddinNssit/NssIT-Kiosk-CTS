using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.TableEntity
{
    public class BTnGTransactionEnt
    {
		public string SalesTransactionNo { get; set; }
		public string PaymentGateway { get; set; }
		public string MerchantTransactionNo { get; set; }
		public string Currency { get; set; }
		public decimal Amount { get; set; }

		// This status is refer to KTMBCTS.Database.PaymentGatewayHeader.Status
		public BTnGHeaderStatus LastHeaderStatus { get; set; }

		// This status is refer to KTMBCTS.Database.PaymentGatewayDetail.Status
		public BTnGDetailStatus LastDetailStatus { get; set; }
		public string CreatedDate { get; set; }
		public string LastModifiedDate { get; set; }
		public long CreatedDateTicks { get; set; }
		public long LastModifiedDateTicks { get; set; }
	}
}


