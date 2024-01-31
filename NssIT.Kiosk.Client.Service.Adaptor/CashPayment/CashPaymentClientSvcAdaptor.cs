using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;

namespace NssIT.Kiosk.Client.Service.Adaptor.CashPayment
{
 //   public class ClientServiceAdaptor
	//{
	//	private string _currProcessId = "-";
	//	private string _logChannel = "ClientServiceAdaptor";
	//	private string _processId = null;
	//	private NssIT.Kiosk.Log.DB.DbLog _log = null;

	//	private IMediaInterface _netInterface;
	//	private IUIPayment _cashPaymentApp;

	//	public ClientServiceAdaptor(IMediaInterface mediaInterface)
	//	{
	//		_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

	//		_netInterface = mediaInterface;

	//		_netInterface.OnDataReceived += _netInterface_OnDataReceived;
	//	}

	//	private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
	//	{

	//		switch (e?.ReceivedData?.Instruction)
	//		{
	//			case CommInstruction.UIPaymShowForm:
	//				break;
	//			case CommInstruction.UIPaymHideForm:
	//				break;
	//			case CommInstruction.UIPaymSetCancelPermission:
	//				break;
	//			case CommInstruction.UIPaymShowCountdownMessage:
	//				break;
	//			case CommInstruction.UIPaymShowCustomerMessage:
	//				break;
	//			case CommInstruction.UIPaymShowProcessingMessage:
	//				break;
	//			case CommInstruction.UIPaymShowBanknote:
	//				break;
	//		}
	//	}
	//}
}
