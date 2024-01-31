using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Trip;

namespace NssIT.Kiosk.Client.ViewPage.Menu
{
	/// <summary>
	/// Interaction logic for pgMenu.xaml
	/// </summary>
	public partial class pgMenu : Page, IMenuExec
	{
		private const string LogChannel = "MainWindow";

		private FromStationSection _fromStationSection = null;
		private ToStationSection _toStationSection = null;

		private DepartDateSection _departDateSection = null;
		private DepartOperatorSection _departOperatorSection = null;
		private DepartSeatSection _departSeatSection = null;

		private ReturnDateSection _returnDateSection = null;
		private ReturnOperatorSection _returnOperatorSection = null;
		private ReturnSeatSection _returnSeatSection = null;

		private PassengerSection _passengerSection = null;
		private PaymentSection _paymentSection = null;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _currentLanguage = LanguageCode.English;

		private ITimeFilter _timeFilterPage = null;

		//private MenuEditTemporarySwitch _menuEditTemporarySwitch = new MenuEditTemporarySwitch();

		public event EventHandler<MenuItemEditEventArgs> OnEditMenuItem;
		public event EventHandler<MenuItemPageNavigateEventArgs> OnPageNavigateChanged;

		public pgMenu()
		{
			InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Menu\LanguageMalay01.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Menu\LanguageEnglish01.xaml");

			_fromStationSection = new FromStationSection(this.Dispatcher, BdFrom, LblFromStation, TxtFromStationNm, ImgFromStation);
			_toStationSection = new ToStationSection(this.Dispatcher, BdTo, LblToStation, TxtToStationNm, ImgToStation);

			_departDateSection = new DepartDateSection(this.Dispatcher, BdDepartDate, LblDepartDate, TxtDepartDate, ImgDepartDate);
			_departOperatorSection = new DepartOperatorSection(this.Dispatcher, BdDepartOperator, LnDepartOperator1, LnDepartOperator2, LblDepartOperatorNm, TxtDepartOperatorNm, ImgDepartOperator);
			_departSeatSection = new DepartSeatSection(this.Dispatcher, BdDepartSeat, LnDepartSeat1, LnDepartSeat2, LblDepartSeat, TxtDepartSeat, ImgDepartSeat);

			_returnDateSection = new ReturnDateSection(this.Dispatcher, BdReturnDate, LblReturnDate, TxtReturnDate, ImgReturnDate);
			_returnOperatorSection = new ReturnOperatorSection(this.Dispatcher, BdReturnOperator, LnReturnOperator1, LnReturnOperator2, LblReturnOperatorNm, TxtReturnOperatorNm, ImgReturnOperator);
			_returnSeatSection = new ReturnSeatSection(this.Dispatcher, BdReturnSeat, LnReturnSeat1, LnReturnSeat2, LblReturnSeat, TxtReturnSeat, ImgReturnSeat);

			_passengerSection = new PassengerSection(this.Dispatcher, BdPassenger, LblPassenger, StpPassengerList);
			_paymentSection = new PaymentSection(this.Dispatcher, BdPayment, LblPayment, TxtPaymentTypeDesc);

			//_departDateSection.OnEditDepartDate += MenuItem_OnEdit;
			//_toStationSection.OnEditToStation += MenuItem_OnEdit;
			// DEBUG-Testing .. testing only
			//_fromStationSection.IsEditAllowed = true;
			//_toStationSection.IsEditAllowed = true;
			//_departDateSection.IsEditAllowed = true;
			//_departOperatorSection.IsEditAllowed = true;
			//_departSeatSection.IsEditAllowed = true;
			//_returnDateSection.IsEditAllowed = true;
			//_returnOperatorSection.IsEditAllowed = true;
			//_returnSeatSection.IsEditAllowed = true;
			//_passengerSection.IsEditAllowed = true;
			//_paymentSection.IsEditAllowed = true;
			//----------------------------------------

			if (App.AvailableTravelMode == TravelMode.DepartOnly)
			{
				BdReturnDate.Visibility = Visibility.Collapsed;
				BdReturnOperator.Visibility = Visibility.Collapsed;
				BdReturnSeat.Visibility = Visibility.Collapsed;
			}
		}

		public void ShieldMenu()
		{
			BdMenuShield.Visibility = Visibility.Visible;
		}

