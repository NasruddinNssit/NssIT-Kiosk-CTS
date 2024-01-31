using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormCoinTest.LowCoin
{
    public class LowCoinBinChecker
    {
        private int MinLowStateCounterForEmptyCoinBox = 16;
        private int _coinCent = 10;

        private int _lowDispensedCoinCount = 0;
        private int _lowDispensedTimes = 0;

        private int _lowCoinStateCount = 0;
        private int _lowCoinEmptyStateCount = 0;
        

        public LowCoinBinChecker(int coinCent)
        {
            _coinCent = coinCent;
        }

        private bool _isLowStatusOnInit = true;
        public bool IsLowStatusOnInit
        {
            get => _isLowStatusOnInit;
            set
            {
                _isLowStatusOnInit = value;
                if (_isLowStatusOnInit == true)
                {
                    _isLowCoin = true;
                }
            }
        }

        private bool _isLowCoin = false;
        public bool IsLowCoin 
        { 
            get
            {
                return _isLowCoin;
            }
            set
            {
                if (value == false)
                {
                    _isLowStatusOnInit = false;
                    _lowDispensedTimes = 0;
                    _lowDispensedCoinCount = 0;
                    _lowCoinStateCount = 0;
                    _lowCoinEmptyStateCount = 0;
                }
                _isLowCoin = value;
            }
        }

        public bool IsRebootRequested
        {
            get
            {
                return (_lowCoinStateCount > 1);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (_lowCoinEmptyStateCount >= MinLowStateCounterForEmptyCoinBox);
            }
        }

        public bool IsEnableToDispense
        {
            get
            {
                if (IsLowCoin)
                {
                    if ((_isLowStatusOnInit) || (IsEmpty))
                        return false;

                    else if (_coinCent == 10)
                    {
                        if (_lowDispensedTimes > 2)
                            return false;

                        else if (_lowDispensedCoinCount > 4)
                            return false;

                        else
                            return true;
                    }
                    else if (_coinCent == 20)
                    {
                        if (_lowDispensedTimes > 2)
                            return false;

                        else if (_lowDispensedCoinCount > 3)
                            return false;

                        else
                            return true;
                    }
                    else if (_coinCent == 50)
                    {
                        if (_lowDispensedTimes > 2)
                            return false;

                        else if (_lowDispensedCoinCount > 2)
                            return false;

                        else
                            return true;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
        }

        public void Dispense(int coinCount)
        {
            if (IsLowCoin)
            {
                _lowDispensedCoinCount += coinCount;
                _lowDispensedTimes += 1;
            }
        }

        public string GetLowStatusMessage()
        {
            string sttMsg = null;

            if ((IsLowStatusOnInit) || (IsLowCoin) || (IsEnableToDispense == false))
            {
                if (IsLowStatusOnInit)
                {
                    sttMsg = $@"{_coinCent} Cent low init.";

                    if (IsEnableToDispense == false)
                        sttMsg += $@" and unable dispense";

                    else
                        sttMsg += $@" but still able to dispense";
                }
                else if (IsEmpty)
                {
                    sttMsg = $@"{_coinCent} Cent is EMPTY.";
                }
                else if (IsLowCoin)
                {
                    sttMsg = $@"{_coinCent} Cent low";

                    if (IsEnableToDispense == false)
                        sttMsg += $@" and unable dispense";

                    else
                        sttMsg += $@" but still able dispense";
                }
                else if (IsEnableToDispense == false)
                {
                    sttMsg = $@"{_coinCent} Cent unable dispense";
                }
                
                if (sttMsg != null)
                {
                    sttMsg += $@"; Dispensed Count: {_lowDispensedCoinCount}; Dispensed Times: {_lowDispensedTimes}";
                }
            }
            else
            {
                sttMsg = null;
            }

            return sttMsg;
        }

        public void UpdateEmptyStateCounter(bool isLowCoinStatus)
        {
            IsLowCoin = isLowCoinStatus;
            if ((isLowCoinStatus) && (IsEnableToDispense == false))
                _lowCoinEmptyStateCount++;
        }

        public void UpdateLowStateAfterCoinDispensed(bool isLowCoinStatus)
        {
            IsLowCoin = isLowCoinStatus;
            if ((IsLowStatusOnInit == false) && (IsLowCoin == true))
            {
                _lowCoinStateCount++;
            }
        }
    }
}
