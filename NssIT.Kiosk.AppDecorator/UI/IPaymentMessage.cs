using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.UI
{
	public interface IUIPayment: IDisposable 
	{
		string CurrentProcessId { get; }
		bool IsSaleSuccess { get; }
		bool IsPaymentCompleted { get; }
		bool IsPaymentDeviceReady { get; }

		Task<bool> MakePayment(string processId, decimal amount);
		void CancelTransaction();
		bool ShutDown();

		event EventHandler OnHidePaymantPage;

		event EventHandler<UIMessageEventArgs> OnShowPaymantPage;

		event EventHandler<UIMessageEventArgs> OnShowCountDownMessage;
		event EventHandler<UIMessageEventArgs> OnShowOutstandingPaymentMessage;

		event EventHandler<UIMessageEventArgs> OnShowProcessingMessage;
		event EventHandler<UIMessageEventArgs> OnShowCustomerMessage;

		event EventHandler<UIMessageEventArgs> OnShowErrorMessage;
		event EventHandler<UIMessageEventArgs> OnSetCancellingPermission;
	}
}