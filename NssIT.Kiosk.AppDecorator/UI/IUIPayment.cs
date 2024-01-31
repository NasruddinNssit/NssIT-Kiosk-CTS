using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.UI
{
	public interface IUIPayment: IDisposable 
	{
		Guid? CurrentNetProcessId { get; }
		string CurrentProcessId { get; }

		bool IsSaleSuccess { get; }
		bool IsPaymentCompleted { get; }
		bool ReadIsPaymentDeviceReady(out bool isLowCoin, out bool isRecoveryInProgressAfterDispenseCoin, out string errorMessage);

		Task<bool> MakePayment(string processId, Guid? netProcessId, decimal amount);
		void CancelTransaction();
		bool ShutDown();

		//event EventHandler<UIMessageEventArgs> OnHidePaymantPage;

		//event EventHandler<UIMessageEventArgs> OnShowPaymantPage;

		//event EventHandler<UIMessageEventArgs> OnShowCountDownMessage;
		//event EventHandler<UIMessageEventArgs> OnShowOutstandingPaymentMessage;

		event EventHandler<UIMessageEventArgs> OnShowProcessingMessage;
		//event EventHandler<UIMessageEventArgs> OnShowCustomerMessage;

		//event EventHandler<UIMessageEventArgs> OnShowErrorMessage;
		//event EventHandler<UIMessageEventArgs> OnSetCancellingPermission;
	}
}