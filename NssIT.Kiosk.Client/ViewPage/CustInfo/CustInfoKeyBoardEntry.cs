using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static NssIT.Kiosk.Client.ViewPage.KeyboardEventArgs;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
	public class CustInfoKeyBoardEntry
	{
		private uscKeyboard _keyboard = null;

		private TextBox _currentTextBox = null;

		public CustInfoKeyBoardEntry(uscKeyboard keyboard)
		{
			_keyboard = keyboard;
			_keyboard.OnKeyPressed += _keyboard_OnKeyPressed;
		}

		private void _keyboard_OnKeyPressed(object sender, KeyboardEventArgs e)
		{
			App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

			if (e.KyCat == KeyCat.NormalChar)
			{
				if (_currentTextBox.Text.Length < _currentTextBox.MaxLength)
					_currentTextBox.Text += e.KeyString;
			}
			else
			{
				if (e.KyCat == KeyCat.Backspace)
				{
					if (_currentTextBox.Text.Length > 0)
					{
						_currentTextBox.Text = _currentTextBox.Text.Substring(0, _currentTextBox.Text.Length - 1);
					}
				}
				else if (e.KyCat == KeyCat.Enter)
				{

				}
				else if (e.KyCat == KeyCat.Space)
				{
					if (_currentTextBox.Text.Length < _currentTextBox.MaxLength)
						_currentTextBox.Text += " ";
				}
			}

			if (string.IsNullOrWhiteSpace(_currentTextBox.Text))
			{ }
			else
			{
				_currentTextBox.Focus();
				_currentTextBox.CaretIndex = _currentTextBox.Text.Length;
			}
		}

		public void FocusedTextBox(TextBox textBox)
		{
			_currentTextBox = textBox;
			_currentTextBox.Focus();

		}

		public void ResetTextBoxFocus()
		{
			_currentTextBox = null;
		}

	}
}