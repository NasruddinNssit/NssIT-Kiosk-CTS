using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class FromStationSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// FromStt : From Station
		/// </summary>
		private Border SectionFromStt { get; set; }
		private Image ImgEditFromStt { get; set; }
		private TextBlock LabelFromStt { get; set; }
		private TextBlock FromStationDesc { get; set; }

		public FromStationSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdFrom, TextBlock lblFromStt, TextBlock txtFromStt, Image imgFromStt)
		{
			IsEditMode = false;
			txtFromStt.Text = "";
			imgFromStt.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdFrom, lblFromStt, txtFromStt, imgFromStt);

			_labelNormalTextColor = LabelFromStt.Foreground;

			FromStationDesc.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdToStation"></param>
		/// <param name="lblFromStt"></param>
		/// <param name="txtFromStt"></param>
		/// <param name="imgFromStt"></param>
		public void ReInit(Border bdFromStation, TextBlock lblFromStt, TextBlock txtFromStt, Image imgFromStt)
		{
			SectionFromStt = bdFromStation;
			LabelFromStt = lblFromStt;
			FromStationDesc = txtFromStt;
			ImgEditFromStt = imgFromStt;
		}

		/// <summary>
		/// Set null to ignore setting
		/// </summary>
		/// <param name="destinationName"></param>
		public void SetValue(string departureStationName)
		{
			if (departureStationName != null)
			{
				FromStationDesc.Text = departureStationName;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditFromStt.Visibility = Visibility.Visible;
				else
					ImgEditFromStt.Visibility = Visibility.Collapsed;

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
					SectionFromStt.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(FromStationDesc.Text))
					{
						FromStationDesc.Visibility = Visibility.Collapsed;
					}
					else
					{
						FromStationDesc.Visibility = Visibility.Visible;
					}
					SectionFromStt.Background = _normalModeBackgroundSectionColor;
					ImgEditFromStt.Visibility = Visibility.Visible;
					LabelFromStt.Foreground = _labelNormalTextColor;
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
					PageDispatcher.Invoke(new Action(() =>
					{
						SectionFromStt.Height = SectionFromStt.ActualHeight;
						SectionFromStt.Background = _editModeBackgroundSectionColor;

						ImgEditFromStt.Visibility = Visibility.Collapsed;
						FromStationDesc.Visibility = Visibility.Collapsed;

						LabelFromStt.Foreground = _labelEditTextColor;
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
				if (string.IsNullOrWhiteSpace(FromStationDesc.Text))
				{
					FromStationDesc.Visibility = Visibility.Collapsed;
				}
				else
				{
					FromStationDesc.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditFromStt.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionFromStt.Background = _normalModeBackgroundSectionColor;

				LabelFromStt.Foreground = _labelNormalTextColor;

				FromStationDesc.Text = "";
				FromStationDesc.Visibility = Visibility.Collapsed;

				ImgEditFromStt.Visibility = Visibility.Collapsed;
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
			SectionFromStt = null;
			FromStationDesc = null;
			ImgEditFromStt = null;
			PageDispatcher = null;
		}
	}
}
