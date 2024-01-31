using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany
{
	/// <summary>
	/// ClassCode:EXIT80.17
	/// </summary>
	public class BusCompanyViewSearch : IDisposable
	{
		private const string LogChannel = "ViewPage";

		private Dispatcher _pageDispatcher = null;
		private ConcurrentQueue<SearchDataPack> _searchDataPackList = new ConcurrentQueue<SearchDataPack>();

		private BusCompanyViewRow[] _fullbusCompanyList = new BusCompanyViewRow[0];
		private BusCompanyViewList _busCompanyViewList = new BusCompanyViewList();
		private ListView _lstBusCompanyViewer = null;

		private bool _disposed = false;
		private bool _pageLoaded = false;
		private Thread _searchingThreadWorker = null;

		public BusCompanyViewSearch(Dispatcher pageDispatcher, BusCompanyViewList busCompanyViewList, ListView lstBusCompanyViewer)
        {
			_pageDispatcher = pageDispatcher;
			_busCompanyViewList = busCompanyViewList ?? new BusCompanyViewList();
			_lstBusCompanyViewer = lstBusCompanyViewer;

			_searchingThreadWorker = new Thread(UpdateBusCompanyListThreadWorking);
			_searchingThreadWorker.IsBackground = true;
			_searchingThreadWorker.Start();
		}

		/// <summary>
		/// FuncCode:EXIT80.1790
		/// </summary>
		public void Dispose()
		{
			_disposed = true;
			lock (_searchDataPackList)
			{
				while (_searchDataPackList.TryDequeue(out _))
				{ }
			}
		}


		public void PageLoaded(BusCompanyViewRow[] fullbusCompanyList)
		{
			_fullbusCompanyList = fullbusCompanyList ?? new BusCompanyViewRow[0];
			_pageLoaded = true;
		}


		public void PageUnLoaded()
		{
			_pageLoaded = false;
		}

		/// <summary>
		/// FuncCode:EXIT80.1705
		/// </summary>
		public void FilterListByTime(string startTime)
		{
			if (_pageLoaded == false)
				return;

			lock (_searchDataPackList)
			{
				_searchDataPackList.Enqueue(new SearchDataPack()
				{
					CompanyNameFilter = string.IsNullOrWhiteSpace(startTime) ? null : startTime
				}) ;

				Monitor.PulseAll(_searchDataPackList);
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1720
		/// </summary>
		private SearchDataPack GetLatestSearchDataPack()
		{
			SearchDataPack retSearchDataPack = null;

			if (_disposed == false)
			{
				try
				{
					if ((_pageLoaded == false) || (App.IsClientReady == false))
					{
						lock (_searchDataPackList)
                        {
							Monitor.Wait(_searchDataPackList, 30 * 1000);
						}
					}
					else
                    {
						lock (_searchDataPackList)
						{
							if (_searchDataPackList.Count == 0)
							{
								Monitor.Wait(_searchDataPackList, 5 * 1000);
							}
							while (_searchDataPackList.TryDequeue(out SearchDataPack outSearchDataPack))
								retSearchDataPack = outSearchDataPack;
						}
					}
				}
				catch (Exception ex)
				{
					App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1720.EX01); TripDetailViewListHelper.GetLatestSelectedData");
					App.Log.LogError(LogChannel, "-", new Exception("(EXIT80.1720.EX01)", ex), "EX01", classNMethodName: "BusCompanyViewSearch.GetLatestSearchDataPack");
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1720.EX01)");
				}
			}
			return retSearchDataPack;
		}

		/// <summary>
		/// FuncCode:EXIT80.1725
		/// </summary>
		private void UpdateBusCompanyListThreadWorking()
		{
			SearchDataPack latestSearchDataPack = null;

			while (_disposed == false)
			{
				try
				{
					latestSearchDataPack = null;
					latestSearchDataPack = GetLatestSearchDataPack();

					if (latestSearchDataPack != null)
					{
						if (_pageLoaded)
						{
							_pageDispatcher.Invoke(new Action(() => {

								DispatcherProcessingDisabled ddp;

								try
								{
									_busCompanyViewList.Clear();
									_lstBusCompanyViewer.SelectedIndex = -1;
									_lstBusCompanyViewer.SelectedItem = null;
									System.Windows.Forms.Application.DoEvents();
									Thread.Sleep(350);

									ddp = _pageDispatcher.DisableProcessing();

									SearchBusCompany(latestSearchDataPack.CompanyNameFilter);
								}
								catch (Exception ex)
								{
									App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1725.EX01); TripDetailViewListHelper.UpdateTripListThreadWorking");
									App.Log.LogError(LogChannel, "-", new Exception("(EXIT80.1725.EX01)", ex), "EX01", classNMethodName: "BusCompanyViewSearch.UpdateBusCompanyListThreadWorking");
								}
								finally
								{
									if (ddp != null)
										ddp.Dispose();

									System.Windows.Forms.Application.DoEvents();
									Task.Delay(200).Wait();
								}
							}));
						}
					}
					else
					{
						lock (_searchDataPackList)
						{
							Monitor.Wait(_searchDataPackList, 100);
						}
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			/// <summary>
			/// FuncCode:EXIT80.1725A
			/// </summary>
			void SearchBusCompany(string searchCompanyName)
			{
				BusCompanyViewRow[] tmpBusCompanyList = new BusCompanyViewRow[0];

				if (_fullbusCompanyList?.Length > 0)
				{
					if (string.IsNullOrWhiteSpace(searchCompanyName) == false)
					{
						string scr = searchCompanyName.ToUpper().Trim();

						tmpBusCompanyList = (from row in _fullbusCompanyList
												where (row.CompanyNameCAP?.ToString().Contains(scr) == true)
													&& (string.IsNullOrWhiteSpace(row.CompanyCode) == false)
												orderby row.CompanyNameCAP
												select row).ToArray();
					}
					else
						tmpBusCompanyList = (from row in _fullbusCompanyList
												where string.IsNullOrWhiteSpace(row.CompanyCode) == false
												orderby row.CompanyNameCAP
												select row).ToArray();
				}

				tmpBusCompanyList = (from row in tmpBusCompanyList
										orderby row.CompanyNameCAP
										select row).ToArray();

				for (int inx = 0; inx < tmpBusCompanyList.Length; inx++)
				{
					_busCompanyViewList.Add(tmpBusCompanyList[inx]);
				}

				if (tmpBusCompanyList?.Length > 0)
					_lstBusCompanyViewer.ScrollIntoView(tmpBusCompanyList[0]);
			}
		}

		class SearchDataPack
		{
			/// <summary>
			/// Company Name
			/// </summary>
			public string CompanyNameFilter { get; set; }
		}
	}
}
