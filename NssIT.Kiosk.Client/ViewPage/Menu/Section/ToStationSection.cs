using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class ToStationSection : IMenuItemSection, IDisposable
	{
		private const string LogChannel = "ViewPage";

		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// ToStt : To Station
		/// </summary>
		private Border SectionToStt { get; set; }
		private Image ImgEditToStt { get; set; }
		private TextBlock LabelToStt { get; set; }
		private TextBlock ToStationDesc { get; set; }

		//public event EventHandler<MenuItemEditEventArgs> OnEditToStation;

		public ToStationSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdTo, TextBlock lblToStt, TextBlock txtToStt, Image imgToStt)
		{
			IsEditMode = false;
			txtToStt.Text = "";
			imgToStt.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdTo, lblToStt, txtToStt, imgToStt);

			_labelNormalTextColor = LabelToStt.Foreground;

			ToStationDesc.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		//public void RaiseOnEditToStation()
		//{
		//	try
		//	{
		//		if (OnEditToStation != null)
		//		{
		//			OnEditToStation.Invoke(null, new MenuItemEditEventArgs(TickSalesMenuItemCode.ToStation));
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "ToStationSection.RaiseOnEditToStation");
		//	}
		//}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdToStation"></param>
		/// <param name="lblToStt"></param>
		/// <param name="txtToStt"></param>
		/// <param name="imgToStt"></param>
		public void ReInit(Border bdToStation, TextBlock lblToStt, TextBlock txtToStt, Image imgToStt)
		{
			SectionToStt = bdToStation;
			LabelToStt = lblToStt;
			ToStationDesc = txtToStt;
			ImgEditToStt = imgToStt;
		}

		/// <summary>
		/// Set null to ignore setting
		/// </summary>
		/// <param name="destinationName"></param>
		public void SetValue(string destinationName)
		{
			if (destinationName != null)
			{
				ToStationDesc.Text = destinationName;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditToStt.Visibility = Visibility.Visible;
				else
					ImgEditToStt.Visibility = Visibility.Collapsed;

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
					SectionToStt.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(ToStationDesc.Text))
					{
						ToStationDesc.Visibility = Visibility.Collapsed;
					}
					else
					{
						ToStationDesc.Visibility = Visibility.Visible;
					}
					SectionToStt.Background = _normalModeBackgroundSectionColor;
					ImgEditToStt.Visibility = Visibility.Visible;
					LabelToStt.Foreground = _labelNormalTextColor;
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
						SectionToStt.Height = SectionToStt.ActualHeight;
						SectionToStt.Background = _editModeBackgroundSectionColor;

						ImgEditToStt.Visibility = Visibility.Collapsed;
						ToStationDesc.Visibility = Visibility.Collapsed;

						LabelToStt.Foreground = _labelEditTextColor;
					}));
					//RaiseOnEditToStation();
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="isAllowEdit">A null to ignore</param>
		public void ShowData(bool? isAllowEdit = null)
		{
			PageDispatcher.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(ToStationDesc.Text))
				{
					ToStationDesc.Visibility = Visibility.Collapsed;
				}
				else
				{
					ToStationDesc.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditToStt.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionToStt.Background = _normalModeBackgroundSectionColor;

				LabelToStt.Foreground = _labelNormalTextColor;

				ToStationDesc.Text = "";
				ToStationDesc.Visibility = Visibility.Collapsed;

				ImgEditToStt.Visibility = Visibility.Collapsed;
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
			SectionToStt = null;
			ToStationDesc = null;
			ImgEditToStt = null;
			PageDispatcher = null;
		}
	}
}