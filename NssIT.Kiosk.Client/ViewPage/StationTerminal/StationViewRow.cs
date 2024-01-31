using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
	public class StationViewRow : ViewModelBase
	{
		private static System.Windows.Media.Brush _whiteBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

		public string _state = null;
		public string State
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state != value)
				{
					_state = value;
					this.OnPropertyChanged("State");
				}
			}
		}

		public string _station = null;
		public string Station
		{
			get
			{
				return _station;
			}
			set
			{
				if (_station != value)
				{
					_station = value;
					this.OnPropertyChanged("Station");
				}
			}
		}

		public string _stationDesc = null;
		public string StationDesc
		{
			get
			{
				return _stationDesc;
			}
			set
			{
				if (_stationDesc != value)
				{
					_stationDesc = value;
					this.OnPropertyChanged("StationDesc");
				}

			}
		}

		public string _stateDesc = null;
		public string StateDesc
		{
			get
			{
				return _stateDesc;
			}
			set
			{
				if (_stateDesc != value)
				{
					_stateDesc = value;
					this.OnPropertyChanged("StateDesc");
				}

			}
		}

		private bool _isIndicateState = false;
		public bool IsIndicateState
		{
			get
			{
				return _isIndicateState;
			}
			set
			{
				if (_isIndicateState != value)
				{
					_isIndicateState = value;
					this.OnPropertyChanged("IsIndicateState");
				}
			}
		}

		public Visibility StationVisibility
		{
			get
			{
				if (IsIndicateState)
					return Visibility.Collapsed;
				else
					return Visibility.Visible;
			}
		}

		public Visibility StateVisibility
		{
			get
			{
				if (IsIndicateState)
					return Visibility.Visible;
				else
					return Visibility.Collapsed;
			}
		}

		private System.Windows.Media.Brush _rowBackGroundColor = _whiteBackground;
		public System.Windows.Media.Brush RowBackGroundColor
		{
			get
			{
				return _rowBackGroundColor;
			}
			set
			{
				if (_rowBackGroundColor != value)
				{
					_rowBackGroundColor = value;
					this.OnPropertyChanged("RowBackGroundColor");
				}
			}
		}

	}
}
