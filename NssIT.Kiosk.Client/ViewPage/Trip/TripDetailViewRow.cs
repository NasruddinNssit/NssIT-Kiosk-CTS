using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public class TripDetailViewRow : ViewModelBase
    {
        public string _timeStr;
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

        public string _orgTimeStr;
        public string OrgTimeStr
        {
            get
            {
                return _orgTimeStr;
            }
            set
            {
                if (_orgTimeStr != value)
                {
                    _orgTimeStr = value;
                    this.OnPropertyChanged("OrgTimeStr");
                }
            }
        }


        public string _currency;
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                if (_currency != value)
                {
                    _currency = value;
                    this.OnPropertyChanged("Currency");
                }
            }
        }

        public string _priceStr;
        public string PriceStr
        {
            get
            {
                return _priceStr;
            }
            set
            {
                if (_priceStr != value)
                {
                    _priceStr = value;
                    this.OnPropertyChanged("PriceStr");
                }
            }
        }

        public int _availableSeat;
        public int AvailableSeat
        {
            get
            {
                return _availableSeat;
            }
            set
            {
                if (_availableSeat != value)
                {
                    _availableSeat = value;
                    this.OnPropertyChanged("AvailableSeat");
                }
            }
        }

        public int _totalSeat;
        public int TotalSeat
        {
            get
            {
                return _totalSeat;
            }
            set
            {
                if (_totalSeat != value)
                {
                    _totalSeat = value;
                    this.OnPropertyChanged("TotalSeat");
                }
            }
        }

        public string _totalSeatStr;
        public string TotalSeatStr
        {
            get
            {
                return _totalSeatStr;
            }
            set
            {
                if (_totalSeatStr != value)
                {
                    _totalSeatStr = value;
                    this.OnPropertyChanged("TotalSeatStr");
                }
            }
        }

        public string _operatorImgPath;
        public string OperatorImgPath
        {
            get
            {
                return _operatorImgPath;
            }
            set
            {
                if (_operatorImgPath != value)
                {
                    _operatorImgPath = value;
                    this.OnPropertyChanged("OperatorImgPath");
                }
            }
        }

        public string _operatorDesc;
        public string OperatorDesc
        {
            get
            {
                return _operatorDesc;
            }
            set
            {
                if (_operatorDesc != value)
                {
                    _operatorDesc = value;
                    this.OnPropertyChanged("OperatorDesc");
                }
            }
        }

        private decimal _price;
        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    this.OnPropertyChanged("Price");
                }
            }
        }

        private string _tripId;
        public string TripId
        {
            get
            {
                return _tripId;
            }
            set
            {
                if (_tripId != value)
                {
                    _tripId = value;
                    this.OnPropertyChanged("TripId");
                }
            }
        }

        private string _tripDate;
        public string TripDate
        {
            get
            {
                return _tripDate;
            }
            set
            {
                if (_tripDate != value)
                {
                    _tripDate = value;
                    this.OnPropertyChanged("TripDate");
                }
            }
        }

        private int _tripIntTime;
        public int TripIntTime
        {
            get
            {
                return _tripIntTime;
            }
            set
            {
                if (_tripIntTime != value)
                {
                    _tripIntTime = value;
                    this.OnPropertyChanged("TripIntTime");
                }
            }
        }

        private string _tripNo;
        public string TripNo
        {
            get
            {
                return _tripNo;
            }
            set
            {
                if (_tripNo != value)
                {
                    _tripNo = value;
                    this.OnPropertyChanged("TripNo");
                }
            }
        }

        private int _seatOccupied;
        public int SeatOccupied
        {
            get
            {
                return _seatOccupied;
            }
            set
            {
                if (_seatOccupied != value)
                {
                    _seatOccupied = value;
                    this.OnPropertyChanged("SeatOccupied");
                }
            }
        }

        private short _timePosi;
        public short TimePosi
        {
            get
            {
                return _timePosi;
            }
            set
            {
                if (_timePosi != value)
                {
                    _timePosi = value;
                    this.OnPropertyChanged("TimePosi");
                }
            }
        }

        private string _embed;
        /// <summary>
        /// Used a switch for PickUp & Drop = 0: Disabled; 1: Enabled
        /// </summary>
        public string Embed
        {
            get
            {
                return _embed;
            }
            set
            {
                if (_embed != value)
                {
                    _embed = value;
                    this.OnPropertyChanged("Embed");
                }
            }
        }

        private string _passengerActualFromStationCode;
        public string PassengerActualFromStationCode
        {
            get
            {
                return _passengerActualFromStationCode;
            }
            set
            {
                if (_passengerActualFromStationCode != value)
                {
                    _passengerActualFromStationCode = value;
                    this.OnPropertyChanged("PassengerActualFromStationCode");
                }
            }
        }

        private string _passengerActualToStationCode;
        public string PassengerActualToStationCode
        {
            get
            {
                return _passengerActualToStationCode;
            }
            set
            {
                if (_passengerActualToStationCode != value)
                {
                    _passengerActualToStationCode = value;
                    this.OnPropertyChanged("PassengerActualToStationCode");
                }
            }
        }

        private decimal _insurance;
        public decimal Insurance
        {
            get
            {
                return _insurance;
            }
            set
            {
                if (_insurance != value)
                {
                    _insurance = value;
                    this.OnPropertyChanged("Insurance");
                }
            }
        }

        private decimal _skywayamount;
        public decimal Skywayamount
        {
            get
            {
                return _skywayamount;
            }
            set
            {
                if(_skywayamount != value)
                {
                    _skywayamount = value;
                    this.OnPropertyChanged("Skywayamount");
                }
            }
        }

        public string _tripClass;
        public string TripClass
        {
            get
            {
                return _tripClass;
            }
            set
            {
                if (_tripClass != value)
                {
                    _tripClass = value;
                    this.OnPropertyChanged("TripClass");
                }
            }
        }

        public string _tripDetail;
        public string TripDetail
        {
            get
            {
                return _tripDetail;
            }
            set
            {
                if (_tripDetail != value)
                {
                    _tripDetail = value;
                    this.OnPropertyChanged("TripDetail");
                }
            }
        }

        public string _tripIdCode;
        public string TripIdCode
        {
            get
            {
                return _tripIdCode;
            }
            set
            {
                if (_tripIdCode != value)
                {
                    _tripIdCode = value;
                    this.OnPropertyChanged("TripIdCode");
                }
            }
        }

        public Visibility _normalSeatAvailableVisible;
        public Visibility NormalSeatAvailableVisible
        {
            get
            {
                return _normalSeatAvailableVisible;
            }
            set
            {
                if (_normalSeatAvailableVisible != value)
                {
                    _normalSeatAvailableVisible = value;
                    this.OnPropertyChanged("NormalSeatAvailableVisible");
                }
            }
        }


        public Visibility _quickFinishSeatAvailableVisible;
        public Visibility QuickFinishSeatAvailableVisible
        {
            get
            {
                return _quickFinishSeatAvailableVisible;
            }
            set
            {
                if (_quickFinishSeatAvailableVisible != value)
                {
                    _quickFinishSeatAvailableVisible = value;
                    this.OnPropertyChanged("QuickFinishSeatAvailableVisible");
                }
            }
        }

        public Visibility _soldOutVisible;
        public Visibility SoldOutVisible
        {
            get
            {
                return _soldOutVisible;
            }
            set
            {
                if (_soldOutVisible != value)
                {
                    _soldOutVisible = value;
                    this.OnPropertyChanged("SoldOutVisible");
                }
            }
        }

        public Visibility _isPickSeatVisible;
        public Visibility IsPickSeatVisible
        {
            get
            {
                return _isPickSeatVisible;
            }
            set
            {
                if (_isPickSeatVisible != value)
                {
                    _isPickSeatVisible = value;
                    this.OnPropertyChanged("IsPickSeatVisible");
                }
            }
        }

    }

    //public class ViewModelBase : INotifyPropertyChanged
    //{
    //    #region INotifyPropertyChanged
    //    protected void OnPropertyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    #endregion INotifyPropertyChanged
    //}
}