		public void UnShieldMenu()
		{
			BdMenuShield.Visibility = Visibility.Collapsed;
		}

		//private void MenuItem_OnEdit(object sender, MenuItemEditEventArgs e)
		//{
		//	try
		//	{
		//		RaiseOnEditMenuItem(e.EditItemCode);
		//	}
		//	catch (Exception ex)
		//	{
		//		App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "pgMenu.RaiseOnEditMenuItem");
		//	}
		//}

		private void RaiseOnEditMenuItem(TickSalesMenuItemCode menuItem)
		{
			try
			{
				if (OnEditMenuItem != null)
				{
					OnEditMenuItem.Invoke(null, new MenuItemEditEventArgs(menuItem));
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "pgMenu.RaiseOnEditMenuItem");
			}
		}

		private void RaiseOnPageNavigateChanged(PageNavigateDirection pageNav)
		{
			try
			{
				if (OnPageNavigateChanged != null)
				{
					OnPageNavigateChanged.Invoke(null, new MenuItemPageNavigateEventArgs(pageNav));
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "pgMenu.RaiseOnPageNavigateChanged");
			}
		}

		private void ActivateMenuItem(object sender, MouseButtonEventArgs e)
		{
			//return;
			try
			{
				if (sender is Border section)
				{
					//_fromStationSection.CloseEdit();
					//_toStationSection.CloseEdit();
					//_departDateSection.CloseEdit();
					//_departOperatorSection.CloseEdit();
					//_departSeatSection.CloseEdit();

					//_returnDateSection.CloseEdit();
					//_returnOperatorSection.CloseEdit();
					//_returnSeatSection.CloseEdit();
					//_passengerSection.CloseEdit();
					//_paymentSection.CloseEdit();

					TickSalesMenuItemCode? editItemCode = null;

					if ((section.Name.Equals(BdFrom.Name)) && (IsEditAllowedFromStation) && (_fromStationSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.FromStation;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdTo.Name)) && (IsEditAllowedToStation) && (_toStationSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.ToStation;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdDepartDate.Name)) && (IsEditAllowedDepartDate) && (_departDateSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.DepartDate;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdDepartOperator.Name)) && (IsEditAllowedDepartOperator) && (_departOperatorSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.DepartOperator;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdDepartSeat.Name)) && (IsEditAllowedDepartSeat) && (_departSeatSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.DepartSeat;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdReturnDate.Name)) && (IsEditAllowedReturnDate) && (_returnDateSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.ReturnDate;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdReturnOperator.Name)) && (IsEditAllowedReturnOperator) && (_returnOperatorSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.ReturnOperator;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdReturnSeat.Name)) && (IsEditAllowedReturnSeat) && (_returnSeatSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.ReturnSeat;
						ActivateEditMenuItem(editItemCode.Value);
					}
					else if ((section.Name.Equals(BdPassenger.Name)) && (IsEditAllowedPassenger) && (_passengerSection.IsEditMode == false))
					{
						//return;

						CloseAllEdit();

						editItemCode = TickSalesMenuItemCode.Passenger;
						ActivateEditMenuItem(editItemCode.Value);
					}
					//else if ((section.Name.Equals(BdPayment.Name)) && (false))
					//{
					//	CloseAllEdit();

					//	editItemCode = TickSalesMenuItemCode.Payment;
					//	ActivateEditMenuItem(editItemCode.Value);
					//}

					//return;
					if (editItemCode.HasValue)
						RaiseOnEditMenuItem(editItemCode.Value);
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMenu.ActivateMenuItem");
				App.ShowDebugMsg($@"{ex.Message}; At pgMenu.ActivateMenuItem");
			}

			void CloseAllEdit()
			{
				_fromStationSection.CloseEdit();
				_toStationSection.CloseEdit();
				_departDateSection.CloseEdit();
				_departOperatorSection.CloseEdit();
				_departSeatSection.CloseEdit();

				_returnDateSection.CloseEdit();
				_returnOperatorSection.CloseEdit();
				_returnSeatSection.CloseEdit();
				_passengerSection.CloseEdit();
				_paymentSection.CloseEdit();
			}
		}

		public void ActivateEditMenuItem(TickSalesMenuItemCode menuItemCode)
		{
			try
			{
				IMenuItemSection itemSection = null;
				switch (menuItemCode)
				{
					case TickSalesMenuItemCode.Non:
						break;
					case TickSalesMenuItemCode.FromStation:
						itemSection = _fromStationSection;
						break;
					case TickSalesMenuItemCode.ToStation:
						itemSection = _toStationSection;
						break;
					case TickSalesMenuItemCode.DepartDate:
						itemSection = _departDateSection;
						break;
					case TickSalesMenuItemCode.DepartOperator:
						itemSection = _departOperatorSection;
						break;
					case TickSalesMenuItemCode.DepartSeat:
						itemSection = _departSeatSection;
						break;
					case TickSalesMenuItemCode.ReturnDate:
						itemSection = _returnDateSection;
						break;
					case TickSalesMenuItemCode.ReturnOperator:
						itemSection = _returnOperatorSection;
						break;
					case TickSalesMenuItemCode.ReturnSeat:
						itemSection = _returnSeatSection;
						break;
					case TickSalesMenuItemCode.Passenger:
						itemSection = _passengerSection;
						break;
					case TickSalesMenuItemCode.Payment:
						itemSection = _paymentSection;
						break;
				}

				if (itemSection != null)
				{
					_fromStationSection.CloseEdit();
					_toStationSection.CloseEdit();
					_departDateSection.CloseEdit();
					_departOperatorSection.CloseEdit();
					_departSeatSection.CloseEdit();

					_returnDateSection.CloseEdit();
					_returnOperatorSection.CloseEdit();
					_returnSeatSection.CloseEdit();
					_passengerSection.CloseEdit();
					_paymentSection.CloseEdit();

					if (itemSection.IsEditAllowed == false)
						itemSection.IsEditAllowed = true;

					itemSection.OpenEdit();
					//System.Windows.Forms.Application.DoEvents();
				}
				else if ((menuItemCode == TickSalesMenuItemCode.Non) || (menuItemCode == TickSalesMenuItemCode.AfterPayment))
				{
					_fromStationSection.CloseEdit();
					_toStationSection.CloseEdit();
					_departDateSection.CloseEdit();
					_departOperatorSection.CloseEdit();
					_departSeatSection.CloseEdit();

					_returnDateSection.CloseEdit();
					_returnOperatorSection.CloseEdit();
					_returnSeatSection.CloseEdit();
					_passengerSection.CloseEdit();
					_paymentSection.CloseEdit();
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMenu.ActivateEditMenuItem");
				App.ShowDebugMsg($@"{ex.Message}; At pgMenu.ActivateEditMenuItem");
			}
		}

		private void BdPrevious_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			RaiseOnPageNavigateChanged(PageNavigateDirection.Previous);
		}
		private void BdExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			RaiseOnPageNavigateChanged(PageNavigateDirection.Exit);
		}

		public void SetLanguage(LanguageCode language)
		{
			try
			{
				if (_currentLanguage != language)
				{
					this.Resources.MergedDictionaries.Clear();

					if (language == LanguageCode.Malay )
						this.Resources.MergedDictionaries.Add(_langMal);
					else
						this.Resources.MergedDictionaries.Add(_langEng);

					_currentLanguage = language;
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT10000401A); pgMenu.SetLanguage");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000401A)", ex), "EX01", "pgMenu.SetLanguage");
			}
		}

		public bool IsEditAllowedFromStation
		{
			get => _fromStationSection.IsEditAllowed;
			set => _fromStationSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedToStation
		{
			get => _toStationSection.IsEditAllowed;
			set => _toStationSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedDepartDate
		{
			get => _departDateSection.IsEditAllowed;
			set => _departDateSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedDepartOperator
		{
			get => _departOperatorSection.IsEditAllowed;
			set => _departOperatorSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedDepartSeat
		{
			get => _departSeatSection.IsEditAllowed;
			set => _departSeatSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedReturnDate
		{
			get => _returnDateSection.IsEditAllowed;
			set => _returnDateSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedReturnOperator
		{
			get => _returnOperatorSection.IsEditAllowed;
			set => _returnOperatorSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedReturnSeat
		{
			get => _returnSeatSection.IsEditAllowed;
			set => _returnSeatSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedPassenger
		{
			get => _passengerSection.IsEditAllowed;
			set => _passengerSection.IsEditAllowed = value;
		}

		public bool IsEditAllowedPayment
		{
			get => _paymentSection.IsEditAllowed;
			set => _paymentSection.IsEditAllowed = value;
		}

		public void SetFromStationData(string departureStationName) => _fromStationSection.SetValue(departureStationName);

		public void SetToStationData(string destinationName) => _toStationSection.SetValue(destinationName);

		public void SetDepartDateData(string departTimeString) => _departDateSection.SetValue(departTimeString);

		public void SetDepartOperatorData(string operatorName) => _departOperatorSection.SetValue(operatorName);

		public void SetDepartSeatData(string seatDesc) => _departSeatSection.SetValue(seatDesc);

		public void SetReturnDateData(DateTime? returnTime) => _returnDateSection.SetValue(returnTime);

		public void SetReturnOperatorData(string operatorName) => _returnOperatorSection.SetValue(operatorName);

		public void SetReturnSeatData(string seatDesc) => _returnSeatSection.SetValue(seatDesc);

		public void AddPassengerData(string passengerName, string passengerId) => _passengerSection.AddPassengerData(passengerName, passengerId);

		public void SetPaymentData(string paymentTypeDesc) => _paymentSection.SetValue(paymentTypeDesc);


		public void ShowFromStationData(bool? isAllowEdit = null) => _fromStationSection.ShowData(isAllowEdit);

		public void ShowToStationData(bool? isAllowEdit = null) => _toStationSection.ShowData(isAllowEdit);

		public void ShowDepartDateData(bool? isAllowEdit = null) => _departDateSection.ShowData(isAllowEdit);

		public void ShowDepartOperatorData(bool? isAllowEdit = null) => _departOperatorSection.ShowData(isAllowEdit);

		public void ShowDepartSeatData(bool? isAllowEdit = null) => _departSeatSection.ShowData(isAllowEdit);

		public void ShowReturnDateData(bool? isAllowEdit = null) => _returnDateSection.ShowData(isAllowEdit);

		public void ShowReturnOperatorData(bool? isAllowEdit = null) => _returnOperatorSection.ShowData(isAllowEdit);

		public void ShowReturnSeatData(bool? isAllowEdit = null) => _returnSeatSection.ShowData(isAllowEdit);

		public void ShowPassengerData(bool? isAllowEdit = null) => _passengerSection.ShowData(isAllowEdit);

		public void ShowPaymentData(bool? isAllowEdit = null) => _paymentSection.ShowData(isAllowEdit);


		public void ResetFromStation() => _fromStationSection.Reset();

		public void ResetToStation() => _toStationSection.Reset();

		public void ResetDepartDate() => _departDateSection.Reset();

		public void ResetDepartOperator() => _departOperatorSection.Reset();

		public void ResetDepartSeat() => _departSeatSection.Reset();

		public void ResetReturnDate() => _returnDateSection.Reset();

		public void ResetReturnOperator() => _returnOperatorSection.Reset();

		public void ResetReturnSeat() => _returnSeatSection.Reset();

		public void ResetPassenger() => _passengerSection.Reset();

		public void ResetPayment() => _paymentSection.Reset();

		public void HideMiniNavigator()
		{
			GrdMiniNavigator.Visibility = Visibility.Collapsed;
		}

		public void ShowMiniNavigator()
		{
			ChangeScreenSize();
			GrdMiniNavigator.Visibility = Visibility.Visible;
		}

		public void InitTimeFilterPage(ITimeFilter timeFilterPage)
		{
			_timeFilterPage = timeFilterPage;
			frmTripTimeFilter.NavigationService.Navigate((Page)_timeFilterPage);
		}

		private bool _isTimeFilterVisibled = false;
		public bool IsTimeFilterVisibled 
		{ 
			get => _isTimeFilterVisibled; 
			set
			{
				_isTimeFilterVisibled = value;

				if (_isTimeFilterVisibled)
					frmTripTimeFilter.Visibility = Visibility.Visible;
				else
					frmTripTimeFilter.Visibility = Visibility.Collapsed;
			}
		}

		private int _changeScreenSizeCount = 0;
		private void ChangeScreenSize()
        {
			if (App.MainScreenControl.QueryWindowSize
				(out double winWidth, out double winHeight) == true)
            {
				if (_changeScreenSizeCount <= 3)
                {
					if (winHeight > 1500)
                    {
						BdPrevious.Height = 60;
						BdExit.Height = 60;
						BdStacker1.Height = 800;
					}
					_changeScreenSizeCount++;
				}
            }
        }
	}
}