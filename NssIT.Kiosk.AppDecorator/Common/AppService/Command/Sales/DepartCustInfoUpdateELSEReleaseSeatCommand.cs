using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class DepartCustInfoUpdateELSEReleaseSeatCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartCustInfoUpdateELSEReleaseSeatRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		
		public CustSeatDetail[] PassengerSeatDetail { get; private set; }
		public string TransactionNo { get; private set; }
		public string TripDate { get; private set; }
		public string DepartDate { get; private set; }

		public DepartCustInfoUpdateELSEReleaseSeatCommand(string processId, Guid? netProcessId, 
			CustSeatDetail[] passengerSeatDetail,
			string transactionNo, string tripDate, string departDate)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			PassengerSeatDetail = passengerSeatDetail;
			TransactionNo = transactionNo;
			TripDate = tripDate;
			DepartDate = departDate;
		}

		public void Dispose()
		{
			NetProcessId = null;
			PassengerSeatDetail = null;
		}
	}
}
