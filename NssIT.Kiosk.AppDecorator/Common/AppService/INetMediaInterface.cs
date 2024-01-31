using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	public interface INetMediaInterface : IDisposable
	{
		event EventHandler<DataReceivedEventArgs> OnDataReceived;

		void SendMsgPack(NetMessagePack msgPack);

		bool SetExpiredNetProcessId(Guid netProcessId);

		void ShutdownService();
	}
}
