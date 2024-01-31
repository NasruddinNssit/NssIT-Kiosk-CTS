using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.UI
{
	public interface IMainPageMessage
	{
		event EventHandler<UIMessageEventArgs> ShowErrorMessage;
		event EventHandler<UIMessageEventArgs> ShowPrice;
		event EventHandler<UIMessageEventArgs> ShowSystemMessage;
		event EventHandler<UIMessageEventArgs> ShowVersion;
	}
}
