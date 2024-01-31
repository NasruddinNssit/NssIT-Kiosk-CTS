using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace NssIT.Kiosk.Client.ViewPage.PickupNDrop
{
    /// <summary>
    /// Interaction logic for pgPickupNDrop.xaml
    /// </summary>
    public partial class pgPickupNDrop : Page, IPickupNDrop, IKioskViewPage
	{
		private const string LogChannel = "ViewPage";

		private LocationViewHelper _pickupViewHelper = null;
        private LocationViewHelper _dropViewHelper = null;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _language = LanguageCode.English;

		public pgPickupNDrop()
        {
            InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\PickupNDrop\rosPickupNDropMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\PickupNDrop\rosPickupNDropEnglish.xaml");

			_pickupViewHelper = new LocationViewHelper(this.LstPickup);
			_pickupViewHelper.OnStationSelected += _pickupViewHelper_OnStationSelected;

			_dropViewHelper = new LocationViewHelper(this.LstDrop);
			_dropViewHelper.OnStationSelected += _dropViewHelper_OnStationSelected;
		}

		private void _pickupViewHelper_OnStationSelected(object sender, LocationSelectedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
				App.ShowDebugMsg($@"pickup Station : {e.Station.StationDesc} selected with id : {e.Station.StationId}");
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgStation._pickupViewHelper_OnStationSelected");
			}
			
		}

		private void _dropViewHelper_OnStationSelected(object sender, LocationSelectedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
				App.ShowDebugMsg($@"drop Station : {e.Station.StationDesc} selected with id : {e.Station.StationId}");
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgStation._dropViewHelper_OnStationSelected");
			}
		}

		private void LstPickup_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstPickup_ScrollChanged");
			}
		}

		private void LstDrop_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstDrop_ScrollChanged");
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				GrdScreenShield.Visibility = Visibility.Collapsed;

				this.Resources.MergedDictionaries.Clear();

				if (_language == LanguageCode.Malay)
					this.Resources.MergedDictionaries.Add(_langMal);
				else
					this.Resources.MergedDictionaries.Add(_langEng);

				_hasConfirm = false;
				_pickupViewHelper.ReloadList(_pickupList);
				_dropViewHelper.ReloadList(_dropList);

				TxtReqPickUpLoc.Visibility = Visibility.Collapsed;
				TxtReqDropOffLoc.Visibility = Visibility.Collapsed;

				ChangeScreenSize();
				//Debug_UpdatePickupList();
				//Debug_UpdateDropList();
				System.Windows.Forms.Application.DoEvents();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgPickupNDrop.Page_Loaded");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgPickupNDrop.Page_Loaded");
			}
		}

		private bool _hasConfirm = false;
		private void BdComfirm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_hasConfirm)
					return;

				LocationViewRow pickRow = _pickupViewHelper.SelectedLocation;
				LocationViewRow dropRow = _dropViewHelper.SelectedLocation;

				TxtReqPickUpLoc.Visibility = Visibility.Collapsed;
				TxtReqDropOffLoc.Visibility = Visibility.Collapsed;

				if (pickRow == null)
					TxtReqPickUpLoc.Visibility = Visibility.Visible;

				else if (dropRow == null)
					TxtReqDropOffLoc.Visibility = Visibility.Visible;

				else
				{
					App.MainScreenControl.ExecMenu.ShieldMenu();

					App.ShowDebugMsg($@"pickup Station : {pickRow.StationDesc} selected with id : {pickRow.StationId}");
					App.ShowDebugMsg($@"drop off Station : {dropRow.StationDesc} selected with id : {dropRow.StationId}");

					_hasConfirm = true;
					Submit(pickRow.StationId, pickRow.StationDesc, pickRow.TimeStr, dropRow.StationId, dropRow.StationDesc);

					//App.NetClientSvc.SalesService.SubmitPickupNDrop(pickRow.StationId, pickRow.StationDesc, pickRow.TimeStr, dropRow.StationId, dropRow.StationDesc, out bool isServerResponded);

					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000701)");

				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgPickupNDrop.BdComfirm_MouseLeftButtonDown");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgPickupNDrop.BdComfirm_MouseLeftButtonDown");
			}
		}

		private void Submit(string departPick, string departPickDesn, string departPickTime, string departDrop, string departDropDesn)
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitPickupNDrop(departPick, departPickDesn, departPickTime, departDrop, departDropDesn, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000701)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000703)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000703)", ex), "EX01", "pgPickupNDrop.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		public void ShieldPage()
		{
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		LocationViewList _pickupList = null;
		LocationViewList _dropList = null;
		public void InitPickupDropData(UIDepartPickupNDropAck uiDepartPickupNDrop)
		{
			_pickupList = new LocationViewList();
			_dropList = new LocationViewList();

			if (uiDepartPickupNDrop.MessageData != null)
			{
				PickupNDropList pndList = (PickupNDropList)uiDepartPickupNDrop.MessageData;

				if (uiDepartPickupNDrop.Session != null)
					_language = uiDepartPickupNDrop.Session.Language;
				else
					_language = LanguageCode.English;

				if (pndList.PickDetailList?.Length > 0)
				{
					foreach (PickDetail row in pndList.PickDetailList)
					{
						_pickupList.Add(new LocationViewRow() { StationId = row.Pick, StationDesc = row.PickDesn, TimeStr = row.PickTime });
					}
				}

				if (pndList.DropDetailList?.Length > 0)
				{
					foreach (DropDetail row in pndList.DropDetailList)
					{
						_dropList.Add(new LocationViewRow() { StationId = row.Drop, StationDesc = row.DropDesn });
					}
				}
			}
		}

		private void Debug_UpdatePickupList()
		{
			LocationViewList pickupList = new LocationViewList();

			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Melaka Sentral xxxxx ddddd fffff hhhhh kkkkk" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "TBS" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Lar Kin" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Ipoh", Selected = true });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Rawang" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Sungai Buluh" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Batu Pahat" });
			pickupList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Klang" });

			_pickupViewHelper.ReloadList(pickupList);

			System.Windows.Forms.Application.DoEvents();
		}

		private void Debug_UpdateDropList()
		{
			LocationViewList dropList = new LocationViewList();

			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Melaka Sentral xxxxx ddddd fffff hhhhh kkkkk" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "TBS" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Lar Kin" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Ipoh" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Rawang" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Sungai Buluh" });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Batu Pahat", Selected = true });
			dropList.Add(new LocationViewRow() { StationId = Guid.NewGuid().ToString("D"), StationDesc = "Klang" });

			_dropViewHelper.ReloadList(dropList);

			System.Windows.Forms.Application.DoEvents();
		}

		private void BtnTestUpdatePickupList_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Debug_UpdatePickupList();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgPickupNDrop.BtnTestUpdatePickupList_Click");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgPickupNDrop.BtnTestUpdatePickupList_Click");
			}
		}

		private void BtnTestUpdateDropList_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Debug_UpdateDropList();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgPickupNDrop.BtnTestUpdateDropOffList_Click");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgPickupNDrop.BtnTestUpdateDropOffList_Click");
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
						BdComfirm.Height = 60;
					}
					_changeScreenSizeCount++;
				}
			}
		}
	}
}