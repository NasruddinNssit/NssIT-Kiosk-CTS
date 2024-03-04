using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
	/// <summary>
	/// Interaction logic for uscIntroEnglish.xaml
	/// </summary>
	public partial class uscIntroEnglish : UserControl, IIntroAdjustPage
	{
		private const string LogChannel = "ViewPage";

		public event EventHandler<BeginEventArgs> OnBegin;

		private Style _buttonEnabledStyle = null;
		private Style _buttonDisabledStyle = null;

		public bool _buttonEnabled = true;

        private PayECRAccess _payWaveAccs = null;

        private PayECRAccess PayWaveAccs
        {
            get
            {
                return _payWaveAccs ?? (_payWaveAccs = NewPayECRAccess());

                PayECRAccess NewPayECRAccess()
                {
                    PayECRAccess payWvAccs = PayECRAccess.GetPayECRAccess("COM3", PayECRAccess.SaleMaxWaitingSec); /*, @"C:\eTicketing_Log\ECR_Receipts\", @"C:\eTicketing_Log\ECR_LOG", true, true);*/
                  
                    return payWvAccs;
                }
            }
        }
        public uscIntroEnglish()
		{
			InitializeComponent();

			_buttonEnabledStyle = this.FindResource("GreenButton") as Style;
			_buttonDisabledStyle = this.FindResource("DisabledButton") as Style;

			TxtSysVer.Text = App.SystemVersion ?? "*";

			
		}

		private void BtnBegin_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!_buttonEnabled)
					return;

				RaiseOnBeginTransaction(TransactionType.BuyTicket);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", "uscIntroEnglish.BtnBegin_Click");
			}
		}

		private void BtnCollectTicket_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!_buttonEnabled)
					return;

				RaiseOnBeginTransaction(TransactionType.CollectTicket);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, "EX01", "uscIntroEnglish.BtnCollectTicket_Click");
			}
		}

		private void BtnDoSettlement(object sender, RoutedEventArgs e)
		{
			if(App.HostNumberForSettlementsTesting.Count > 0)
			{
				foreach(var hostNo in App.HostNumberForSettlementsTesting)
				{
					PayWaveAccs.SettlePayment(Guid.NewGuid().ToString(), hostNo.Trim());
				}
			}
		}


        private void RaiseOnBeginTransaction(TransactionType transactionType)
		{
			if (OnBegin != null)
			{
				OnBegin.Invoke(this, new BeginEventArgs(transactionType));
			}
		}

        public void SetOperationState(bool state, DateTime? startTime, DateTime? endTime)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (state)
                {

                    if (App.IsBoardingPassEnabled)
                        BtnCollectTicket.Visibility = Visibility.Hidden;
                    else
                        BtnCollectTicket.Visibility = Visibility.Visible;

                    BtnStart.Visibility = Visibility.Visible;

                    BtnOffOperation.Visibility = Visibility.Hidden;
                }
                else
                {
                    string meridiem = (startTime.Value.Hour < 12) ? "AM" : "PM";

                    string endMer = (endTime.Value.Hour < 12) ? "AM" : "PM";
                    // Format time as HH:MM AM/PM
                    string startTimeOpr = $"{(startTime.Value.Hour % 12 == 0 ? 12 : startTime.Value.Hour % 12):00}:{startTime.Value.Minute:00} {meridiem}";

                    string endTimeOpr = $"{(endTime.Value.Hour % 12 == 0 ? 12 : endTime.Value.Hour % 12):00}:{endTime.Value.Minute:00} {endMer}";

                    TimeStart.Text = startTimeOpr;
                    TimeEnd.Text = endTimeOpr;

                    BtnStart.Visibility = Visibility.Hidden;

                    BtnCollectTicket.Visibility = Visibility.Hidden;

                    BtnOffOperation.Visibility = Visibility.Visible;
                }

            }));
        }

        public void SetStartButtonEnabled(bool enabled)
		{
			this.Dispatcher.Invoke(new Action(() => {
				_buttonEnabled = enabled;

				if (App.IsBoardingPassEnabled)
					BtnCollectTicket.Visibility = Visibility.Visible;
				else
					BtnCollectTicket.Visibility = Visibility.Hidden;

				if (_buttonEnabled)
				{
					BtnStart.Style = _buttonEnabledStyle;
					BtnCollectTicket.Style = _buttonEnabledStyle;
					TxtBuyTicket.Visibility = Visibility.Visible;
					TxtBuyTicketDisabled.Visibility = Visibility.Collapsed;
					//BtnCollectTicket.Visibility = Visibility.Visible;
					TxtCollectBoardingPass.Visibility = Visibility.Visible;
					TxtCollectBoardingPassDisabled.Visibility = Visibility.Collapsed;
				}
				else
				{
					BtnStart.Style = _buttonDisabledStyle;
					BtnCollectTicket.Style = _buttonDisabledStyle;
					TxtBuyTicket.Visibility = Visibility.Collapsed;
					TxtBuyTicketDisabled.Visibility = Visibility.Visible;
					//BtnCollectTicket.Visibility = Visibility.Hidden;
					TxtCollectBoardingPass.Visibility = Visibility.Collapsed;
					TxtCollectBoardingPassDisabled.Visibility = Visibility.Visible;
				}
			}));
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			//ImgTicketGirl.Source = new BitmapImage(new Uri("pack://application:,,,/AssemblyName;component/Resources/logo.png"));
			try
			{
				if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Larkin)
				{
					ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/larkin-logo-reverse.png", UriKind.RelativeOrAbsolute));
					ImgTicketGirl.Source = new BitmapImage(new Uri("/Resources/TicketGirl-larkin-EN.png", UriKind.RelativeOrAbsolute));
				}
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Gombak)
				{
					ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/TerminalBersepaduGombak-logo.png", UriKind.RelativeOrAbsolute));
					ImgTicketGirl.Source = new BitmapImage(new Uri("/Resources/TicketGirl-TBG-EN.png", UriKind.RelativeOrAbsolute));
				}
				else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Klang)
				{
					ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/Klang Sentral Terminal 00.jpeg", UriKind.RelativeOrAbsolute));
					ImgTicketGirl.Source = new BitmapImage(new Uri("/Resources/Klang Sentral Girl_BI.png", UriKind.RelativeOrAbsolute));
				}
				else if(App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Genting)
				{
					ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/genting.png", UriKind.RelativeOrAbsolute));
                    ImgTicketGirl.Source = new BitmapImage(new Uri("/Resources/Klang Sentral Girl_BI.png", UriKind.RelativeOrAbsolute));
					ImgStationLogo.Height = 60;
                }
                else
				{
					ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/MelakaSentral-logo.png", UriKind.RelativeOrAbsolute));
					ImgTicketGirl.Source = new BitmapImage(new Uri("/Resources/TicketGirl - Melaka - EN.png", UriKind.RelativeOrAbsolute));
				}

				TxtSysVer.Text = App.GetFullSystemVersion();
				e.Handled = true;
			}
			catch (Exception ex)
			{
				string db = ex.Message;
			}
			return;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		}

		public void UpdateVersion()
		{
			try
			{
				this.Dispatcher.Invoke(new Action(() => {
					TxtSysVer.Text = App.GetFullSystemVersion();
				}));
				ShowVersionNFeatures();
			}
			catch { }
		}

		private RunThreadMan _showVersionNFeaturesThreadMan = null;
		private void ShowVersionNFeatures()
		{
			int waitPeriodSec = 120;

			_showVersionNFeaturesThreadMan?.AbortRequest(out _, 500);

			_showVersionNFeaturesThreadMan = new RunThreadMan(new Action(() =>
			{
				try
				{
					this.Dispatcher.Invoke(new Action(() =>
					{
						ImgCash.Visibility = Visibility.Collapsed;
						ImgEWallet.Visibility = Visibility.Collapsed;
						ImgCreditCard.Visibility = Visibility.Collapsed;
					}));

					DateTime expiredTime = DateTime.Now.AddSeconds(waitPeriodSec - 10);

					while (
						((App.AvailablePaymentTypeList is null) || (App.AvailablePaymentTypeList.Length == 0)) &&
						(expiredTime.Subtract(DateTime.Now).TotalSeconds >= 0)
						)
					{
						Thread.Sleep(500);
					}

					if (App.AvailablePaymentTypeList?.Length > 0)
					{
						foreach (PaymentType pT in App.AvailablePaymentTypeList)
						{
							if (pT == PaymentType.Cash)
							{
								this.Dispatcher.Invoke(new Action(() =>
								{
									ImgCash.Visibility = Visibility.Visible;
								}));
							}
							else if (pT == PaymentType.CreditCard)
							{
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    ImgCreditCard.Visibility = Visibility.Visible;
                                }));
                            }
							else if (pT == PaymentType.PaymentGateway)
							{
								this.Dispatcher.Invoke(new Action(() =>
								{
									ImgEWallet.Visibility = Visibility.Visible;
								}));
							}
						}
						this.Dispatcher.Invoke(new Action(() => {
							TxtSysVer.Text = App.GetFullSystemVersion();
						}));
					}
				}
				catch { }
			}), "uscIntroEnglish.UserControl_Loaded", waitPeriodSec, LogChannel);
		}

		public void AdjustSize(double pageWidth)
		{
			if (pageWidth < 1500)
			{
				Thickness oldMar = TxtUserMsg.Margin;
				TxtUserMsg.Margin = new Thickness(50, 130, oldMar.Right, oldMar.Bottom);
			}
		}
	}
}
