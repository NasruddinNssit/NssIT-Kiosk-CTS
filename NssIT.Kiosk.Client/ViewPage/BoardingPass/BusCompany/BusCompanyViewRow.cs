using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany
{
    public class BusCompanyViewRow : ViewModelBase, IDisposable 
    {
        private string _companyName;
        public string CompanyName
        {
            get
            {
                return _companyName;
            }
            set
            {
                if (_companyName != value)
                {
                    _companyName = value;
                    this.OnPropertyChanged("CompanyName");
                }
            }
        }

        private string _companyNameCAP;
        public string CompanyNameCAP
        {
            get
            {
                return _companyNameCAP;
            }
            set
            {
                if (_companyNameCAP != value)
                {
                    _companyNameCAP = value;
                    this.OnPropertyChanged("CompanyNameCAP");
                }
            }
        }

        private string _companyCode;
        public string CompanyCode
        {
            get
            {
                return _companyCode;
            }
            set
            {
                if (_companyCode != value)
                {
                    _companyCode = value;
                    this.OnPropertyChanged("CompanyCode");
                }
            }
        }

        private string _companyImgId;
        public string CompanyImgId
        {
            get
            {
                return _companyImgId;
            }
            set
            {
                if (_companyImgId != value)
                {
                    _companyImgId = value;
                    this.OnPropertyChanged("CompanyImgId");
                }
            }
        }

        private string _companyLogoURL;
        public string CompanyLogoURL
        {
            get
            {
                return _companyLogoURL;
            }
            set
            {
                if (_companyLogoURL != value)
                {
                    _companyLogoURL = value;
                    this.OnPropertyChanged("CompanyLogoURL");
                }
            }
        }

        private BitmapImage _bitmapImage;
        public BitmapImage BitmapImage
        {
            get
            {
                return _bitmapImage;
            }
            set
            {
                if (_bitmapImage != value)
                {
                    _bitmapImage = value;
                    this.OnPropertyChanged("BitmapImage");
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
