using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	public class SeatCalibrator
	{
		private Brush _availableSeatColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xE9, 0xE9, 0xE9));
		private Brush _selectedSeatColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x2B, 0x9C, 0xDB));
		private Brush _occupiedSeatColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

		private Brush _seatForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));
		private Brush _selectedSeatForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

		private double _walkingPathWidth = 30D;
		private double _walkingPathHeight = 30D;

		// Bus space-splitter
		private string _bdBusFrontBumperXaml = @"<Border Height=""5"" Tag=""FrontBumper"" Margin=""5,8,5,10"" CornerRadius=""5"" Background=""#FFE9E9E9"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />";
		private string _bdBusEndXaml = @"<Border Height=""10"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />";

		// Bus seats
		private string _driverSeatXaml = @"<Border Width=""40"" Height=""40"" CornerRadius=""10"" Background=""#FFE9E9E9"" Margin=""2"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
			<Border>
				<Border.Background>
					<ImageBrush ImageSource=""/NssIT.Kiosk.Client;component/Resources/icon-driver.png"" />
				</Border.Background >
			</Border>
			</Border> ";
		private string _availableSeatXaml = @"<Border Width=""50"" Height=""50"" CornerRadius=""5"" Background=""#FFE9E9E9"" Margin=""2"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
			<TextBlock Foreground = ""#FF444444"" Text=""A3"" TextAlignment=""Center"" FontSize=""14"" VerticalAlignment=""Center""  />
			</Border>
			";

		// Bus Row
		private string _bdRowOfSeatXaml = @"<StackPanel Orientation=""Horizontal"" FlowDirection=""RightToLeft"" Margin=""5,0"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />";

		// -- -- -- -- --
		private Dispatcher _uiDispatcher = null;
		private Border _bdLowerDeck = null;
		private Border _bdUpperDeck = null;
		private TextBlock _txtMsg = null;

		private SeatDeckCollection _seatLowCollection = null;
		private SeatDeckCollection _seatUpCollection = null;

		public decimal DefaultSeatPrice { get; set; } = 1.0M;
		public decimal DefaultInsurancePrice { get; set; } = 1.0M;

		private int _maxSeatPerTrip = 5;
		private List<string> _selectActualIdList = new List<string>();

		private pgSeat _pgSeat = null;

		private LanguageCode _language = LanguageCode.English;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		public SeatCalibrator(pgSeat pgseat, Dispatcher dispatcher, Border bdLowerDeck, Border bdUpperDeck, TextBlock txtMsg)
		{
			if (dispatcher is null)
				throw new Exception("Invalid dispatcher parameter in Seat Calibrator.");

			if (bdLowerDeck is null)
				throw new Exception("Invalid LowerDeck parameter in Seat Calibrator.");

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatEnglish.xaml");

			_seatLowCollection = new SeatDeckCollection();
			_seatUpCollection = new SeatDeckCollection();

			_uiDispatcher = dispatcher;
			_bdLowerDeck = bdLowerDeck;
			_bdUpperDeck = bdUpperDeck;
			_txtMsg = txtMsg;

			_pgSeat = pgseat;
		}

		public void InitCalibration(SeatDeckCollection[] seatDeckArray, int maximumSeatPerTrip, decimal defaultSeatPrice, LanguageCode language)
		{
			MaxSeatPerTrip = maximumSeatPerTrip;
			DefaultSeatPrice = defaultSeatPrice;
			_language = language;

			if (seatDeckArray != null)
			{
				if (seatDeckArray.Length >= 1)
					_seatLowCollection = seatDeckArray[0];
				else
					_seatLowCollection = new SeatDeckCollection();

				if (seatDeckArray.Length >= 2)
					_seatUpCollection = seatDeckArray[1];
				else
					_seatUpCollection = new SeatDeckCollection();
			}
			else
			{
				_seatLowCollection = new SeatDeckCollection();
				_seatUpCollection = new SeatDeckCollection();
			}
		}

		public int MaxSeatPerTrip
		{
			get
			{
				return _maxSeatPerTrip;
			}
			set
			{
				_maxSeatPerTrip = (value <= 0) ? 1 : value;
			}
		}

		public List<SeatInfo> SelectedSeats
		{
			get
			{
				List<SeatInfo> retList = new List<SeatInfo>();

				SeatInfo[] list1 = (from seatKeyPair in _seatLowCollection.SeatList
									where seatKeyPair.Value.StateOfSeat == SeatState.Selected
									select seatKeyPair.Value).ToArray();

				retList.AddRange(list1);

				SeatInfo[] list2 = (from seatKeyPair in _seatUpCollection.SeatList
									where seatKeyPair.Value.StateOfSeat == SeatState.Selected
									select seatKeyPair.Value).ToArray();

				retList.AddRange(list2);

				return retList;
			}
		}

		//public SeatCollection LowDeckSeats
		//{
		//	get => _seatLowCollection;
		//}

		//public SeatCollection UpDeckSeats
		//{
		//	get => _seatUpCollection;
		//}

		public void SeatTriggerSelection(Border bdSeat)
		{
			_uiDispatcher.Invoke(new Action(() =>
			{
				// Varification
				if (bdSeat is null)
					return;

				SeatDeckCollection seatCollection = null;
				SeatInfo seatInfo = null;
				string id = null;

				if (SeatInfo.IsSeatIdValid(bdSeat.Tag.ToString()))
					id = bdSeat.Tag.ToString();
				else
					return;

				if (_seatLowCollection.SeatList.TryGetValue(id, out seatInfo))
				{
					seatCollection = _seatLowCollection;
				}
				else
				{
					seatInfo = null;
					if (_seatUpCollection.SeatList.TryGetValue(id, out seatInfo))
					{
						seatCollection = _seatUpCollection;
					}
				}

				if (seatInfo is null)
					return;

				// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
				// Calculated Existing Total Seat Selected
				int seatCount1 = (from seatKeyPair in _seatLowCollection.SeatList
								  where seatKeyPair.Value.StateOfSeat == SeatState.Selected
								  select seatKeyPair).Count();

				int seatCount2 = (from seatKeyPair in _seatUpCollection.SeatList
								  where seatKeyPair.Value.StateOfSeat == SeatState.Selected
								  select seatKeyPair).Count();

				int totalSeatCountSelected = seatCount1 + seatCount2;
				// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
				// Seat Selection Triggering 
				if ((seatInfo.StateOfSeat == SeatState.Available) && (totalSeatCountSelected < MaxSeatPerTrip))
				{
					_txtMsg.Text = "";
					seatCollection.SelectSeat(id, out bool isValidSelection);
					if (isValidSelection)
					{
						bdSeat.Background = _selectedSeatColor;
						((TextBlock)bdSeat.Child).Foreground = _selectedSeatForeground;
					}
				}
				else if (seatInfo.StateOfSeat == SeatState.Selected)
				{
					_txtMsg.Text = "";

					SeatState seatState = seatCollection.DeselectSeat(id, out bool isSeatFound);

					if ((seatState == SeatState.Available) && (isSeatFound))
					{
						bdSeat.Background = _availableSeatColor;
						((TextBlock)bdSeat.Child).Foreground = _seatForeground;
					}
				}
				else if (totalSeatCountSelected >= MaxSeatPerTrip)
				{
					//MaxSeatPerTrip
					if (_language == LanguageCode.Malay)
						_txtMsg.Text = string.Format(_langMal["CUST_ERROR_MAX_SEAT_Label"]?.ToString(), MaxSeatPerTrip.ToString().Trim());
					else
						_txtMsg.Text = string.Format(_langEng["CUST_ERROR_MAX_SEAT_Label"]?.ToString(), MaxSeatPerTrip.ToString().Trim());

					if (string.IsNullOrWhiteSpace(_txtMsg.Text))
					{
						_txtMsg.Text = string.Format("Only maximum {0} seats is allowed.", MaxSeatPerTrip.ToString().Trim());
					}
				}
				// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
			}));
		}

		public void RefreshAllContainerSeat(out int lowerDeckColumnCount, out int upperDeckColumnCount)
		{
			lowerDeckColumnCount = 0;
			upperDeckColumnCount = 0;

			int lowDeckColCount = 0;
			int upDeckColCount = 0;

			_uiDispatcher.Invoke(new Action(() => {
				DispatcherProcessingDisabled dispProcDisabler;
				try
				{
					dispProcDisabler = _uiDispatcher.DisableProcessing();

					if ((_bdLowerDeck != null) && (_bdLowerDeck.Child is StackPanel stkInnerLowDeck))
					{
						stkInnerLowDeck.Children.Clear();
						RefreshContainer(_seatLowCollection, _bdLowerDeck, includeDriverSeat: true, maxColumnCount: out lowDeckColCount);

					}

					if ((_seatUpCollection.IsValidCollection) && (_bdUpperDeck != null) && (_bdUpperDeck.Child is StackPanel stkInnerUpDeck))
					{
						stkInnerUpDeck.Children.Clear();
						RefreshContainer(_seatUpCollection, _bdUpperDeck, includeDriverSeat: false, maxColumnCount: out upDeckColCount);
					}
					else if ((_bdUpperDeck != null) && (_bdUpperDeck.Child is StackPanel stkInnerUpDeckII))
					{
						stkInnerUpDeckII.Children.Clear();
					}
				}
				finally
				{
					if (dispProcDisabler != null)
						dispProcDisabler.Dispose();
				}
			}));

			lowerDeckColumnCount = lowDeckColCount;
			upperDeckColumnCount = upDeckColCount;

			void RefreshContainer(SeatDeckCollection seatCollection, Border aDeck, bool includeDriverSeat, out int maxColumnCount)
			{
				maxColumnCount = 0;
				seatCollection.ReadContainerDimension(out int grp1ColCount, out int grp2ColCount, out int maxRowCount);

				MemoryStream frontBumperStream = null;
				StreamReader frontBumperReader = null;

				MemoryStream driverStream = null;
				StreamReader driverReader = null;

				MemoryStream busEndStream = null;
				StreamReader busEndReader = null;

				MemoryStream rowSeatStream = null;
				StreamReader rowSeatReader = null;

				MemoryStream seatStream = null;
				StreamReader seatReader = null;

				// rowStrRd  rowStm  seatStrRd  seatStm

				try
				{
					int walkingPathInx = grp1ColCount - 1;

					SeatInfo[] seatSortedList = (from seatKeyPair in seatCollection.SeatList
												 orderby seatKeyPair.Value.RowInx, seatKeyPair.Value.ColumnInx
												 select seatKeyPair.Value).ToArray();

					frontBumperStream = new MemoryStream(Encoding.UTF8.GetBytes(_bdBusFrontBumperXaml));
					frontBumperReader = new StreamReader(frontBumperStream);

					if (includeDriverSeat)
					{
						driverStream = new MemoryStream(Encoding.UTF8.GetBytes(_driverSeatXaml));
						driverReader = new StreamReader(driverStream);
					}

					busEndStream = new MemoryStream(Encoding.UTF8.GetBytes(_bdBusEndXaml));
					busEndReader = new StreamReader(busEndStream);

					rowSeatStream = new MemoryStream(Encoding.UTF8.GetBytes(_bdRowOfSeatXaml));
					rowSeatReader = new StreamReader(rowSeatStream);

					seatStream = new MemoryStream(Encoding.UTF8.GetBytes(_availableSeatXaml));
					seatReader = new StreamReader(seatStream);

					int currRowInx = -1;

					UIElementCollection stkDeckChildren = ((StackPanel)aDeck.Child).Children;
					StackPanel currSeatRow = null;

					Border bdFrontBumper = null;
					Border bdDriver = null;
					Border bdBusEnd = null;

					Border currBdSeat = null;
					Border currBdWalkingPath = null;

					foreach (SeatInfo seat in seatSortedList)
					{
						// Add New Row of Seat
						if ((currSeatRow is null) || (currRowInx != seat.RowInx))
						{
							if ((currSeatRow is null) && (currRowInx == -1))
							{
								// Add Front Bumper
								bdFrontBumper = (Border)CreateNewControl(frontBumperStream, frontBumperReader);
								stkDeckChildren.Add(bdFrontBumper);

								// Add Driver Seat
								if (includeDriverSeat)
								{
									// Add New Row for Driver Seat
									currSeatRow = (StackPanel)CreateNewControl(rowSeatStream, rowSeatReader);
									stkDeckChildren.Add(currSeatRow);

									// Add Seat
									bdDriver = (Border)CreateNewControl(driverStream, driverReader);
									currSeatRow.Children.Add(bdDriver);
								}
							}

							// Add Row of Seat
							currSeatRow = (StackPanel)CreateNewControl(rowSeatStream, rowSeatReader);
							currRowInx = seat.RowInx;

							stkDeckChildren.Add(currSeatRow);
						}
						//-- -- -- -- -- -- -- -- -- -- -- 
						// Add New Seat
						currBdSeat = (Border)CreateNewControl(seatStream, seatReader);
						maxColumnCount = ((seat.ColumnInx + 1) > maxColumnCount) ? (seat.ColumnInx + 1) : maxColumnCount;

						// .. currBdSeat.Tag will help to identify a seat id when user selecting a border control.
						switch (seat.StateOfSeat)
						{
							case SeatState.Available:
								((TextBlock)currBdSeat.Child).Text = seat.SeatDesc;
								((TextBlock)currBdSeat.Child).Tag = seat.Id;
								currBdSeat.Tag = seat.Id;
								break;

							case SeatState.Occupied:
								currBdSeat.Background = _occupiedSeatColor;
								((TextBlock)currBdSeat.Child).Text = seat.SeatDesc;
								((TextBlock)currBdSeat.Child).Tag = "Occupied";
								currBdSeat.Tag = "Occupied";
								break;

							case SeatState.Selected:
								currBdSeat.Background = _selectedSeatColor;
								((TextBlock)currBdSeat.Child).Foreground = _selectedSeatForeground;
								((TextBlock)currBdSeat.Child).Text = seat.SeatDesc;
								((TextBlock)currBdSeat.Child).Tag = seat.Id;
								currBdSeat.Tag = seat.Id;
								break;

							default:
								currBdSeat.Visibility = Visibility.Hidden;
								((TextBlock)currBdSeat.Child).Tag = "Blank";
								currBdSeat.Tag = "Blank";
								break;
						}
						currSeatRow.Children.Add(currBdSeat);

						/////CYA - DEBUG-- Remark the follwing code before deploy
						if (App.SysParam.PrmIsDebugMode)
						{
							currBdSeat.MouseLeftButtonDown += CurrBdSeat_MouseLeftButtonDown;
							currBdSeat.MouseLeftButtonUp += CurrBdSeat_MouseLeftButtonUp;
						}
						//------------------------------------------------------------------

						//-- -- -- -- -- -- -- -- -- -- -- 
						// Add Walking Path
						if (seat.ColumnInx == walkingPathInx)
						{
							currBdWalkingPath = (Border)CreateNewControl(seatStream, seatReader);

							currBdWalkingPath.Width = _walkingPathWidth;
							currBdWalkingPath.Height = _walkingPathHeight;
							currBdWalkingPath.Visibility = Visibility.Hidden;
							currBdWalkingPath.Tag = "Blank";

							currSeatRow.Children.Add(currBdWalkingPath);
						}
						//-- -- -- -- -- -- -- -- -- -- -- 
					}

					// Add End Bus Border
					bdBusEnd = (Border)CreateNewControl(busEndStream, busEndReader);
					stkDeckChildren.Add(bdBusEnd);
				}
				finally
				{
					if (frontBumperReader != null)
						frontBumperReader.Dispose();

					if (frontBumperStream != null)
						frontBumperStream.Dispose();

					if (includeDriverSeat)
					{
						if (driverReader != null)
							driverReader.Dispose();

						if (driverStream != null)
							driverStream.Dispose();
					}

					if (busEndReader != null)
						busEndReader.Dispose();

					if (busEndStream != null)
						busEndStream.Dispose();

					if (rowSeatReader != null)
						rowSeatReader.Dispose();

					if (rowSeatStream != null)
						rowSeatStream.Dispose();

					if (seatReader != null)
						seatReader.Dispose();

					if (seatStream != null)
						seatStream.Dispose();
				}
			}

			object CreateNewControl(MemoryStream memoryStream, StreamReader streamReader)
			{
				memoryStream.Seek(0, SeekOrigin.Begin);
				XmlReader xmlReaderRow = XmlReader.Create(streamReader);
				return XamlReader.Load(xmlReaderRow);
			}
		}

		private string _lastTagId = null;
		private void CurrBdSeat_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (sender is Border bd)
			{
				if ((SeatInfo.IsSeatIdValid(bd.Tag.ToString())) && (bd.Tag.ToString().Equals(_lastTagId)))
					_pgSeat.TriggerSeatBorder(bd);
			}

			_lastTagId = null;
		}

		private void CurrBdSeat_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is Border db)
			{
				if (SeatInfo.IsSeatIdValid(db.Tag.ToString()))
					_lastTagId = db.Tag.ToString();
			}
		}

		/// <summary>
		/// Generate seats for a decker (Lower Decker or Upper Decker)
		/// </summary>
		/// <param name="grp1ColCount"></param>
		/// <param name="grp2ColCount"></param>
		/// <param name="maxRowCount"></param>
		/// <param name="seatPrice">Include terminalcharge + adultprice + insurance + onlineqrcharge </param>
		/// <param name="seatDetailList"></param>
		/// <returns></returns>
		public SeatDeckCollection GenerateSeatDeckCollection(int grp1ColCount, int grp2ColCount, int maxRowCount, decimal seatPrice, seat_detail[] seatDetailList)
		{
			if (seatDetailList.Length != ((grp1ColCount + grp2ColCount) * maxRowCount))
				throw new Exception("Unable to generate seat data; (EXIT10000521); Dimension is out of range;");

			SeatDeckCollection seatColl = new SeatDeckCollection(); ;

			seat_detail[] seqSeatDet = (from seat in seatDetailList
				orderby seat.seq
				select seat).ToArray();

			seatColl.Init(grp1ColCount, grp2ColCount, maxRowCount);

			int inxDetail = -1;
			seat_detail st = null;
			SeatState seatState = SeatState.Blank;
			for (int colInx1 = 0; colInx1 < grp1ColCount; colInx1++)
			{
				for (int rowInx = 0; rowInx < maxRowCount; rowInx++)
				{
					inxDetail++;
					st = seqSeatDet[inxDetail];

					if (st.avai.ToUpper().Trim().Equals("Y"))
						seatState = SeatState.Available;
					else
						seatState = SeatState.Occupied;

					seatColl.AddSeat(colInx1, rowInx, st.seatid.ToString().Trim(), st.desn ?? "-", st.type, seatState, seatPrice);
				}
			}

			for (int colInx2 = grp1ColCount; colInx2 < (grp1ColCount + grp2ColCount); colInx2++)
			{
				for (int rowInx = 0; rowInx < maxRowCount; rowInx++)
				{
					inxDetail++;
					st = seqSeatDet[inxDetail];

					if (st.avai.ToUpper().Trim().Equals("Y"))
						seatState = SeatState.Available;
					else
						seatState = SeatState.Occupied;

					seatColl.AddSeat(colInx2, rowInx, st.seatid.ToString().Trim(), st.desn ?? "-", st.type, seatState, seatPrice);
				}
			}

			return seatColl;
		}

		public void DebugTest_XX_GetSeatsData(out int maxSeatPerTrip, out SeatDeckCollection[] seatDeckArr, out decimal defaultSeatPrice)
		{
			maxSeatPerTrip = 1;
			seatDeckArr = new SeatDeckCollection[0];
			defaultSeatPrice = 1.00M;

			Random ranDomX = new Random();

			int deckCheckRanDomNo = ranDomX.Next(1, 20);
			int deckCount = ((deckCheckRanDomNo % 2) == 0) ? 1 : 2;

			seatDeckArr = new SeatDeckCollection[deckCount];
			defaultSeatPrice = Math.Round(((decimal)ranDomX.Next(10, 200) / 8M), 2);

			for (int inx = 0; inx < deckCount; inx++)
			{
				seatDeckArr[inx] = AddLowDeckSeatsData(ranDomX.Next(1, 5), ranDomX.Next(1, 5), ranDomX.Next(5, 25), defaultSeatPrice);
			}

			// Fix MaxSeatPerTrip
			maxSeatPerTrip = (ranDomX.Next(1, 50) % 6);
			//-------------------------------------------------

			SeatDeckCollection AddLowDeckSeatsData(int grp1ColCount, int grp2ColCount, int maxRowCount, decimal seatPrice)
			{
				SeatDeckCollection retSeatCollection = new SeatDeckCollection();
				retSeatCollection.Init(grp1ColCount, grp2ColCount, maxRowCount);

				Random ranDom = new Random();
				int ranNo = 0;

				for (int rowInx = 0; rowInx < maxRowCount; rowInx++)
				{
					for (int colInx1 = 0; colInx1 < grp1ColCount; colInx1++)
					{
						SeatState seatState = SeatState.Blank;
						ranNo = ranDom.Next(1, 24);

						if ((ranNo % 6) == 0)
							//seatState = SeatState.Selected;
							seatState = SeatState.Available;

						else if ((ranNo % 5) == 0)
							seatState = SeatState.Blank;

						else if ((ranNo % 3) == 0)
							seatState = SeatState.Occupied;

						else
							seatState = SeatState.Available;


						retSeatCollection.AddSeat(colInx1, rowInx, Guid.NewGuid().ToString("D"), ranDom.Next(25, 99).ToString(), "Adult", seatState, seatPrice);
					}

					for (int colInx2 = grp1ColCount; colInx2 < (grp1ColCount + grp2ColCount); colInx2++)
					{
						SeatState seatState = SeatState.Blank;
						ranNo = ranDom.Next(1, 24);

						if ((ranNo % 6) == 0)
							//seatState = SeatState.Selected;
							seatState = SeatState.Available;

						else if ((ranNo % 5) == 0)
							seatState = SeatState.Blank;

						else if ((ranNo % 3) == 0)
							seatState = SeatState.Occupied;

						else
							seatState = SeatState.Available;

						retSeatCollection.AddSeat(colInx2, rowInx, Guid.NewGuid().ToString("D"), ranDom.Next(25, 99).ToString(), "Adult", seatState, seatPrice);
					}
				}
				return retSeatCollection;
			}
		}
	}
}
