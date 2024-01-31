using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class PaymentSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private TextBlock PaymentTypeDesc { get; set; }

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		private Border SectionPayment { get; set; }
		private TextBlock LabelPayment { get; set; }

		public PaymentSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdPayment, TextBlock lblPayment, TextBlock txtPaymentTypeDesc)
		{
			IsEditMode = false;
			PageDispatcher = dispatcher;

			ReInit(bdPayment, lblPayment, txtPaymentTypeDesc);

			_labelNormalTextColor = LabelPayment.Foreground;

			PaymentTypeDesc.Visibility = Visibility.Collapsed;
			IsEditAllowed = true;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdPayment"></param>
		/// <param name="lblPayment"></param>
		public void ReInit(Border bdPayment, TextBlock lblPayment, TextBlock txtPaymentTypeDesc)
		{
			SectionPayment = bdPayment;
			LabelPayment = lblPayment;
			PaymentTypeDesc = txtPaymentTypeDesc;
		}

		public void SetValue(string paymentTypeDesc)
		{
			if (paymentTypeDesc != null)
			{
				PaymentTypeDesc.Text = paymentTypeDesc;
			}
		}

		//private bool _isEditAllowed = false;
		public bool IsEditAllowed { get; set; }

		public bool IsEditMode { get; private set; }

		public void CloseEdit()
		{
			if (IsEditMode)
			{
				IsEditMode = false;
				PageDispatcher.Invoke(new Action(() => {
					SectionPayment.Background = _normalModeBackgroundSectionColor;
					LabelPayment.Foreground = _labelNormalTextColor;
					if (string.IsNullOrWhiteSpace(PaymentTypeDesc.Text))
					{
						PaymentTypeDesc.Visibility = Visibility.Collapsed;
					}
					else
					{
						PaymentTypeDesc.Visibility = Visibility.Visible;
					}
				}));
			}
		}

		public void OpenEdit()
		{
			if (!IsEditMode)
			{
				IsEditMode = true;
				PageDispatcher.Invoke(new Action(() =>
				{
					SectionPayment.Background = _editModeBackgroundSectionColor;
					LabelPayment.Foreground = _labelEditTextColor;
					PaymentTypeDesc.Visibility = Visibility.Collapsed;
				}));
			}
		}

		public void ShowData(bool? isAllowEdit = null)
		{
			PageDispatcher.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(PaymentTypeDesc.Text))
				{
					PaymentTypeDesc.Visibility = Visibility.Collapsed;
				}
				else
				{
					PaymentTypeDesc.Visibility = Visibility.Visible;
				}
			}));
		}

		public void Reset()
		{
			//IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionPayment.Background = _normalModeBackgroundSectionColor;
				LabelPayment.Foreground = _labelNormalTextColor;

				PaymentTypeDesc.Text = "";
				PaymentTypeDesc.Visibility = Visibility.Collapsed;
			}));
		}

		public void Hide()
		{
			PageDispatcher.Invoke(new Action(() => {
			}));
		}

		public void Show()
		{
			PageDispatcher.Invoke(new Action(() => {
			}));
		}

		public void Dispose()
		{
			PaymentTypeDesc = null;
			SectionPayment = null;
			PageDispatcher = null;
		}
	}
}