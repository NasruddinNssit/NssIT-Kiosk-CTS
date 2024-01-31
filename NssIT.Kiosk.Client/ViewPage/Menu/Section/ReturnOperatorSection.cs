using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class ReturnOperatorSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// RetnOprt : Return Operator
		/// </summary>
		private Border BorderRetnOprt { get; set; }
		private Image ImgEditRetnOprt { get; set; }
		private Line LineRetnOprt1 { get; set; }
		private Line LineRetnOprt2 { get; set; }
		private TextBlock LabelRetnOprtNm { get; set; }
		private TextBlock ReturnOperatorName { get; set; }

		public ReturnOperatorSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdReturnOperator, Line lnReturnOperator1, Line lnReturnOperator2, TextBlock lblReturnOperatorNm, TextBlock txtReturnOperatorNm, Image imgReturnOperator)
		{
			IsEditMode = false;

			txtReturnOperatorNm.Text = "";
			imgReturnOperator.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdReturnOperator, lnReturnOperator1, lnReturnOperator2, lblReturnOperatorNm, txtReturnOperatorNm, imgReturnOperator);

			_labelNormalTextColor = LabelRetnOprtNm.Foreground;

			ReturnOperatorName.Visibility = Visibility.Collapsed;
			LineRetnOprt1.Visibility = Visibility.Visible;
			LineRetnOprt2.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdReturnOperator"></param>
		/// <param name="lnReturnOperator1"></param>
		/// <param name="lnReturnOperator2"></param>
		/// <param name="lblReturnOperatorNm"></param>
		/// <param name="txtReturnOperatorNm"></param>
		/// <param name="imgReturnOperator"></param>
		public void ReInit(Border bdReturnOperator, Line lnReturnOperator1, Line lnReturnOperator2, TextBlock lblReturnOperatorNm, TextBlock txtReturnOperatorNm, Image imgReturnOperator)
		{
			BorderRetnOprt = bdReturnOperator;
			LineRetnOprt1 = lnReturnOperator1;
			LineRetnOprt2 = lnReturnOperator2;
			LabelRetnOprtNm = lblReturnOperatorNm;
			ReturnOperatorName = txtReturnOperatorNm;
			ImgEditRetnOprt = imgReturnOperator;
		}

		public void SetValue(string operatorName)
		{
			if (operatorName != null)
			{
				ReturnOperatorName.Text = operatorName;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditRetnOprt.Visibility = Visibility.Visible;
				else
					ImgEditRetnOprt.Visibility = Visibility.Collapsed;

				_isEditAllowed = value;
			}
		}
		public bool IsEditMode { get; private set; }

		public void CloseEdit()
		{
			if (IsEditMode)
			{
				IsEditMode = false;
				PageDispatcher.Invoke(new Action(() => {
					BorderRetnOprt.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(ReturnOperatorName.Text))
					{
						ReturnOperatorName.Visibility = Visibility.Collapsed;
						LineRetnOprt2.Visibility = Visibility.Collapsed;
					}
					else
					{
						ReturnOperatorName.Visibility = Visibility.Visible;
						LineRetnOprt2.Visibility = Visibility.Visible;
					}
					BorderRetnOprt.Background = _normalModeBackgroundSectionColor;
					LineRetnOprt1.Visibility = Visibility.Visible;
					ImgEditRetnOprt.Visibility = Visibility.Visible;
					LabelRetnOprtNm.Foreground = _labelNormalTextColor;
				}));
			}
		}

		public void OpenEdit()
		{
			if (IsEditAllowed)
			{
				if (!IsEditMode)
				{
					IsEditMode = true;
					PageDispatcher.Invoke(new Action(() => {
						BorderRetnOprt.Height = BorderRetnOprt.ActualHeight;
						BorderRetnOprt.Background = _editModeBackgroundSectionColor;

						ImgEditRetnOprt.Visibility = Visibility.Collapsed;
						ReturnOperatorName.Visibility = Visibility.Collapsed;
						LineRetnOprt1.Visibility = Visibility.Collapsed;
						LineRetnOprt2.Visibility = Visibility.Collapsed;

						LabelRetnOprtNm.Foreground = _labelEditTextColor;
					}));
				}
			}
		}

		/// <summary> Normally used in non-edit mode.
		/// </summary>
		/// <param name="isAllowEdit">A null to ignore</param>
		public void ShowData(bool? isAllowEdit = null)
		{
			PageDispatcher.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(ReturnOperatorName.Text))
				{
					ReturnOperatorName.Visibility = Visibility.Collapsed;
				}
				else
				{
					ReturnOperatorName.Visibility = Visibility.Visible;

					if (!IsEditMode)
						LineRetnOprt2.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditRetnOprt.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				BorderRetnOprt.Background = _normalModeBackgroundSectionColor;

				LabelRetnOprtNm.Foreground = _labelNormalTextColor;

				ReturnOperatorName.Text = "";

				ReturnOperatorName.Visibility = Visibility.Collapsed;
				LineRetnOprt1.Visibility = Visibility.Visible;
				LineRetnOprt2.Visibility = Visibility.Collapsed;
				ImgEditRetnOprt.Visibility = Visibility.Collapsed;
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
			BorderRetnOprt = null;
			LineRetnOprt1 = null;
			LineRetnOprt2 = null;
			ReturnOperatorName = null;
			ImgEditRetnOprt = null;
			PageDispatcher = null;
		}
	}
}