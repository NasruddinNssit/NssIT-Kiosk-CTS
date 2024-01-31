using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class ReturnDateSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// RetnDate : Return Date
		/// </summary>
		private Border SectionRetnDate { get; set; }
		private Image ImgEditRetnDate { get; set; }
		private TextBlock LabelRetnDate { get; set; }
		private TextBlock ReturnDateString { get; set; }

		public ReturnDateSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdReturnDate, TextBlock lblReturnDate, TextBlock txtReturnDate, Image imgReturnDate)
		{
			IsEditMode = false;
			txtReturnDate.Text = "";
			imgReturnDate.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdReturnDate, lblReturnDate, txtReturnDate, imgReturnDate);

			_labelNormalTextColor = LabelRetnDate.Foreground;

			ReturnDateString.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdReturnDate"></param>
		/// <param name="lblReturnDate"></param>
		/// <param name="txtReturnDate"></param>
		/// <param name="imgReturnDate"></param>
		public void ReInit(Border bdReturnDate, TextBlock lblReturnDate, TextBlock txtReturnDate, Image imgReturnDate)
		{
			SectionRetnDate = bdReturnDate;
			LabelRetnDate = lblReturnDate;
			ReturnDateString = txtReturnDate;
			ImgEditRetnDate = imgReturnDate;
		}

		/// <summary>
		/// Set null to ignore setting
		/// </summary>
		/// <param name="returnTime"></param>
		public void SetValue(DateTime? returnTime)
		{
			if (returnTime.HasValue)
			{
				ReturnDateString.Text = returnTime.Value.ToString("dd MMM yyyy HH:mm");
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditRetnDate.Visibility = Visibility.Visible;
				else
					ImgEditRetnDate.Visibility = Visibility.Collapsed;

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
					SectionRetnDate.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(ReturnDateString.Text))
					{
						ReturnDateString.Visibility = Visibility.Collapsed;
					}
					else
					{
						ReturnDateString.Visibility = Visibility.Visible;
					}
					SectionRetnDate.Background = _normalModeBackgroundSectionColor;
					ImgEditRetnDate.Visibility = Visibility.Visible;
					LabelRetnDate.Foreground = _labelNormalTextColor;
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
						SectionRetnDate.Height = SectionRetnDate.ActualHeight;
						SectionRetnDate.Background = _editModeBackgroundSectionColor;

						ImgEditRetnDate.Visibility = Visibility.Collapsed;
						ReturnDateString.Visibility = Visibility.Collapsed;

						LabelRetnDate.Foreground = _labelEditTextColor;
					}));
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="isAllowEdit">A null to ignore</param>
		public void ShowData(bool? isAllowEdit = null)
		{
			PageDispatcher.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(ReturnDateString.Text))
				{
					ReturnDateString.Visibility = Visibility.Collapsed;
				}
				else
				{
					ReturnDateString.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditRetnDate.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionRetnDate.Background = _normalModeBackgroundSectionColor;

				LabelRetnDate.Foreground = _labelNormalTextColor;

				ReturnDateString.Text = "";

				ReturnDateString.Visibility = Visibility.Collapsed;
				ImgEditRetnDate.Visibility = Visibility.Collapsed;
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
			SectionRetnDate = null;
			ReturnDateString = null;
			ImgEditRetnDate = null;
			PageDispatcher = null;
		}
	}
}