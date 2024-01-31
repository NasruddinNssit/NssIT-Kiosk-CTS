using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public class PassengerIdentity
    {
        public bool IsIDReadSuccess { get; private set; } = false;
        public string PassportNumber { get; private set; } = null;
        public string IdNumber { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string Message { get; private set; } = null;

        public PassengerIdentity(bool isIDReadSuccess, string passportNumber, string idNumber, string name, string message)
        {
            IsIDReadSuccess = isIDReadSuccess;
            PassportNumber = string.IsNullOrWhiteSpace(passportNumber) ? null : passportNumber.Trim();
            IdNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber.Trim();
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();

            if (IsIDReadSuccess)
            {
                if (Name is null)
                {
                    IsIDReadSuccess = false;
                    Message = (string.IsNullOrWhiteSpace(Message) ? "" : $@"{Message};") + $@"Name not found";
                }
                if ((PassportNumber is null) && (IdNumber is null))
                {
                    IsIDReadSuccess = false;
                    Message = (string.IsNullOrWhiteSpace(Message) ? "" : $@"{Message};") + $@"Identity Number not found";
                }
            }
            else
            {
                if (Message is null)
                {
                    Message = "No data found-*.";
                }
            }
        }

    }
}
