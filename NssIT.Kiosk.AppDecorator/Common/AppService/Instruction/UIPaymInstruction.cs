using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Instruction
{
	public enum UIPaymInstruction
	{
		ShowForm = CommInstruction.UIPaymShowForm,
		HideForm = CommInstruction.UIPaymHideForm,
		CloseForm = CommInstruction.UIPaymCloseForm,
		InProgressEvent = CommInstruction.UIPaymInProgress,

		ShowBanknote = CommInstruction.UIPaymShowBanknote,
		ShowOutstandingPayment = CommInstruction.UIPaymShowOutstandingPayment,
		ShowRefundPayment = CommInstruction.UIPaymShowRefundPayment,

		ShowErrorMessage = CommInstruction.ShowErrorMessage,
		ShowCustomerMessage = CommInstruction.UIPaymShowCustomerMessage,
		ShowProcessingMessage = CommInstruction.UIPaymShowProcessingMessage,
		ShowCountdownMessage = CommInstruction.UIPaymShowCountdownMessage,

		SetCancelPermission = CommInstruction.UIPaymSetCancelPermission,
		CancelPayment = CommInstruction.UIPaymCancelPayment,
		CreateNewPayment = CommInstruction.UIPaymCreateNewPayment
	}
}