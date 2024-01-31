using System;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Common.AppService;

namespace NssIT.Kiosk.AppDecorator.UI
{
	public interface IUIApplicationJob
	{
		Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg);
		bool ShutDown();

		event EventHandler<UIMessageEventArgs> OnShowResultMessage;
	}
}
