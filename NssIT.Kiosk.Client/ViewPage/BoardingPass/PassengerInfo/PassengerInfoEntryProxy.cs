using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static NssIT.Kiosk.Client.ViewPage.KeyboardEventArgs;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.PassengerInfo
{
	/// <summary>
	/// ClassCode:EXIT80.22;
	/// </summary>
	public class PassengerInfoEntryProxy
	{
		private string _logChannel = "ViewPage";

		private uscKeyboard _keyboard = null;
		private TextBox _currentTextBox = null;
		private WatermarkTextEntry _nameEntry = null;
		private WatermarkTextEntry _icNoEntry = null;
		private WatermarkTextEntry _contactNoEntry = null;

		private DbLog Log { get; set; }

		public PassengerInfoEntryProxy(uscKeyboard keyboard, Dispatcher pgDispatcher, 
			Border bdPassNameWorkLocation, TextBox txtPassgNameWatermark, Grid grdPassgNameSection, TextBox txtPassgName, Border bdClearPassgName,
			Border bdICPassWorkLocation, TextBox txtICPassWatermark, Grid grdICPassSection, TextBox txtICPass, Border bdClearICPass,
			Border bdContactNoWorkLocation, TextBox txtContactNoWatermark, Grid grdContactNoSection, TextBox txtContactNo, Border bdClearContactNo)
		{
			Log = DbLog.GetDbLog();

			_nameEntry = new WatermarkTextEntry(pgDispatcher, bdPassNameWorkLocation, txtPassgNameWatermark, grdPassgNameSection, txtPassgName, bdClearPassgName);
			_icNoEntry = new WatermarkTextEntry(pgDispatcher, bdICPassWorkLocation, txtICPassWatermark, grdICPassSection, txtICPass, bdClearICPass);
			_contactNoEntry = new WatermarkTextEntry(pgDispatcher, bdContactNoWorkLocation, txtContactNoWatermark, grdContactNoSection, txtContactNo, bdClearContactNo);

			_nameEntry.OnChangeKeyboardFocusTextBox += TxtEntry_OnChangeKeyboardFocusTextBox;
			_icNoEntry.OnChangeKeyboardFocusTextBox += TxtEntry_OnChangeKeyboardFocusTextBox;
			_contactNoEntry.OnChangeKeyboardFocusTextBox += TxtEntry_OnChangeKeyboardFocusTextBox;

			_keyboard = keyboard;
			_keyboard.OnKeyPressed += _keyboard_OnKeyPressed;
		}

        private void TxtEntry_OnChangeKeyboardFocusTextBox(object sender, ChangeKeyboardFocusTextBoxEventArgs e)
        {
			_currentTextBox = e.FocusTextBox;
		}

        private void _keyboard_OnKeyPressed(object sender, KeyboardEventArgs e)
		{
			ReNewCountdown("_keyboard_OnKeyPressed");

			if (_currentTextBox == null)
				return;

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
				else if ((e.KyCat == KeyCat.Space) && (_currentTextBox.Text.Length < _currentTextBox.MaxLength))
				{
					if (_currentTextBox.Text.Length < _currentTextBox.MaxLength)
						_currentTextBox.Text += " ";
				}
			}

			FocusedTextBox(_currentTextBox);
		}

		public void LoadPage()
		{
			_nameEntry.TxtEntry.Text = "";
			_icNoEntry.TxtEntry.Text = "";
			_contactNoEntry.TxtEntry.Text = "";

			_nameEntry.ResetBorderFocusEffect();
			_icNoEntry.ResetBorderFocusEffect();
			_contactNoEntry.ResetBorderFocusEffect();

			_nameEntry.Refresh();
			_icNoEntry.Refresh();
			_contactNoEntry.Refresh();

			_nameEntry.SetFocus();
			_currentTextBox = _nameEntry.TxtEntry;
		}

		/// <summary>
		/// </summary>
		/// <param name="textBox"></param>
		/// <param name="isEnableBorderEffect">A null means don't care</param>
		public void FocusedTextBox(TextBox textBox, bool? isEnableBorderEffect = null)
		{
			if (_icNoEntry.IsEqual(textBox))
			{
				_icNoEntry.SetFocus(isEnableBorderEffect);
				_currentTextBox = textBox;
			}
			else if (_contactNoEntry.IsEqual(textBox))
			{
				_contactNoEntry.SetFocus(isEnableBorderEffect);
				_currentTextBox = textBox;
			}
			else
			{
				_nameEntry.SetFocus(isEnableBorderEffect);
				_currentTextBox = _nameEntry.TxtEntry;
			}
		}

		public void FocusAEntryTextBox()
		{
			if (_currentTextBox != null)
				FocusedTextBox(_currentTextBox);
		}

		/// <summary>
		/// FuncCode:EXIT80.2215
		/// </summary>
		private void ReNewCountdown(string tag)
		{
			try
			{
				App.CollectBoardingPassCountDown?.ReNewCountdown();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "PassengerInfoKeyBoardEntry.ReNewCountdown");
			}
		}
	}

	/// <summary>
	/// ClassCode:EXIT80.28;
	/// </summary>
	public class WatermarkTextEntry
	{
		public event EventHandler<ChangeKeyboardFocusTextBoxEventArgs> OnChangeKeyboardFocusTextBox;

		private Brush _focusBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
		private Brush _normalBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));

		private const string _logChannel = "ViewPage";

		private Dispatcher _pgDispatcher = null;
		private TextBox _txtWatermark = null;
		private Grid _grdTextEntrySection = null;
		private Border _dbClear = null;
		private Border _bdWorkLocation = null;

		public TextBox TxtEntry { get; private set; }

		//Grid grdSection
		public WatermarkTextEntry(Dispatcher pgDispatcher,
			Border bdWorkLocation, TextBox txtWatermark, Grid grdTextEntrySection, TextBox txtEntry, Border dbClear)
		{
			_pgDispatcher = pgDispatcher;
			_txtWatermark = txtWatermark;
			_grdTextEntrySection = grdTextEntrySection;
			TxtEntry = txtEntry;
			_dbClear = dbClear;
			_bdWorkLocation = bdWorkLocation;

			_txtWatermark.GotFocus += TxtWatermark_GotFocus;
			_txtWatermark.LostFocus += TxtWatermark_LostFocus;
			TxtEntry.GotFocus += TxtEntry_GotFocus;
			TxtEntry.LostFocus += TxtEntry_LostFocus;
			_dbClear.MouseLeftButtonUp += Clear_MouseLeftButtonUp;
		}

        public bool IsEqual(TextBox textBox)
		{
			return TxtEntry.Equals(textBox);
		}

        private void Clear_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			ReNewCountdown("WatermarkTextEntry.Clear_MouseLeftButtonUp");
			TxtEntry.Text = "";
			SetFocus();
		}

        private void TxtEntry_LostFocus(object sender, RoutedEventArgs e)
        {
			ReNewCountdown("WatermarkTextEntry.TxtEntry_LostFocus");
			ResetBorderFocusEffect();
			Refresh();
		}

        private void TxtEntry_GotFocus(object sender, RoutedEventArgs e)
        {
			ReNewCountdown("WatermarkTextEntry.TxtEntry_GotFocus");
			Refresh(isTextBoxCaretIndexSet: true);
			RaiseOnChangeKeyboardFocusTextBox("TxtEntry_GotFocus");
		}

        private void TxtWatermark_GotFocus(object sender, RoutedEventArgs e)
        {
			ReNewCountdown("WatermarkTextEntry.TxtWatermark_GotFocus");
			Refresh(isTextBoxCaretIndexSet: true);
			RaiseOnChangeKeyboardFocusTextBox("TxtWatermark_GotFocus");
		}

		private void TxtWatermark_LostFocus(object sender, RoutedEventArgs e)
		{
			ReNewCountdown("WatermarkTextEntry.TxtWatermark_LostFocus");
			ResetBorderFocusEffect();
			Refresh();
		}

		public void SetFocus(bool? isEnableBorderEffect = null)
		{
			if (string.IsNullOrWhiteSpace(TxtEntry.Text))
			{
                _grdTextEntrySection.Visibility = Visibility.Collapsed;
                _txtWatermark.Visibility = Visibility.Visible;
            	_txtWatermark.CaretIndex = 0;
				_txtWatermark.Focus();
			}
			else
			{
                _grdTextEntrySection.Visibility = Visibility.Visible;
                _txtWatermark.Visibility = Visibility.Collapsed;
            	TxtEntry.CaretIndex = TxtEntry.Text.Length;
				TxtEntry.Focus();
			}

			if (isEnableBorderEffect.HasValue && (isEnableBorderEffect == true))
			{
				SetBorderFocusEffect();
			}

			RaiseOnChangeKeyboardFocusTextBox("SetFocus");
		}

		public void Refresh(bool? isTextBoxCaretIndexSet = null)
		{
			if (string.IsNullOrWhiteSpace(TxtEntry.Text))
			{
				_grdTextEntrySection.Visibility = Visibility.Collapsed;
				_txtWatermark.Visibility = Visibility.Visible;

				if (isTextBoxCaretIndexSet == true)
				{
					_txtWatermark.CaretIndex = 0;
				}
			}
			else
			{
				_grdTextEntrySection.Visibility = Visibility.Visible;
				_txtWatermark.Visibility = Visibility.Collapsed;

				if (isTextBoxCaretIndexSet == true)
				{
					TxtEntry.CaretIndex = TxtEntry.Text.Length;
				}
			}
		}

		public void SetBorderFocusEffect()
		{
			_pgDispatcher.Invoke(new Action(() =>
			{
				_bdWorkLocation.BorderBrush = _focusBorderEffectColor;
				_bdWorkLocation.BorderThickness = new Thickness(5, 5, 5, 5);
			}));
		}

		public void ResetBorderFocusEffect()
		{
			_pgDispatcher.Invoke(new Action(() =>
			{
				_bdWorkLocation.BorderBrush = _normalBorderEffectColor;
				_bdWorkLocation.BorderThickness = new Thickness(1, 1, 1, 1);
			}));
		}

		private void ReNewCountdown(string tag)
		{
			try
			{
				App.CollectBoardingPassCountDown?.ReNewCountdown();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "WatermarkTextEntry.ReNewCountdown");
			}
		}

		private void RaiseOnChangeKeyboardFocusTextBox(string methodName)
		{
			try
			{
				OnChangeKeyboardFocusTextBox?.Invoke(null, new ChangeKeyboardFocusTextBoxEventArgs(TxtEntry));
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "*", ex, "EX01", $@"WatermarkTextEntry.RaiseOnChangeKeyboardFocusTextBox<-{methodName}");
			}
		}
	}

	public class ChangeKeyboardFocusTextBoxEventArgs : EventArgs
	{
		public TextBox FocusTextBox { get; private set; }

		public ChangeKeyboardFocusTextBoxEventArgs(TextBox focusTextBox)
		{
			FocusTextBox = focusTextBox;
		}
	}
}
