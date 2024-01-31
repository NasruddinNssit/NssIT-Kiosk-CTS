using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
	public class AccessCommandPack : IDisposable
	{
		private string _errorMsg = null;
		private IKioskMsg _resultData = null;

		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public Guid ExecutionRefId { get; private set; } = Guid.NewGuid();

		public DateTime CreationTimeStamp { get; private set; } = DateTime.Now;
		public DateTime ResultTimeStamp { get; private set; } = DateTime.MaxValue;

		public AccessDBCommandCode CommandCode { get; private set; } = AccessDBCommandCode.UnKnown;
		public string CommandDesc { get => Enum.GetName(typeof(AccessDBCommandCode), CommandCode); }

		public IAccessDBCommand Command { get; private set; } = null;

		public bool IsResultDelivered { get; private set; } = false;
		public bool IsResultReady { get; private set; } = false;
		public bool IsErrorFound { get => (string.IsNullOrWhiteSpace(_errorMsg) == false); }

		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone
		{
			get
			{
				if (Command != null)
					return Command.ProcessDone;
				else
					return false;
			}
		}

		private AccessCommandPack() { }

		public AccessCommandPack(IAccessDBCommand command)
		{
			ProcessId = command.ProcessId;
			NetProcessId = command.NetProcessId;

			Command = command;
			CommandCode = command.CommandCode;
		}

		public AccessCommandPack DuplicatedDummyCommandPack()
		{
			return new AccessCommandPack()
			{
				ProcessId = this.ProcessId,
				NetProcessId = this.NetProcessId,

				ExecutionRefId = this.ExecutionRefId,
				CreationTimeStamp = this.CreationTimeStamp,

				CommandCode = this.CommandCode
			};
		}

		/// <summary>
		/// Once this method is called, command execution will always considered already done and result is ready.
		/// </summary>
		/// <param name="errorFound">If errorFound is true, errorMessage must have a valid message, else an unknown error message will be used.</param>
		/// <param name="resultData"></param>
		/// <param name="errorMessage">Must have a valid message when errorFound is true</param>
		public void UpSertResult(bool errorFound, IKioskMsg resultData = null, string errorMessage = null)
		{
			if (errorFound)
				WriteError(errorMessage ?? "");
			else
				WriteError(null);

			_resultData = resultData;

			// Once this command is called, IsResultReady will be set to true even resultData is null and errorMessage is nullResultTimeStamp
			ResultTimeStamp = DateTime.Now;
			IsResultReady = true;
		}

		/// <summary>
		/// Method will update IsResultDelivered flag. Wait for result within maxWaitPeriod. Return true when result is ready. Else return false.
		/// </summary>
		/// <param name="maxWaitPeriod"></param>
		/// <param name="errorMsg">If error found, output will not be null and not be blanked message.</param>
		/// <param name="resultData">Output the result instant for the related command execution.</param>
		/// <returns>Return true when result is ready. Else return false.</returns>
		public bool PopUpResult(TimeSpan maxWaitPeriod, out string errorMsg, out IKioskMsg resultData)
		{
			errorMsg = null;
			resultData = null;

			IKioskMsg retVal = WaitingReadResult(maxWaitPeriod);

			if (IsResultReady)
			{
				errorMsg = _errorMsg;
				resultData = _resultData;
				IsResultDelivered = true;
			}

			return IsResultReady;
		}

		/// <summary>
		/// Method will not update IsResultDelivered flag. Return true when result is ready. Else return false.
		/// </summary>
		/// <param name="errorMsg"></param>
		/// <param name="executionResult"></param>
		/// <returns>Return true when result is ready. Else return false.</returns>
		public bool PreviewResult(out string errorMsg, out IKioskMsg executionResult)
		{
			errorMsg = null;
			executionResult = null;

			if (IsResultReady)
			{
				errorMsg = _errorMsg;
				executionResult = _resultData;
			}

			return IsResultReady;
		}

		private IKioskMsg WaitingReadResult(TimeSpan maxWaitingPeriod)
		{
			if (maxWaitingPeriod.TotalSeconds <= 0)
				return _resultData;

			DateTime expireTime = DateTime.Now.Add(maxWaitingPeriod);

			while ((!IsResultReady) && (expireTime.Subtract(DateTime.Now).TotalSeconds > 0))
			{
				if ((expireTime.Subtract(DateTime.Now).TotalSeconds > 10))
					Task.Delay(1000 * 10).Wait();

				else if ((expireTime.Subtract(DateTime.Now).TotalSeconds > 1))
					Task.Delay(1000).Wait();

				else
					Task.Delay(100).Wait();
			}

			return _resultData;
		}

		private void WriteError(string errorMessage)
		{
			if ((errorMessage != null) && (string.IsNullOrWhiteSpace(errorMessage)))
				_errorMsg = "Unknown error for cash machine message (EX2001).";
			else
				_errorMsg = errorMessage;
		}

		public void Dispose()
		{
			_resultData = null;
			Command = null;
		}
	}
}
