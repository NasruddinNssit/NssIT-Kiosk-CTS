using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.ConstantX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.LowCoin
{
    public class LowCoinBinChecker
    {
        private int MinLowStateCounterForEmptyCoinBox = 10;
        private CoinCent _coinCent = CoinCent.Coin10Cent;

        private int _lowDispensedCoinCount = 0;
        private int _lowDispensedTimes = 0;

        private int _lowCoinEmptyStateCount = 0;

        public LowCoinBinChecker(CoinCent coinCent)
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
                    _lowCoinEmptyStateCount = 0;
                }
                _isLowCoin = value;
            }
        }

        public bool IsRebootCoinRequested
        {
            get
            {
                if (IsEmpty)
                    return false;

                else if (IsLowStatusOnInit)
                    return false;

                else if (IsLowCoin)
                    return true;
                
                else
                    return false;
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
                    if ((IsLowStatusOnInit) || (IsEmpty))
                        return false;

                    else if (_coinCent == CoinCent.Coin10Cent)
                    {
                        if (_lowDispensedTimes > 1)
                            return false;

                        else if (_lowDispensedCoinCount > 4)
                            return false;

                        else
                            return true;
                    }
                    else if (_coinCent == CoinCent.Coin20Cent)
                    {
                        if (_lowDispensedTimes > 1)
                            return false;

                        else if (_lowDispensedCoinCount > 3)
                            return false;

                        else
                            return true;
                    }
                    else if (_coinCent == CoinCent.Coin50Cent)
                    {
                        if (_lowDispensedTimes > 1)
                            return false;

                        else if (_lowDispensedCoinCount > 1)
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
                    sttMsg = $@"Low {(int)_coinCent} Cent coin on init.";

                    if (IsEnableToDispense == false)
                        sttMsg += $@" and unable to dispense";

                    else
                        sttMsg += $@" but still available";
                }
                else if (IsEmpty)
                {
                    sttMsg = $@"{(int)_coinCent} Cent coin is EMPTY.";
                }
                else if (IsEnableToDispense == false)
                {
                    sttMsg = $@"Not able to dispense {(int)_coinCent} Cent coin";
                }
                else if (IsLowCoin)
                {
                    sttMsg = $@"Low {(int)_coinCent} Cent coin but still available";
                }

                if (sttMsg != null)
                {
                    sttMsg += $@"; Low Dispensed Count: {_lowDispensedCoinCount}; Low Dispensed Times: {_lowDispensedTimes}";
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
    }
}