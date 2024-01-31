using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public class ICPassHttpReader : IIdentityReader
    {
        private string _icReadingURL = @"http://localhost:1234/Para=2";

        public ICPassHttpReader(string icReadingURL = @"http://localhost:1234/Para=2")
        {
            _icReadingURL = icReadingURL;
        }

        public PassengerIdentity ReadIC(int waitDelaySec = 10)
        {
            PassengerIdentity retPsggId = null;
            string errorMsg = null;

            try
            {
                if (ReadICDataToString(out string dataString, out string errorMessage, waitDelaySec))
                {
                    string[] split = dataString.Split(new Char[] { ',' }, StringSplitOptions.None);

                    if (int.TryParse(split[0], out int readResultCode))
                    {
                        if (readResultCode == 0)
                        {
                            string icNo = split[2];
                            string name = split[3];
                            retPsggId = new PassengerIdentity(true, null, icNo, name, null);
                        }
                        else
                        {
                            errorMsg = $@"{readResultCode};{split[1]}";
                            retPsggId = new PassengerIdentity(false, null, null, null, errorMsg);
                        }
                    }
                    else
                    {
                        retPsggId = new PassengerIdentity(false, null, null, null, "Unexpected error (1); Unable to read from IC reader.");
                    }
                }
                else
                {
                    retPsggId = new PassengerIdentity(false, null, null, null, errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                //CYA-DEBUG need to handle error here.
                retPsggId = new PassengerIdentity(false, null, null, null, "Unexpected error (2); " + ex.Message);
            }

            return retPsggId;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            bool ReadICDataToString(out string dataString, out string errorMessage, int waitDelayInSec = 10)
            {
                dataString = null;
                errorMessage = null;
                int waitDelSec = waitDelayInSec;

                try
                {
                    var client = new HttpClient();

                    Task<string> readTask = client.GetStringAsync(_icReadingURL);
                    readTask.Wait(1000 * waitDelSec);

                    if (readTask.IsCompleted)
                    {
                        dataString = readTask.Result;
                        return true;
                    }
                    else
                    {
                        errorMessage = "Timeout. Unable to read your IC. Please try again";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    dataString = null;
                    errorMessage = "Error when reading IC;" + ex.Message;
                    //CYA-DEBUG need to handle error here.
                    return false;
                }
            }
        }

    }
}
