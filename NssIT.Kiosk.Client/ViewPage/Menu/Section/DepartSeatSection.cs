using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class DepartSeatSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// DepSeat : Depart Seat
		/// </summary>
		private Border BorderDepSeat { get; set; }
		private Image ImgEditDepSeat { get; set; }
		private Line LineDepSeat1 { get; set; }
		private Line LineDepSeat2 { get; set; }
		private TextBlock LabelDepSeatDesc { get; set; }
		private TextBlock DepartSeatDesc { get; set; }

		public DepartSeatSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdDepartSeat, Line lnDepartSeat1, Line lnDepartSeat2, TextBlock lblDepartSeat, TextBlock txtDepartSeat, Image imgDepartSeat)
		{
			IsEditMode = false;

			txtDepartSeat.Text = "";
			imgDepartSeat.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdDepartSeat, lnDepartSeat1, lnDepartSeat2, lblDepartSeat, txtDepartSeat, imgDepartSeat);

			_labelNormalTextColor = LabelDepSeatDesc.Foreground;

			DepartSeatDesc.Visibility = Visibility.Collapsed;
			LineDepSeat1.Visibility = Visibility.Visible;
			LineDepSeat2.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdDepartSeat"></param>
		/// <param name="lnDepartSeat1"></param>
		/// <param name="lnDepartSeat2"></param>
		/// <param name="lblDepartSeat"></param>
		/// <param name="txtDepartSeat"></param>
		/// <param name="imgDepartSeat"></param>
		public void ReInit(Border bdDepartSeat, Line lnDepartSeat1, Line lnDepartSeat2, TextBlock lblDepartSeat, TextBlock txtDepartSeat, Image imgDepartSeat)
		{
			BorderDepSeat = bdDepartSeat;
			LineDepSeat1 = lnDepartSeat1;
			LineDepSeat2 = lnDepartSeat2;
			LabelDepSeatDesc = lblDepartSeat;
			DepartSeatDesc = txtDepartSeat;
			ImgEditDepSeat = imgDepartSeat;
		}

		public void SetValue(string seatDesc)
		{
			if (seatDesc != null)
			{
				DepartSeatDesc.Text = seatDesc;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditDepSeat.Visibility = Visibility.Visible;
				else
					ImgEditDepSeat.Visibility = Visibility.Collapsed;

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
					BorderDepSeat.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(DepartSeatDesc.Text))
					{
						DepartSeatDesc.Visibility = Visibility.Collapsed;
						LineDepSeat2.Visibility = Visibility.Collapsed;
					}
					else
					{
						DepartSeatDesc.Visibility = Visibility.Visible;
						LineDepSeat2.Visibility = Visibility.Visible;
					}
					BorderDepSeat.Background = _normalModeBackgroundSectionColor;
					LineDepSeat1.Visibility = Visibility.Visible;
					ImgEditDepSeat.Visibility = Visibility.Visible;
					LabelDepSeatDesc.Foreground = _labelNormalTextColor;
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
						BorderDepSeat.Height = BorderDepSeat.ActualHeight;
						BorderDepSeat.Background = _editModeBackgroundSectionColor;

						ImgEditDepSeat.Visibility = Visibility.Collapsed;
						DepartSeatDesc.Visibility = Visibility.Collapsed;
						LineDepSeat1.Visibility = Visibility.Collapsed;
						LineDepSeat2.Visibility = Visibility.Collapsed;

						LabelDepSeatDesc.Foreground = _labelEditTextColor;
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
				if (string.IsNullOrWhiteSpace(DepartSeatDesc.Text))
				{
					DepartSeatDesc.Visibility = Visibility.Collapsed;
				}
				else
				{
					DepartSeatDesc.Visibility = Visibility.Visible;

					if (!IsEditMode)
						LineDepSeat2.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditDepSeat.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				BorderDepSeat.Background = _normalModeBackgroundSectionColor;

				LabelDepSeatDesc.Foreground = _labelNormalTextColor;

				DepartSeatDesc.Text = "";

				DepartSeatDesc.Visibility = Visibility.Collapsed;
				LineDepSeat1.Visibility = Visibility.Visible;
				LineDepSeat2.Visibility = Visibility.Collapsed;
				ImgEditDepSeat.Visibility = Visibility.Collapsed;
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
			BorderDepSeat = null;
			LineDepSeat1 = null;
			LineDepSeat2 = null;
			DepartSeatDesc = null;
			ImgEditDepSeat = null;
			PageDispatcher = null;
		}
	}
}
