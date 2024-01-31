using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Network
{
	public enum CommunicationDirection
	{
		Unknown = 0,

		/// <summary>
		/// Send one command only. No need answer.
		/// </summary>
		SendOnly = 1,

		/// <summary>
		/// Send one command and expect one response answer.
		/// </summary>
		SendOneResponseOne = 2,

		/// <summary>
		/// Send one command and expect many response answer.
		/// </summary>
		SendOneResponseMany = 3
	}
}
