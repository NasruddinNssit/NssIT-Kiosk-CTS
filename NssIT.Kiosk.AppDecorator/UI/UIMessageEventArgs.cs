using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.UI
{
	public class UIMessageEventArgs : EventArgs
	{
		public Guid? NetProcessId { get; private set; }

		/// <summary>
		/// A null indicates property value is ignored in processing
		/// </summary>
		public MessageType? MsgType { get; set; }

		/// <summary>
		/// For UI use. A null indicates property value is ignored in processing
		/// </summary>
		public bool? Visibled { get; set; }

		/// <summary>
		/// For UI use. A null indicates property value is ignored in processing
		/// </summary>
		public bool? Enabled { get; set; }

		/// <summary>
		/// A null indicates property value is ignored in processing
		/// </summary>
		public string Tag { get; set; }

		/// <summary>
		/// A null indicates property value is ignored in processing
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// A null indicates property value is ignored in processing
		/// </summary>
		//public IEventMessageObj MessageObj { get; set; }

		public IKioskMsg KioskMsg { get; set; }

		public UIMessageEventArgs(Guid? netPreocessId) => NetProcessId = netPreocessId;
	}
}
