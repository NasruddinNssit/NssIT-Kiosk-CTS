using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class ReturnSeatSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// RetnSeat : Return Seat
		/// </summary>
		private Border BorderRetnSeat { get; set; }
		private Image ImgEditRetnSeat { get; set; }
		private Line LineRetnSeat1 { get; set; }
		private Line LineRetnSeat2 { get; set; }
		private TextBlock LabelRetnSeatDesc { get; set; }
		private TextBlock ReturnSeatDesc { get; set; }

		public ReturnSeatSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdReturnSeat, Line lnReturnSeat1, Line lnReturnSeat2, TextBlock lblReturnSeat, TextBlock txtReturnSeat, Image imgReturnSeat)
		{
			IsEditMode = false;

			txtReturnSeat.Text = "";
			imgReturnSeat.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdReturnSeat, lnReturnSeat1, lnReturnSeat2, lblReturnSeat, txtReturnSeat, imgReturnSeat);

			_labelNormalTextColor = LabelRetnSeatDesc.Foreground;

			ReturnSeatDesc.Visibility = Visibility.Collapsed;
			LineRetnSeat1.Visibility = Visibility.Visible;
			LineRetnSeat2.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdReturnSeat"></param>
		/// <param name="lnReturnSeat1"></param>
		/// <param name="lnReturnSeat2"></param>
		/// <param name="lblReturnSeat"></param>
		/// <param name="txtReturnSeat"></param>
		/// <param name="imgReturnSeat"></param>
		public void ReInit(Border bdReturnSeat, Line lnReturnSeat1, Line lnReturnSeat2, TextBlock lblReturnSeat, TextBlock txtReturnSeat, Image imgReturnSeat)
		{
			BorderRetnSeat = bdReturnSeat;
			LineRetnSeat1 = lnReturnSeat1;
			LineRetnSeat2 = lnReturnSeat2;
			LabelRetnSeatDesc = lblReturnSeat;
			ReturnSeatDesc = txtReturnSeat;
			ImgEditRetnSeat = imgReturnSeat;
		}

		public void SetValue(string seatDesc)
		{
			if (seatDesc != null)
			{
				ReturnSeatDesc.Text = seatDesc;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditRetnSeat.Visibility = Visibility.Visible;
				else
					ImgEditRetnSeat.Visibility = Visibility.Collapsed;

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
					BorderRetnSeat.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(ReturnSeatDesc.Text))
					{
						ReturnSeatDesc.Visibility = Visibility.Collapsed;
						LineRetnSeat2.Visibility = Visibility.Collapsed;
					}
					else
					{
						ReturnSeatDesc.Visibility = Visibility.Visible;
						LineRetnSeat2.Visibility = Visibility.Visible;
					}
					BorderRetnSeat.Background = _normalModeBackgroundSectionColor;
					LineRetnSeat1.Visibility = Visibility.Visible;
					ImgEditRetnSeat.Visibility = Visibility.Visible;
					LabelRetnSeatDesc.Foreground = _labelNormalTextColor;
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
						BorderRetnSeat.Height = BorderRetnSeat.ActualHeight;
						BorderRetnSeat.Background = _editModeBackgroundSectionColor;

						ImgEditRetnSeat.Visibility = Visibility.Collapsed;
						ReturnSeatDesc.Visibility = Visibility.Collapsed;
						LineRetnSeat1.Visibility = Visibility.Collapsed;
						LineRetnSeat2.Visibility = Visibility.Collapsed;

						LabelRetnSeatDesc.Foreground = _labelEditTextColor;
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
				if (string.IsNullOrWhiteSpace(ReturnSeatDesc.Text))
				{
					ReturnSeatDesc.Visibility = Visibility.Collapsed;
				}
				else
				{
					ReturnSeatDesc.Visibility = Visibility.Visible;

					if (!IsEditMode)
						LineRetnSeat2.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditRetnSeat.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				BorderRetnSeat.Background = _normalModeBackgroundSectionColor;

				LabelRetnSeatDesc.Foreground = _labelNormalTextColor;

				ReturnSeatDesc.Text = "";

				ReturnSeatDesc.Visibility = Visibility.Collapsed;
				LineRetnSeat1.Visibility = Visibility.Visible;
				LineRetnSeat2.Visibility = Visibility.Collapsed;
				ImgEditRetnSeat.Visibility = Visibility.Collapsed;
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
			BorderRetnSeat = null;
			LineRetnSeat1 = null;
			LineRetnSeat2 = null;
			ReturnSeatDesc = null;
			ImgEditRetnSeat = null;
			PageDispatcher = null;
		}
	}
}