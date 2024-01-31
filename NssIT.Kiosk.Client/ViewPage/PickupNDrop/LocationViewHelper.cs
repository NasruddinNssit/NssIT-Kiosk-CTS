using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.PickupNDrop
{
	public class LocationViewHelper
	{
		private const string LogChannel = "ViewPage";

		private LocationViewList _locationList = new LocationViewList();
		private ListView _dataListViewer = null;

		public event EventHandler<LocationSelectedEventArgs> OnStationSelected;
		public LocationViewHelper(ListView pickupListViewer)
		{
			_dataListViewer = pickupListViewer;
			pickupListViewer.DataContext = _locationList;
			_dataListViewer.SelectionChanged += _dataListViewer_SelectionChanged;
		}

		public LocationViewRow SelectedLocation
		{
			get
			{
				LocationViewRow[] locList = (from station in _locationList.Collection
											where station.Selected == true
											select station).ToArray();

				return ((locList.Length == 0) ? null : locList[0]);
			}
		}

		private void _dataListViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_dataListViewer.Dispatcher.Invoke(new Action(() => {
				LocationViewRow stationViewRow = (LocationViewRow)_dataListViewer.SelectedItem;

				if (stationViewRow == null)
					return;

				SelectStation(stationViewRow);
				if (OnStationSelected != null)
				{
					OnStationSelected.Invoke(sender, new LocationSelectedEventArgs(stationViewRow));
				}
			}));

		}

		private void SelectStation(LocationViewRow stationViewRow)
		{
			try
			{
				foreach (LocationViewRow row in _locationList)
				{
					if (row.StationId == stationViewRow.StationId)
						row.Selected = true;
					else
						row.Selected = false;
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", "LocationViewHelper.SelectStation");
			}
		}

		public void ReloadList(LocationViewList viewList)
		{
			_locationList.Clear();

			foreach (LocationViewRow row in viewList)
			{
				_locationList.Add(new LocationViewRow() { StationId = row.StationId, StationDesc = row.StationDesc, Selected = row.Selected });
			}
		}
	}
}