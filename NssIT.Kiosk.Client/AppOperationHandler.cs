using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client
{
    public class AppOperationHandler : IDisposable
    {

        private const string LogChannel = "OperationHandler";


        private string _procIdPreFix = null;
        private string _startOperationTimeString = "00:00";
        private string _endOprationTimeString = "00:00";
        private bool _isInOperationTime = false;
        private DateTime? _startOperationTime = null;
        private DateTime? _endOperationTime = null;

        public EventHandler<OnCloseOperationTimeEventArgs> OnRequestOffOperation;
        public EventHandler OnRequestOnOperation;


        private Thread _worker;



        public AppOperationHandler(string startOperationTime, string endOprationTime)
        {
            _isInOperationTime=true;

            _procIdPreFix = DateTime.Now.ToString("yyyyMMddHHmm") + "-";

            _startOperationTimeString = startOperationTime;
            _endOprationTimeString = endOprationTime;


            _startOperationTime = GetNextStartOperationTime(null);
            _endOperationTime = GetNextEndOperationTime(null);
        }

        public void Load()
        {
            AppDecorator.Config.Setting setting = AppDecorator.Config.Setting.GetSetting();

            if(setting.NoOperationTime == false)
            {
                _worker = new Thread(new ThreadStart(SchecduleThreadWorking));
                _worker.IsBackground = true;
                _worker.Start();
            }
        }


        private DbLog _schdlog = null;

        private DbLog Log
        {
            get => _schdlog ?? (_schdlog = DbLog.GetDbLog());
        }

        private DateTime GetNextStartOperationTime(DateTime? lastOperationTime)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime nextStartOperationTime;

            if (lastOperationTime.HasValue == false)
            { 
                if(_startOperationTimeString.Length > 0)
                {

                    string dateStr = $@"{DateTime.Now.ToString("yyyy/MM/dd")} {_startOperationTimeString}";
                    nextStartOperationTime = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", provider);
                }
                else
                {
                    // Get the current date
                    DateTime currentDate = DateTime.Now;
                    // Set the time portion to 12:00 PM (noon)
                    nextStartOperationTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 5, 0, 0);
                }
            }
            else
            {
                 nextStartOperationTime = GetNextDayStartOperationTime();
            }

            return nextStartOperationTime;

             DateTime GetNextDayStartOperationTime()
            {
                DateTime currenTime = DateTime.Now;
                DateTime resultTime = currenTime;
               
                DateTime nextDay = currenTime.AddDays(1);
                string dateStr = $@"{nextDay.ToString("yyyy/MM/dd")} {_startOperationTimeString}";

                resultTime = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", provider);

                return resultTime;
               
            }
        }


        private DateTime GetNextEndOperationTime(DateTime? lastOperationTime)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime nextEndOperationTime;


            if (lastOperationTime.HasValue == false)
            {
                if (_endOprationTimeString.Length > 0)
                {

                    string dateStr = $@"{DateTime.Now.ToString("yyyy/MM/dd")} {_endOprationTimeString}";
                    nextEndOperationTime = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", provider);
                }
                else
                {
                    // Get the current date
                    DateTime currentDate = DateTime.Now;
                    // Set the time portion to 12:00 PM (noon)
                    nextEndOperationTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 0, 0);
                }
            }
            else
            {
                nextEndOperationTime = GetNextDayEndOperationTime();
            }

            return nextEndOperationTime;

            DateTime GetNextDayEndOperationTime()
            {
                DateTime currenTime = DateTime.Now;
                DateTime resultTime = currenTime;

                DateTime nextDay = currenTime.AddDays(1);
                string dateStr = $@"{nextDay.ToString("yyyy/MM/dd")} {_endOprationTimeString}";

                resultTime = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", provider);

                return resultTime;

            }


        }

        private bool CheckIsValidTimeToStartOperation(DateTime currentTime, DateTime startOperationTime, out bool isTimeToStartOperation)
        {
            isTimeToStartOperation = false;

            if (startOperationTime.Subtract(currentTime).TotalSeconds < 0)
            {
                isTimeToStartOperation = true;
                return true;
            }
            else
            {
                return true;
            }

            //if (currentTime >= startOperationTime && currentTime <= endOperationTime)
            //{
            //    isTimeToStartOperation = true;
            //    return true;
            //}
            //else
            //    return true;
        }

        private bool CheckIsValidTimeToEndOperation(DateTime currentTime, DateTime endOperationTime , out bool isTimeToEndOperation)
        {
            isTimeToEndOperation = false;
            if (endOperationTime.Subtract(currentTime).TotalSeconds < 0)
            {
                isTimeToEndOperation = true;
                return true;
            }else
                return true;
        }


        private bool _isScheduleWorkingContinue = true;

        private long _processInx = 0;

        private void SchecduleThreadWorking()
        {
            //Below allow system to stablelized before start schedule.
            Thread.Sleep(3 * 1000);


            string procId = "-";
            bool isInOperationState = false;
            while(_isScheduleWorkingContinue)
            {
                _processInx += 1;

                try
                {
                    procId = $@"{_procIdPreFix}{_processInx.ToString()}";
                    if(!isInOperationState) 
                    {
                        if (IsTimeForStartOperation())
                        {
                            isInOperationState = true;
                            OnRequestOnOperation?.Invoke(this, new EventArgs());
                        }
                        else
                        {
                            isInOperationState = false;
                            OnRequestOffOperation?.Invoke(this, new OnCloseOperationTimeEventArgs { EndOperationTime = _endOperationTime.Value, StartOperationTime = _startOperationTime.Value });

                        }
                    }
                    else
                    {
                        if (IsTimeForEndOperation())
                        {
                            isInOperationState = false;

                            OnRequestOffOperation?.Invoke(this, new OnCloseOperationTimeEventArgs { EndOperationTime = _endOperationTime.Value, StartOperationTime = _startOperationTime.Value });
                        }
                    }
                  
                }catch (Exception ex)
                {
                    Log.LogError(LogChannel, procId, ex, "S0101", "AppOperationHandler.SchecduleWorking");

                }
                finally
                {
                    if (isInOperationState)
                    {
                        UpdateTimeForNextStartOperation();
                    }
                    else
                    {
                        if (IsTimeForEndOperation())
                        {
                            UpdateTimeForNextEndOperation();
                        }
                       
                    }

                    Thread.Sleep(2 * 1000);

                    if(isInOperationState)
                    {
                        if((_endOperationTime.HasValue) && (_endOperationTime.Value.Subtract(DateTime.Now).TotalHours > 1))
                        {
                            Thread.Sleep(10 * 60 * 1000);
                            //Thread.Sleep(3 * 1000);
                        }
                        else
                        {
                            Thread.Sleep(3 * 1000);
                        }
                    }
                    else
                    {
                        if((_startOperationTime.HasValue) && (_startOperationTime.Value.Subtract(DateTime.Now).TotalHours > 1))
                        {
                            Thread.Sleep(10 * 60 * 1000);
                            //Thread.Sleep(3 * 1000);
                        }
                        else
                        {
                            Thread.Sleep(3 * 1000);
                        }
                    }
                }
            }
        }

        public void UpdateTimeForNextEndOperation()
        {
            if(_endOperationTime.HasValue == false)
            {
                _endOperationTime = GetNextEndOperationTime(null);
            }
            else
            {
                DateTime nextEndOperationTime = GetNextEndOperationTime(_endOperationTime.Value);
                _endOperationTime = nextEndOperationTime;
            }

        }


        public void UpdateTimeForNextStartOperation()
        {
            if(_startOperationTime.HasValue == false)
            {
                _startOperationTime = GetNextStartOperationTime(null);
            }
            else
            {
                DateTime nextStartOperationDate = GetNextStartOperationTime(_startOperationTime.Value);
                _startOperationTime = nextStartOperationDate;
            }
        }


        public bool IsTimeForEndOperation() 
        {
            bool isTimeForEndOperation = false;
            DateTime currentTime = DateTime.Now;
            if(CheckIsValidTimeToEndOperation(currentTime, _endOperationTime.Value, out bool isTimeToEndOperation))
            {
                if (isTimeToEndOperation)
                    isTimeForEndOperation = true;
                else
                    isTimeForEndOperation = false;
            }

            return isTimeForEndOperation;
        }
        public bool IsTimeForStartOperation()
        {
            bool isTimeForStartOperation = false;
            DateTime currentTime = DateTime.Now;

            if(CheckIsValidTimeToStartOperation(currentTime,_startOperationTime.Value, out bool isTimeToStartOperation))
            {
                if(isTimeToStartOperation)
                    isTimeForStartOperation = true;
                else 
                    isTimeForStartOperation= false;
            }

            return isTimeForStartOperation;
        }

        public void Dispose()
        {
            _isScheduleWorkingContinue = false;
        }
    }
}
