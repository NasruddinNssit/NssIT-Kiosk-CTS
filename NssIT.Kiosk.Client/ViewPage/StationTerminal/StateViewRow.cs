using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
	public class StateViewRow : ViewModelBase
	{
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
	}
}
