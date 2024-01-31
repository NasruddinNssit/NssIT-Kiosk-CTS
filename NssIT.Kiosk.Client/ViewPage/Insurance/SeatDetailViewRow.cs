using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    public class SeatDetailViewRow : ViewModelBase
    {
        public string _seatDesn;
        public string SeatDesn
        {
            get
            {
                return _seatDesn;
            }
            set
            {
                if (_seatDesn != value)
                {
                    _seatDesn = value;
                    this.OnPropertyChanged("SeatDesn");
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
            private set
            {
                if (_priceStr != value)
                {
                    _priceStr = value;
                    this.OnPropertyChanged("PriceStr");
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

                    PriceStr = $@"{_price:#,###.00}";
                }                
            }
        }
    }
}