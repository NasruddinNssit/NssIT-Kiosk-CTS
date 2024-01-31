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

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    /// <summary>
    /// Interaction logic for pgTimeFilter.xaml
    /// </summary>
    public partial class pgTimeFilter : Page, ITimeFilter
    {
        public event EventHandler<TimeFilterEventArgs> OnTimeFilterChangedTrigger;
        public event EventHandler<TimeFilterEventArgs> OnTimeFilterResetTrigger;

        private string _logChannel = "ViewPage";

        private Brush _timeChangedBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xC1, 0x00));
        private Brush _timeNormalBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xC1, 0x00));

        private (bool TimeChanged, DateTime LastChangedTime, DateTime StartFilterTime, DateTime EndFilterTime) _timeFilterChanged
            = (TimeChanged: false, LastChangedTime: DateTime.MinValue, 
                StartFilterTime: new DateTime(1900, 1, 1, 0, 0, 0, 0), EndFilterTime: new DateTime(1900, 1, 1, 23, 59, 59, 999));

        private double _minChangeTriggerSec = 1D;

        private TimeFilterThumbMoveHelper _thumb1 = null;
        private TimeFilterThumbMoveHelper _thumb2 = null;

        private Thread _timeFilterTriggerThreadWorker = null;

        private SemaphoreSlim _filterLock = new SemaphoreSlim(1);

        public pgTimeFilter()
        {
            InitializeComponent();

            _thumb1 = new TimeFilterThumbMoveHelper(ImgThumb1, CvSliderContainer);
            _thumb2 = new TimeFilterThumbMoveHelper(ImgThumb2, CvSliderContainer);

            _thumb1.OnTimeFilterChanged += _thumb_OnTimeFilterChanged;
            _thumb2.OnTimeFilterChanged += _thumb_OnTimeFilterChanged;

            _thumb1.OnTimeFilterReset += _thumb1_OnTimeFilterReset;
            _thumb2.OnTimeFilterReset += _thumb1_OnTimeFilterReset;

            // Note : Thread is disable temporary because of using "Apply Filter"
            //_timeFilterTriggerThreadWorker = new Thread(new ThreadStart(TimeFilterTriggerThreadWorking));
            //_timeFilterTriggerThreadWorker.IsBackground = true;
            //_timeFilterTriggerThreadWorker.Start();
        }

        public bool IsFilterActived { get; set; } = false;

        private void _thumb1_OnTimeFilterReset(object sender, TimeFilterEventArgs e)
        {
            bool locked = false;
            try
            {
                App.ShowDebugMsg($@"pgTimeFilter._thumb1_OnTimeFilterReset; Start Time : {e.StartTime.ToString("hh:mm tt")}; End Time : {e.EndTime.ToString("hh:mm tt")}");

                if (_filterLock.WaitAsync().Wait(1000 * 3))
                {
                    locked = true;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtFromTime.Foreground = _timeNormalBackground;
                        TxtToTime.Foreground = _timeNormalBackground;

                        _timeFilterChanged.StartFilterTime = e.StartTime;
                        _timeFilterChanged.EndFilterTime = e.EndTime;

                        TxtFromTime.Text = e.StartTime.ToString("hh:mm tt");
                        TxtToTime.Text = e.EndTime.ToString("hh:mm tt");
                    }));
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error pgTimeFilter._thumb1_OnTimeFilterReset; {ex.Message}");
                App.Log.LogError(_logChannel, "-", ex, null, "pgTimeFilter._thumb1_OnTimeFilterReset");
            }
            finally
            {
                //if (locked)
                _filterLock.Release();
            }
        }

        private void _thumb_OnTimeFilterChanged(object sender, TimeFilterEventArgs e)
        {
            bool locked = false;
            try
            {
                //App.ShowDebugMsg($@"pgTimeFilter._thumb_OnTimeFilterChanged; Start Time : {e.StartTime.ToString("hh:mm tt")}; End Time : {e.EndTime.ToString("hh:mm tt")}");

                App.TimeoutManager.ResetTimeout();

                if (_filterLock.WaitAsync().Wait(1000 * 3))
                {
                    locked = true;
                    _timeFilterChanged.TimeChanged = true;
                    _timeFilterChanged.LastChangedTime = DateTime.Now;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (TxtFromTime.Foreground.Equals(_timeChangedBackground) == false)
                        {
                            TxtFromTime.Foreground = _timeChangedBackground;
                            TxtToTime.Foreground = _timeChangedBackground;
                        }

                        _timeFilterChanged.StartFilterTime = e.StartTime;
                        _timeFilterChanged.EndFilterTime = e.EndTime;

                        TxtFromTime.Text = e.StartTime.ToString("hh:mm tt");
                        TxtToTime.Text = e.EndTime.ToString("hh:mm tt");
                    }));
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error pgTimeFilter._thumb_OnTimeFilterChanged; {ex.Message}");
                App.Log.LogError(_logChannel, "-", ex, null, "pgTimeFilter._thumb_OnTimeFilterChanged");
            }
            finally
            {
                //if (locked)
                _filterLock.Release();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => {
                _thumb1.InitThumb(_thumb2, CvSliderContainer.ActualHeight);
                _thumb2.InitThumb(_thumb1, CvSliderContainer.ActualHeight);
            }));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        { }

        public void InitFilter(ResourceDictionary languageResource)
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(languageResource);
        }

        private void TimeFilterTriggerThreadWorking()
        {
            bool locked = false;

            while (true)
            {
                locked = false;
                if (IsFilterActived)
                {
                    try
                    {
                        if (_filterLock.WaitAsync().Wait(1000 * 3))
                        {
                            locked = true;

                            if (_timeFilterChanged.TimeChanged)
                            {
                                DateTime expiredTime = _timeFilterChanged.LastChangedTime.AddSeconds(_minChangeTriggerSec);

                                if (expiredTime.Subtract(DateTime.Now).TotalMilliseconds <= 0)
                                {
                                    _timeFilterChanged.TimeChanged = false;

                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        App.ShowDebugMsg($@"pgTimeFilter.TimeFilterTriggerThreadWorking - III");

                                        TxtFromTime.Foreground = _timeNormalBackground;
                                        TxtToTime.Foreground = _timeNormalBackground;
                                    }));

                                    if (IsFilterActived)
                                        RaiseOnTimeFilterChangedTrigger(_timeFilterChanged.StartFilterTime, _timeFilterChanged.EndFilterTime);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"Error pgTimeFilter.TimeFilterTriggerThreadWorking; {ex.Message}");
                        App.Log.LogError(_logChannel, "-", ex, null, "pgTimeFilter.TimeFilterTriggerThreadWorking");
                    }
                    finally
                    {
                        //if (locked)
                        _filterLock.Release();
                    }
                }
                Task.Delay(1000).Wait();
            }
        }

        private void BtnFilter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                App.ShowDebugMsg($@"pgTimeFilter.BtnFilter_MouseLeftButtonDown");

                if (IsFilterActived)
                    RaiseOnTimeFilterChangedTrigger(_timeFilterChanged.StartFilterTime, _timeFilterChanged.EndFilterTime);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error BtnFilter_MouseLeftButtonDown; {ex.Message}");
                App.Log.LogError(_logChannel, "-", ex, null, "pgTimeFilter.BtnFilter_MouseLeftButtonDown");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetFilter();
        }

        public void ResetFilter()
        {
            try
            {
                App.ShowDebugMsg($@"pgTimeFilter.ResetFilter");

                _thumb1.ResetFilter();
                _thumb2.ResetFilter();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error ResetFilter; {ex.Message}");
                App.Log.LogError(_logChannel, "-", ex, null, "pgTimeFilter.ResetFilter");
            }
        }

        private void RaiseOnTimeFilterChangedTrigger(DateTime startTime, DateTime endTime)
        {
            try
            {
                App.ShowDebugMsg($@"pgTimeFilter.RaiseOnTimeFilterChangedTrigger - I");

                if (OnTimeFilterChangedTrigger != null)
                    OnTimeFilterChangedTrigger.Invoke(null, new TimeFilterEventArgs(startTime, endTime));

                App.ShowDebugMsg($@"pgTimeFilter.RaiseOnTimeFilterChangedTrigger - II");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error pgTimeFilter.RaiseOnTimeFilterChangedTrigger; {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception("Unhandled event exception", ex), null, "pgTimeFilter.RaiseOnTimeFilterChangedTrigger");
            }
        }

        private void RaiseOnTimeFilterReset(DateTime startTime, DateTime endTime)
        {
            try
            {
                App.ShowDebugMsg($@"pgTimeFilter.RaiseOnTimeFilterReset - I");

                if (OnTimeFilterResetTrigger != null)
                    OnTimeFilterResetTrigger.Invoke(null, new TimeFilterEventArgs(startTime, endTime));

                App.ShowDebugMsg($@"pgTimeFilter.RaiseOnTimeFilterReset - II");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error pgTimeFilter.RaiseOnTimeFilterReset; {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception("Unhandled event exception", ex), null, "pgTimeFilter.RaiseOnTimeFilterReset");
            }
        }

        
    }
}
