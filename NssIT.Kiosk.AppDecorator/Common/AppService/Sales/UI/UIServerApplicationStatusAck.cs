using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIServerApplicationStatusAck : IKioskMsg
    {
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.ServerApplicationStatusAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public bool ServerAppHasDisposed { get; private set; } = false;
		public bool ServerAppHasShutdown { get; private set; } = false;
		public bool ServerWebServiceIsDetected { get; private set; } = false;

		public AppGroup ApplicationGroup { get; private set; } = AppGroup.Unknown;
		public string ServerSystemVersion { get; private set; } = "";
		public PaymentType[] AvailablePaymentTypeList { get; private set; } = new PaymentType[0];

		public bool IsBoardingPassEnabled { get; private set; } = false;

		public UIServerApplicationStatusAck(Guid? refNetProcessId, string processId, DateTime timeStamp,
			bool serverAppHasDisposed, bool serverAppHasShutdown, bool serverWebServiceIsDetected, AppGroup appGroup, string systemVersion,
			PaymentType[] availablePaymentTypeList, bool isBoardingPassEnabled)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			ServerAppHasDisposed = serverAppHasDisposed;
			ServerAppHasShutdown = serverAppHasShutdown;
			ServerWebServiceIsDetected = serverWebServiceIsDetected;

			ApplicationGroup = appGroup;
			ServerSystemVersion = systemVersion;
			AvailablePaymentTypeList = availablePaymentTypeList ?? (new PaymentType[0]);
			IsBoardingPassEnabled = isBoardingPassEnabled;
		}

		public void Dispose()
		{ }
	}
}
