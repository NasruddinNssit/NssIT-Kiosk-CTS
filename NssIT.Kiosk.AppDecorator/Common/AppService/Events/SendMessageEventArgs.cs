using NssIT.Kiosk.AppDecorator.Common.AppService.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Events
{
	public class SendMessageEventArgs : EventArgs, ISendMessageEventArgs, IDisposable 
	{
		public Guid? NetProcessId { get; } = null;
		public string ProcessId { get; } = null;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public ResultStatus ResultState { get; private set; } = ResultStatus.New;

		private string _message = null;
		public string Message 
		{ 
			get
			{
				if ((string.IsNullOrWhiteSpace(KioskMessage?.ErrorMessage) == false) && (KioskMessage?.ErrorMessage?.Trim().Length > 0))
					return KioskMessage.ErrorMessage;
				else
					return _message;
			}
			set
			{
				_message = value;
			}
		}

		public IKioskMsg KioskMessage { get; private set; } = null;

		public AppModule ModuleAppctGroup { get; set; } = AppModule.Unknown;

		public SendMessageEventArgs(Guid? netProcessId, string processId, ResultStatus resultState, IKioskMsg kioskMessage)
		{
			NetProcessId = netProcessId;
			ProcessId = string.IsNullOrWhiteSpace(processId) ? "-" : processId;
			ResultState = resultState;
			KioskMessage = kioskMessage;

			if (KioskMessage != null)
			{
				ModuleAppctGroup = KioskMessage.Module;
				Message = kioskMessage.ErrorMessage;
				Module = KioskMessage.Module;
			}
		}

		public void Dispose()
		{
			KioskMessage = null;
		}
	}
}
