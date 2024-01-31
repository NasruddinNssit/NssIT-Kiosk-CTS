using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
	public class StateViewListHelper
	{
		private string _logChannel = "ViewPage";

		private Brush _selectedStateBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0x2B, 0x9C, 0xDB));
		private Brush _deselectedStateBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

		private Brush _selectedStateForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _deselectedStateForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));

		public event EventHandler<StateChangedEventArgs> OnStateChanged;

		private DbLog Log { get; set; }
		private Page _pgStationSelection = null;
		private ListView _lstStateViewer = null;
		private StateViewList _stateViewList = null;

		private Border _previousSelectedStateControl = null;
		private Border _selectedStateControl = null;

		private StateViewRow _previousSelectedStateRow = null;
		private StateViewRow _selectedStateRow = null;

		private LanguageCode _language = LanguageCode.English;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		public string AllStateCode { get => "~*ALL*~"; }

		public StateViewListHelper(Page pgStationSelection, ListView lstStateViewer)
		{
			Log = DbLog.GetDbLog();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationEng.xaml");

			_pgStationSelection = pgStationSelection;
			_lstStateViewer = lstStateViewer;

			_stateViewList = new StateViewList();
			_lstStateViewer.DataContext = _stateViewList;
		}

		public void StateMouseDownHandle(object sender, MouseButtonEventArgs e)
		{
			try
			{
				StateViewRow selState = (StateViewRow)((Border)sender).DataContext;

				if (
					(selState.State.Equals(_selectedStateRow?.State) == false)
					|| (_selectedStateRow is null)
					)
				{
					if (_selectedStateControl != null)
					{
						_previousSelectedStateRow = _selectedStateRow;
						_previousSelectedStateControl = _selectedStateControl;
						_previousSelectedStateControl.Background = _deselectedStateBackground;
						((TextBlock)_previousSelectedStateControl.Child).Foreground = _deselectedStateForeground;
					}

					_selectedStateControl = (Border)sender;
					_selectedStateControl.Background = _selectedStateBackground;
					((TextBlock)_selectedStateControl.Child).Foreground = _selectedStateForeground;
					_selectedStateRow = (StateViewRow)_selectedStateControl.DataContext;

					RaiseOnStateChanged(_selectedStateRow.State);
				}
				//else if ((selState.State.Equals(_selectedStateRow?.State)))
				//{
				//	_selectedStateControl.Background = _deselectedStateBackground;
				//	((TextBlock)_selectedStateControl.Child).Foreground = _deselectedStateForeground;

				//	_previousSelectedStateRow = null;
				//	_selectedStateRow = null;

				//	_previousSelectedStateControl = null;
				//	_selectedStateControl = null;

				//	RaiseOnStateChanged(null);
				//}
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "StateViewListHelper:StateMouseDownHandle");
				App.ShowDebugMsg(ex.ToString());
			}
		}

		public void InitView(LanguageCode language)
		{
			_language = language;
			_selectedStateRow = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="state">A NULL value indicate reset of state selection</param>
		private void RaiseOnStateChanged(string state)
		{
			try
			{
				OnStateChanged?.Invoke(null, new StateChangedEventArgs(state));
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "", ex, "EX01", "StateViewListHelper.RaiseOnStateChanged");
			}
		}

		public void CreateStateList(destination_statedetail[] destStateList)
		{
			string sPACE = " ";

			_stateViewList.Clear();

			if (destStateList != null)
			{
				if (_language == LanguageCode.Malay)
					_stateViewList.Add(new StateViewRow() { State = AllStateCode, StateDesc = $@"{sPACE}{_langMal["ALL_STATE_Label"]}{sPACE}"});
				else
					_stateViewList.Add(new StateViewRow() { State = AllStateCode, StateDesc = $@"{sPACE}{_langEng["ALL_STATE_Label"]}{sPACE}"});

				destination_statedetail[] sortedStateArr = (from stt in destStateList
															orderby stt.desc
															select stt).ToArray();

				foreach (var state in sortedStateArr)
				{
					_stateViewList.Add(new StateViewRow() { State = state.state, StateDesc = state.desc });
				}
			}
		}

		public void DebugTest_CreateStateList()
		{
			_stateViewList.Clear();

			//_allStateCode
			_stateViewList.Add(new StateViewRow() { State = AllStateCode, StateDesc = " All State " });
			_stateViewList.Add(new StateViewRow() { State = "PERLIS", StateDesc = "Perlis" });
			_stateViewList.Add(new StateViewRow() { State = "KEDAH", StateDesc = "Kedah" });
			_stateViewList.Add(new StateViewRow() { State = "KELANTAN", StateDesc = "Kelantan" });
			_stateViewList.Add(new StateViewRow() { State = "PULAU PINANG", StateDesc = "Pulau Pinang" });
			_stateViewList.Add(new StateViewRow() { State = "PERAK", StateDesc = "Perak" });
			_stateViewList.Add(new StateViewRow() { State = "PAHANG", StateDesc = "Pahang" });
			_stateViewList.Add(new StateViewRow() { State = "TERENGGANU", StateDesc = "Terengganu" });
			_stateViewList.Add(new StateViewRow() { State = "SELANGOR", StateDesc = "Selangor" });
			_stateViewList.Add(new StateViewRow() { State = "KUALA LUMPUR", StateDesc = "Kuala Lumpur" });
			_stateViewList.Add(new StateViewRow() { State = "W.P. PUTRA JAYA", StateDesc = "Putrajaya" });
			_stateViewList.Add(new StateViewRow() { State = "NEGERI SEMBILAN", StateDesc = "Negeri Sembilan" });
			_stateViewList.Add(new StateViewRow() { State = "MELAKA", StateDesc = "Melaka" });
			_stateViewList.Add(new StateViewRow() { State = "JOHOR", StateDesc = "Johor" });

			//_stateViewList.Add(new StateViewRow() { State = "Labuan", StateDesc = "Labuan" });
			//_stateViewList.Add(new StateViewRow() { State = "Sabah", StateDesc = "Sabah" });
			//_stateViewList.Add(new StateViewRow() { State = "Sarawak", StateDesc = "Sarawak" });
		}

		public destination_statedetail[] Debug_SampleStates()
		{
			List<destination_statedetail> list = new List<destination_statedetail>();

			list.Add(new destination_statedetail() { state = "PERLIS", desc = "Perlis" });
			list.Add(new destination_statedetail() { state = "KEDAH", desc = "Kedah" });
			list.Add(new destination_statedetail() { state = "KELANTAN", desc = "Kelantan" });
			list.Add(new destination_statedetail() { state = "PULAU PINANG", desc = "Pulau Pinang" });
			list.Add(new destination_statedetail() { state = "PERAK", desc = "Perak" });
			list.Add(new destination_statedetail() { state = "PAHANG", desc = "Pahang" });
			list.Add(new destination_statedetail() { state = "TERENGGANU", desc = "Terengganu" });
			list.Add(new destination_statedetail() { state = "SELANGOR", desc = "Selangor" });
			list.Add(new destination_statedetail() { state = "KUALA LUMPUR", desc = "Kuala Lumpur" });
			list.Add(new destination_statedetail() { state = "W.P. PUTRA JAYA", desc = "Putrajaya" });
			list.Add(new destination_statedetail() { state = "NEGERI SEMBILAN", desc = "Negeri Sembilan" });
			list.Add(new destination_statedetail() { state = "MELAKA", desc = "Melaka" });
			list.Add(new destination_statedetail() { state = "JOHOR", desc = "Johor" });

			return list.ToArray();
		}
	}
}
