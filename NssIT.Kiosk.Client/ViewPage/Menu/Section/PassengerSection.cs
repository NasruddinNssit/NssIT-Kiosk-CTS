using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class PassengerSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;
		private Brush _dataTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xC1, 0x00));

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// Passenger : Passenger
		/// </summary>
		private Border SectionPassenger { get; set; }
		private TextBlock LabelPassenger { get; set; }
		private StackPanel PassengerList { get; set; }

		public PassengerSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdFrom, TextBlock lblPassenger, StackPanel stpPassengerList)
		{
			IsEditMode = false;

			stpPassengerList.Children.Clear();
			stpPassengerList.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdFrom, lblPassenger, stpPassengerList);

			_labelNormalTextColor = LabelPassenger.Foreground;

			PassengerList.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdToStation"></param>
		/// <param name="lblPassenger"></param>
		/// <param name="stpPassengerList"></param>
		/// <param name="imgPassenger"></param>
		public void ReInit(Border bdFromStation, TextBlock lblPassenger, StackPanel stpPassengerList)
		{
			SectionPassenger = bdFromStation;
			LabelPassenger = lblPassenger;
			PassengerList = stpPassengerList;
		}

		/// <summary>
		/// Set null to ignore setting
		/// </summary>
		/// <param name="passengerName"></param>
		/// <param name="passengerId"></param>
		public void AddPassengerData(string passengerName, string passengerId)
		{
			if (passengerName != null)
			{
				passengerName = passengerName ?? "-";
				passengerId = passengerId ?? "";

				PageDispatcher.Invoke(new Action(() => {
					PassengerList.Children.Add(new TextBlock()
					{
						Text = $@"{passengerName}  ({passengerId})",
						Margin = new Thickness(5, 0, 0, 0),
						TextWrapping = TextWrapping.Wrap,
						FontSize = 18,
						Foreground = _dataTextColor,
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center
					});
				}));
			}
		}

		public bool IsEditAllowed { get; set; }

		public bool IsEditMode { get; private set; }

		public void CloseEdit()
		{
			if (IsEditMode)
			{
				IsEditMode = false;
				PageDispatcher.Invoke(new Action(() => {
					SectionPassenger.Height = double.NaN;
					if (PassengerList.Children.Count == 0)
					{
						PassengerList.Visibility = Visibility.Collapsed;
					}
					else
					{
						PassengerList.Visibility = Visibility.Visible;
					}
					SectionPassenger.Background = _normalModeBackgroundSectionColor;
					LabelPassenger.Foreground = _labelNormalTextColor;
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
						SectionPassenger.Height = SectionPassenger.ActualHeight;
						SectionPassenger.Background = _editModeBackgroundSectionColor;

						PassengerList.Visibility = Visibility.Collapsed;

						LabelPassenger.Foreground = _labelEditTextColor;
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
				if (PassengerList.Children.Count == 0)
				{
					PassengerList.Visibility = Visibility.Collapsed;
				}
				else
				{
					PassengerList.Visibility = Visibility.Visible;
				}

			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				SectionPassenger.Background = _normalModeBackgroundSectionColor;

				LabelPassenger.Foreground = _labelNormalTextColor;

				PassengerList.Children.Clear();
				PassengerList.Visibility = Visibility.Collapsed;

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
			SectionPassenger = null;
			PassengerList = null;
			PageDispatcher = null;
		}
	}
}