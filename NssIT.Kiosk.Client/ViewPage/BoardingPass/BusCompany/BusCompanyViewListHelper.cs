using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany
{
    public class BusCompanyViewListHelper
    {
		private const string LogChannel = "ViewPage";

		public event EventHandler OnBeginQueryNewTripData;
		public event EventHandler OnUpdateTripViewInProgress;

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 0, 100);

		private Dispatcher _pageDispatcher = null;
		private ListView _lstBusCompanyViewer = null;
		private BusCompanyViewList _busCompanyViewList = null;
		private BusCompanyViewRow[] _fullbusCompanyList = new BusCompanyViewRow[0];

		private bool _pageLoaded = false;

		private DbLog Log { get; set; }
		private boardingcompany_status _rawBusCompanyData = null;
		private CultureInfo _provider = CultureInfo.InvariantCulture;

		public BusCompanyViewList BusCompanyViewList => _busCompanyViewList;

		public BusCompanyViewListHelper(Dispatcher pageDispatcher, ListView lstBusCompanyViewer)
        {
			Log = DbLog.GetDbLog();
			_busCompanyViewList = new BusCompanyViewList();
			_pageDispatcher = pageDispatcher;
			lstBusCompanyViewer.DataContext = _busCompanyViewList;
			_lstBusCompanyViewer = lstBusCompanyViewer;
		}

		public BusCompanyViewRow[] PageLoaded()
		{
			_pageLoaded = true;

			App.CTCompanyLogoCache.ClearCacheOnTimeout();

			_lstBusCompanyViewer.SelectedIndex = -1;
			_lstBusCompanyViewer.SelectedItem = null;

			LoadViewList();

			return _fullbusCompanyList;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void LoadViewList()
            {
				_busCompanyViewList.Clear();
				string baseURL = _rawBusCompanyData.companyLogoURL;

				BusCompanyViewRow[] rowList = (from row in _rawBusCompanyData.details
											   where string.IsNullOrWhiteSpace(row.companycode) == false
											   select new BusCompanyViewRow
											   {
												   CompanyCode = row.companycode,
												   CompanyImgId = row.companyimgid,
												   CompanyName = row.companydesn, 
												   CompanyLogoURL = $@"{baseURL}?ID={row.companyimgid}", 
												   CompanyNameCAP = (row.companydesn ?? "").Trim().ToUpper()
											   }).ToArray();
				rowList = (from row in rowList
						   orderby row.CompanyNameCAP
						   select row).ToArray();

				_fullbusCompanyList = rowList;

				for (int inx = 0; inx < _fullbusCompanyList.Length; inx++)
                {
					UpdateCompanyLogo(_fullbusCompanyList[inx]);
					_busCompanyViewList.Add(_fullbusCompanyList[inx]);
				}

				_lstBusCompanyViewer.SelectedIndex = -1;
				_lstBusCompanyViewer.SelectedItem = null;
				_lstBusCompanyViewer.ScrollIntoView(_busCompanyViewList[0]);
			}
		}

		public void PageUnLoaded()
		{
			_pageLoaded = false;
			_lstBusCompanyViewer.SelectedIndex = -1;
			_lstBusCompanyViewer.SelectedItem = null;
			_fullbusCompanyList = new BusCompanyViewRow[0];
		}

		public void InitData(boardingcompany_status busCompanyData)
        {
			_rawBusCompanyData = busCompanyData;
		}

		public BusCompanyViewRow GetItemRow(string companyCode)
		{
			var tpArr = (from row in _busCompanyViewList.Items
						 where row.CompanyCode.Equals(companyCode)
						 select row).ToArray();

			BusCompanyViewRow retRow = null;
			if (tpArr.Length > 0)
			{
				retRow = tpArr[0];
			}

			return retRow;
		}

		private async Task UpdateCompanyLogo(BusCompanyViewRow row)
        {
			if (row != null)
            {
				row.BitmapImage = await App.CTCompanyLogoCache.GetImage(row.CompanyLogoURL);
			}
        }

		
	}
}
