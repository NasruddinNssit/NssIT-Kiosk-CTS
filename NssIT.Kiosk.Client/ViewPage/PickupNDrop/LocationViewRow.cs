using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.PickupNDrop
{
	public class LocationViewRow : ViewModelBase
	{
		private static Brush _selectedFillColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

		private string _stationId;
		public string StationId
		{
			get
			{
				return _stationId;
			}
			set
			{
				if (_stationId != value)
				{
					_stationId = value;
					this.OnPropertyChanged("StationId");
				}
			}
		}

		private string _stationDesc;
		public string StationDesc
		{
			get
			{
				return _stationDesc;
			}
			set
			{
				if (_stationId != (value))
				{
					_stationDesc = value;
					this.OnPropertyChanged("StationDesc");
				}
			}
		}

		private string _timeStr;
		public string TimeStr
		{
			get
			{
				return _timeStr;
			}
			set
			{
				if (_timeStr != value)
				{
					_timeStr = value;
					this.OnPropertyChanged("TimeStr");
				}
			}
		}

		private bool _selected = false;
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected != (value))
				{
					_selected = value;

					if (_selected == true)
					{
						SelectionColor = _selectedFillColor;
						SelectedVisibility = Visibility.Visible;
					}
					else
					{
						SelectionColor = null;
						SelectedVisibility = Visibility.Hidden;
					}

					this.OnPropertyChanged("Selected");
				}
			}
		}

		private Brush _selectionColor = null;
		public Brush SelectionColor
		{
			get
			{
				return _selectionColor;
			}
			set
			{
				if (_selectionColor != (value))
				{
					_selectionColor = value;
					this.OnPropertyChanged("SelectionColor");
				}
			}
		}

		private Visibility _selectedVisibility = Visibility.Hidden;
		public Visibility SelectedVisibility
		{
			get
			{
				return _selectedVisibility;
			}
			set
			{
				if (_selectedVisibility != (value))
				{
					_selectedVisibility = value;
					this.OnPropertyChanged("SelectedVisibility");
				}
			}
		}
	}
}
