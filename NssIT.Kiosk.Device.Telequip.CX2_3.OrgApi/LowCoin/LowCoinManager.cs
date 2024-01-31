using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.ConstantX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.LowCoin
{
    public class LowCoinManager
    {
        private LowCoinBinChecker _10LowCoinChecker = new LowCoinBinChecker(CoinCent.Coin10Cent);
        private LowCoinBinChecker _20LowCoinChecker = new LowCoinBinChecker(CoinCent.Coin20Cent);
        private LowCoinBinChecker _50LowCoinChecker = new LowCoinBinChecker(CoinCent.Coin50Cent);

        private bool _coinRecoveredSuccessful = true;
        private int _machineFailToSpinCount = 0;

        public LowCoinManager()
        { }

        /// <summary>
        /// Request to reboot application.
        /// </summary>
        public bool IsReqToStartApp
        {
            get;
            set;
        } = false;

        public bool IsDispensePossible
        {
            get
            {
                if (IsReqToStartApp == true)
                    return false;

                if (IsSpinMotorMalfunction)
                    return false;

                else if ((_10LowCoinChecker.IsLowStatusOnInit == true)
                    || (_20LowCoinChecker.IsLowStatusOnInit == true)
                    || (_50LowCoinChecker.IsLowStatusOnInit == true))
                    return false;

                else if (_10LowCoinChecker.IsEnableToDispense == false)
                    return false;

                //---------------------------------------------------------------
                //..For Testing; Remark below block for testing; Testing for recovering dipense coin when first dispense coin has fail.

                else if (_coinRecoveredSuccessful == false)
                    return false;

                else if ((_20LowCoinChecker.IsEnableToDispense == false)
                    && (_50LowCoinChecker.IsEnableToDispense == false))
                    return false;
                //---------------------------------------------------------------

                else if (_10LowCoinChecker.IsEnableToDispense == true)
                    return true;

                else
                    return false;
            }
        }

        public bool IsAllCoinEmpty
        {
            get
            {
                if (_10LowCoinChecker.IsEmpty == true)
                    return true;
                else
                    return false;
            }
        }

        public bool IsRebootCoinMachineRequested
        {
            get
            {
                bool isRebootRequested = false;

                if (_10LowCoinChecker.IsRebootCoinRequested)
                    isRebootRequested = true;

                if (_20LowCoinChecker.IsRebootCoinRequested)
                    isRebootRequested = true;

                if (_50LowCoinChecker.IsRebootCoinRequested)
                    isRebootRequested = true;

                return isRebootRequested;
            }
        }

        /// <summary>
        /// When 2 or more Coin Bin are not dispense coin, request to reboot-spin-motor coin machine. Else the coin machine may not able to dispense coin.
        /// </summary>
        public bool IsRequestCriticalRebootSpinMotor
        {
            get
            {
                if (IsAllCoinEmpty == false)
                {
                    if ((_10LowCoinChecker.IsLowStatusOnInit == true)
                        || (_20LowCoinChecker.IsLowStatusOnInit == true)
                        || (_50LowCoinChecker.IsLowStatusOnInit == true))
                    {
                        return false;
                    }

                    /////..... no need to RebootSpinMotor if 10 has already empty
                    else if (_10LowCoinChecker.IsEmpty == true)
                    {
                        return false;
                    }


                    /////..... 10 Cent still NOT able to be dispensed
                    else if (_10LowCoinChecker.IsEnableToDispense == false)
                    {
                        return true;
                    }

                    ////----------------------------------------------------------------------------------
                    /////..... 10 Cent still ABLE to be dispensed with Low Status
                    else if ((_10LowCoinChecker.IsLowCoin == true) && (_10LowCoinChecker.IsEnableToDispense == true))
                    {
                        if (((_20LowCoinChecker.IsEnableToDispense == false) && (_50LowCoinChecker.IsEnableToDispense == false))
                            &&
                            ((_20LowCoinChecker.IsEmpty == false) && (_50LowCoinChecker.IsEmpty == false)))
                        {
                            return true;
                        }
                        else if ((_50LowCoinChecker.IsEnableToDispense == false)
                            &&
                            ((_20LowCoinChecker.IsEmpty == true) && (_50LowCoinChecker.IsEmpty == false)))
                        {
                            return true;
                        }
                        else if ((_20LowCoinChecker.IsEnableToDispense == false)
                            &&
                            ((_20LowCoinChecker.IsEmpty == false) && (_50LowCoinChecker.IsEmpty == true)))
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                    ////----------------------------------------------------------------------------------
                    /////..... 10 Cent still NORMAL available
                    else if (_10LowCoinChecker.IsLowCoin == false)
                    {
                        if (((_20LowCoinChecker.IsEnableToDispense == false) && (_50LowCoinChecker.IsEnableToDispense == false))
                            &&
                            ((_20LowCoinChecker.IsEmpty == false) && (_50LowCoinChecker.IsEmpty == false)))
                        {
                            return true;
                        }
                        else if ((_50LowCoinChecker.IsEnableToDispense == false)
                            &&
                            ((_20LowCoinChecker.IsEmpty == true) && (_50LowCoinChecker.IsEmpty == false)))
                        {
                            return true;
                        }
                        else if ((_20LowCoinChecker.IsEnableToDispense == false)
                            &&
                            ((_20LowCoinChecker.IsEmpty == false) && (_50LowCoinChecker.IsEmpty == true)))
                        {
                            return true;
                        }
                        else
                            return false;
                    }

                    ////----------------------------------------------------------------------------------
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public bool IsSpinMotorMalfunction
        {
            get
            {
                return (_machineFailToSpinCount > 5);
            }
        }

        public bool GetDispensePossibleByCoinCent(CoinCent coinCent)
        {
            if (coinCent == CoinCent.Coin10Cent)
            {
                return _10LowCoinChecker.IsEnableToDispense;
            }
            else if (coinCent == CoinCent.Coin20Cent)
            {
                return _20LowCoinChecker.IsEnableToDispense;
            }
            else if (coinCent == CoinCent.Coin50Cent)
            {
                return _50LowCoinChecker.IsEnableToDispense;
            }
            else
            {
                throw new Exception($@"Invalid CoinCent {coinCent}; Get Dispense Possible By Coin-cent;");
            }
        }

        public void QueryLowCoin(out bool low10Cent, out bool low20Cent, out bool low50Cent)
        {
            low10Cent = _10LowCoinChecker.IsLowCoin;
            low20Cent = _20LowCoinChecker.IsLowCoin;
            low50Cent = _50LowCoinChecker.IsLowCoin;
        }

        public void QueryCoinStatus(int coinCent, out bool isLow, out bool isEmpty, out bool isEnableToDispense)
        {
            isLow = false;
            isEmpty = false;
            isEnableToDispense = true;

            switch (coinCent)
            {
                case 10:
                    isLow = _10LowCoinChecker.IsLowCoin;
                    isEmpty = _10LowCoinChecker.IsEmpty;
                    isEnableToDispense = _10LowCoinChecker.IsEnableToDispense;
                    break;
                case 20:
                    isLow = _20LowCoinChecker.IsLowCoin;
                    isEmpty = _20LowCoinChecker.IsEmpty;
                    isEnableToDispense = _20LowCoinChecker.IsEnableToDispense;
                    break;
                case 50:
                    isLow = _50LowCoinChecker.IsLowCoin;
                    isEmpty = _50LowCoinChecker.IsEmpty;
                    isEnableToDispense = _50LowCoinChecker.IsEnableToDispense;
                    break;
            }
        }

        public void UpdateLowCoinStates(bool is10CentLowStatus, bool is20CentLowStatus, bool is50CentLowStatus)
        {
            _10LowCoinChecker.IsLowCoin = is10CentLowStatus;
            _20LowCoinChecker.IsLowCoin = is20CentLowStatus;
            _50LowCoinChecker.IsLowCoin = is50CentLowStatus;
        }

        public void UpdateDispenseStatus(string dispenseCode)
        {
            if (int.TryParse(dispenseCode.Substring(0, 1), out int x10Count) && (x10Count > 0))
            {
                _10LowCoinChecker.Dispense(x10Count);
            }
            if (int.TryParse(dispenseCode.Substring(1, 1), out int x20Count) && (x20Count > 0))
            {
                _20LowCoinChecker.Dispense(x20Count);
            }
            if (int.TryParse(dispenseCode.Substring(2, 1), out int x50Count) && (x50Count > 0))
            {
                _50LowCoinChecker.Dispense(x50Count);
            }
        }

        public void UpdateLowCoinStatesOnInit(bool is10CentLowStatus, bool is20CentLowStatus, bool is50CentLowStatus)
        {
            _10LowCoinChecker.IsLowStatusOnInit = is10CentLowStatus;
            _20LowCoinChecker.IsLowStatusOnInit = is20CentLowStatus;
            _50LowCoinChecker.IsLowStatusOnInit = is50CentLowStatus;
        }

        /// <summary>
        /// After coin machime has rebooted, check coin bin is emtpy or not. Low Coin Status will be updated at the same time.
        /// </summary>
        /// <param name="is10CentLowStatus"></param>
        /// <param name="is20CentLowStatus"></param>
        /// <param name="is50CentLowStatus"></param>
        public void UpdateEmptyCoinCounterAfterCoinMachReboot(bool is10CentLowStatus, bool is20CentLowStatus, bool is50CentLowStatus)
        {
            _10LowCoinChecker.UpdateEmptyStateCounter(is10CentLowStatus);
            _20LowCoinChecker.UpdateEmptyStateCounter(is20CentLowStatus);
            _50LowCoinChecker.UpdateEmptyStateCounter(is50CentLowStatus);
        }

        /// <summary>
        /// Report to application about coin machine reboot success or not. Low Coin Status will be updated at the same time.
        /// </summary>
        /// <param name="is10CentLowStatus"></param>
        /// <param name="is20CentLowStatus"></param>
        /// <param name="is50CentLowStatus"></param>
        public void UpdateLowStateAfterCoinMachReboot(bool is10CentLowStatus, bool is20CentLowStatus, bool is50CentLowStatus)
        {
            _10LowCoinChecker.IsLowCoin = is10CentLowStatus;
            _20LowCoinChecker.IsLowCoin = is20CentLowStatus;
            _50LowCoinChecker.IsLowCoin = is50CentLowStatus;

            _coinRecoveredSuccessful = true;
            if (is10CentLowStatus == true)
            {
                if ((is20CentLowStatus == true) || (is50CentLowStatus == true))
                    _coinRecoveredSuccessful = false;
            }
            else if (is20CentLowStatus == true)
            {
                if ((is10CentLowStatus == true) || (is50CentLowStatus == true))
                    _coinRecoveredSuccessful = false;
            }
            else if (is50CentLowStatus == true)
            {
                if ((is10CentLowStatus == true) || (is20CentLowStatus == true))
                    _coinRecoveredSuccessful = false;
            }
        }

        /// <summary>
        /// Update Spin Motor State; This is to check fail state of motor spinning then confirm coin bin motor is malfunction.
        /// </summary>
        /// <param name="isSpinSuccess"></param>
        public void UpdateSpinMotorState(bool isSpinSuccess)
        {
            if (isSpinSuccess == false)
                _machineFailToSpinCount++;
            else
                _machineFailToSpinCount = 0;
        }

        /// <summary>
        /// Return low coin status only. Return null if all coin bin are in good/normal condition
        /// </summary>
        /// <returns></returns>
        public string GetStatusMessage()
        {
            string retMsg = "";
            string tmpStr;

            if ((_10LowCoinChecker.IsLowStatusOnInit) || (_10LowCoinChecker.IsLowCoin) || (_10LowCoinChecker.IsEnableToDispense == false))
            {
                tmpStr = _10LowCoinChecker.GetLowStatusMessage();
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }
            if ((_20LowCoinChecker.IsLowStatusOnInit) || (_20LowCoinChecker.IsLowCoin) || (_20LowCoinChecker.IsEnableToDispense == false))
            {
                tmpStr = _20LowCoinChecker.GetLowStatusMessage();
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }
            if ((_50LowCoinChecker.IsLowStatusOnInit) || (_50LowCoinChecker.IsLowCoin) || (_50LowCoinChecker.IsEnableToDispense == false))
            {
                tmpStr = _50LowCoinChecker.GetLowStatusMessage();
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }

            if (IsReqToStartApp)
            {
                tmpStr = "Request to restart application";
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }

            if (IsSpinMotorMalfunction)
            {
                tmpStr = "Rebooting bin motor is malfunction";
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }

            if (IsDispensePossible == false)
            {
                tmpStr = "!!! Unable to dispense any coin !!!";
                if (tmpStr != null)
                    retMsg += tmpStr + "; ";
            }

            if (string.IsNullOrWhiteSpace(retMsg))
                return null;
            else
                return retMsg;
        }
    }
}
