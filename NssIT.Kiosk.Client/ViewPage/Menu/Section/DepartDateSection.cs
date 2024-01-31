using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class DepartDateSection : IMenuItemSection, IDisposable
	{
		private const string LogChannel = "ViewPage";

		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// DepDate : Depart Date
		/// </summary>
		private Border SectionDepDate { get; set; }
		private Image ImgEditDepDate { get; set; }
		private TextBlock LabelDepDate { get; set; }
		private TextBlock DepartDateString { get; set; }

		//public event EventHandler<MenuItemEditEventArgs> OnEditDepartDate;

		//public void RaiseOnEditDepartDate()
		//{
		//	try
		//	{
		//		if (OnEditDepartDate != null)
		//		{
		//			OnEditDepartDate.Invoke(null, new MenuItemEditEventArgs(TickSalesMenuItemCode.DepartDate));
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "DepartDateSection.RaiseOnDepartDate");
		//	}
		//}

		public DepartDateSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdDepartDate, TextBlock lblDepartDate, TextBlock txtDepartDate, Image imgDepartDate)
		{
			IsEditMode = false;
			txtDepartDate.Text = "";
			imgDepartDate.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdDepartDate, lblDepartDate, txtDepartDate, imgDepartDate);

			_labelNormalTextColor = LabelDepDate.Foreground;

			DepartDateString.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdDepartDate"></param>
		/// <param name="lblDepartDate"></param>
		/// <param name="txtDepartDate"></param>
		/// <param name="imgDepartDate"></param>
		public void ReInit(Border bdDepartDate, TextBlock lblDepartDate, TextBlock txtDepartDate, Image imgDepartDate)
		{
			SectionDepDate = bdDepartDate;
			LabelDepDate = lblDepartDate;
			DepartDateString = txtDepartDate;
			ImgEditDepDate = imgDepartDate;
		}

		/// <summary>
		/// Set null to ignore setting
		/// </summary>
		/// <param name="departTime"></param>
		public void SetValue(string dateTimeString)
		{
			DepartDateString.Text = dateTimeString;
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditDepDate.Visibility = Visibility.Visible;
				else
					ImgEditDepDate.Visibility = Visibility.Collapsed;

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
					SectionDepDate.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(DepartDateString.Text))
					{
						DepartDateString.Visibility = Visibility.Collapsed;
					}
					else
					{
						DepartDateString.Visibility = Visibility.Visible;
					}
					SectionDepDate.Background = _normalModeBackgroundSectionColor;
					ImgEditDepDate.Visibility = Visibility.Visible;
					LabelDepDate.Foreground = _labelNormalTextColor;
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
						SectionDepDate.Height = SectionDepDate.ActualHeight;
						SectionDepDate.Background = _editModeBackgroundSectionColor;

						ImgEditDepDate.Visibility = Visibility.Collapsed;
						DepartDateString.Visibility = Visibility.Collapsed;

						LabelDepDate.Foreground = _labelEditTextColor;
					}));

					//RaiseOnEditDepartDate();
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="isAllowEdit">A null to ignore</param>
		public void ShowData(bool? isAllowEdit = null)
		{
			PageDispatcher.Invoke(new Action(() => {
				if (string.IsNullOrWhiteSpace(DepartDateString.Text))
				{
					DepartDateString.Visibility = Visibility.Collapsed;
				}
				else
				{
					DepartDateString.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditDepDate.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionDepDate.Background = _normalModeBackgroundSectionColor;

				LabelDepDate.Foreground = _labelNormalTextColor;

				DepartDateString.Text = "";

				DepartDateString.Visibility = Visibility.Collapsed;
				ImgEditDepDate.Visibility = Visibility.Collapsed;
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
			SectionDepDate = null;
			DepartDateString = null;
			ImgEditDepDate = null;
			PageDispatcher = null;
		}
	}
}