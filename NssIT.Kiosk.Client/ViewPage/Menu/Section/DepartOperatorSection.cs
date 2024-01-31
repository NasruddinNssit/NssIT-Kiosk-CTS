using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public class DepartOperatorSection : IMenuItemSection, IDisposable
	{
		//private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _normalModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
		private Brush _editModeBackgroundSectionColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _labelEditTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private Brush _labelNormalTextColor = null;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		/// <summary>
		/// DepOprt : Depart Operator
		/// </summary>
		private Border BorderDepOprt { get; set; }
		private Image ImgEditDepOprt { get; set; }
		private Line LineDepOprt1 { get; set; }
		private Line LineDepOprt2 { get; set; }
		private TextBlock LabelDepOprtNm { get; set; }
		private TextBlock DepartOperatorName { get; set; }

		public DepartOperatorSection(System.Windows.Threading.Dispatcher dispatcher,
			Border bdDepartOperator, Line lnDepartOperator1, Line lnDepartOperator2, TextBlock lblDepartOperatorNm, TextBlock txtDepartOperatorNm, Image imgDepartOperator)
		{
			IsEditMode = false;

			txtDepartOperatorNm.Text = "";
			imgDepartOperator.Visibility = Visibility.Collapsed;

			PageDispatcher = dispatcher;

			ReInit(bdDepartOperator, lnDepartOperator1, lnDepartOperator2, lblDepartOperatorNm, txtDepartOperatorNm, imgDepartOperator);

			_labelNormalTextColor = LabelDepOprtNm.Foreground;

			DepartOperatorName.Visibility = Visibility.Collapsed;
			LineDepOprt1.Visibility = Visibility.Visible;
			LineDepOprt2.Visibility = Visibility.Collapsed;

			IsEditAllowed = false;
		}

		/// <summary>
		/// To refresh object pointer/reference
		/// </summary>
		/// <param name="bdDepartOperator"></param>
		/// <param name="lnDepartOperator1"></param>
		/// <param name="lnDepartOperator2"></param>
		/// <param name="lblDepartOperatorNm"></param>
		/// <param name="txtDepartOperatorNm"></param>
		/// <param name="imgDepartOperator"></param>
		public void ReInit(Border bdDepartOperator, Line lnDepartOperator1, Line lnDepartOperator2, TextBlock lblDepartOperatorNm, TextBlock txtDepartOperatorNm, Image imgDepartOperator)
		{
			BorderDepOprt = bdDepartOperator;
			LineDepOprt1 = lnDepartOperator1;
			LineDepOprt2 = lnDepartOperator2;
			LabelDepOprtNm = lblDepartOperatorNm;
			DepartOperatorName = txtDepartOperatorNm;
			ImgEditDepOprt = imgDepartOperator;
		}

		public void SetValue(string operatorName)
		{
			if (operatorName != null)
			{
				DepartOperatorName.Text = operatorName;
			}
		}

		private bool _isEditAllowed = false;
		public bool IsEditAllowed
		{
			get => _isEditAllowed;
			set
			{
				if (value == true)
					ImgEditDepOprt.Visibility = Visibility.Visible;
				else
					ImgEditDepOprt.Visibility = Visibility.Collapsed;

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
					BorderDepOprt.Height = double.NaN;
					if (string.IsNullOrWhiteSpace(DepartOperatorName.Text))
					{
						DepartOperatorName.Visibility = Visibility.Collapsed;
						LineDepOprt2.Visibility = Visibility.Collapsed;
					}
					else
					{
						DepartOperatorName.Visibility = Visibility.Visible;
						LineDepOprt2.Visibility = Visibility.Visible;
					}
					BorderDepOprt.Background = _normalModeBackgroundSectionColor;
					LineDepOprt1.Visibility = Visibility.Visible;
					ImgEditDepOprt.Visibility = Visibility.Visible;
					LabelDepOprtNm.Foreground = _labelNormalTextColor;
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
						BorderDepOprt.Height = BorderDepOprt.ActualHeight;
						BorderDepOprt.Background = _editModeBackgroundSectionColor;

						ImgEditDepOprt.Visibility = Visibility.Collapsed;
						DepartOperatorName.Visibility = Visibility.Collapsed;
						LineDepOprt1.Visibility = Visibility.Collapsed;
						LineDepOprt2.Visibility = Visibility.Collapsed;

						LabelDepOprtNm.Foreground = _labelEditTextColor;
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
				if (string.IsNullOrWhiteSpace(DepartOperatorName.Text))
				{
					DepartOperatorName.Visibility = Visibility.Collapsed;
				}
				else
				{
					DepartOperatorName.Visibility = Visibility.Visible;

					if (!IsEditMode)
						LineDepOprt2.Visibility = Visibility.Visible;
				}

				if (isAllowEdit.HasValue)
					ImgEditDepOprt.Visibility = isAllowEdit.Value == true ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void Reset()
		{
			IsEditAllowed = false;
			IsEditMode = false;
			PageDispatcher.Invoke(new Action(() => {
				BorderDepOprt.Background = _normalModeBackgroundSectionColor;

				LabelDepOprtNm.Foreground = _labelNormalTextColor;

				DepartOperatorName.Text = "";

				DepartOperatorName.Visibility = Visibility.Collapsed;
				LineDepOprt1.Visibility = Visibility.Visible;
				LineDepOprt2.Visibility = Visibility.Collapsed;
				ImgEditDepOprt.Visibility = Visibility.Collapsed;
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
			BorderDepOprt = null;
			LineDepOprt1 = null;
			LineDepOprt2 = null;
			DepartOperatorName = null;
			ImgEditDepOprt = null;
			PageDispatcher = null;
		}
	}
}