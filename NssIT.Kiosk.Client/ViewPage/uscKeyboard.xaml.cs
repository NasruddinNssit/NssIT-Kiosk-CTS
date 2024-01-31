using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace NssIT.Kiosk.Client.ViewPage
{
    /// <summary>
    /// Interaction logic for uscKeyboard.xaml
    /// </summary>
    public partial class uscKeyboard : UserControl
    {
		private const string LogChannel = "ViewPage";

		public event EventHandler<KeyboardEventArgs> OnKeyPressed;

		public uscKeyboard()
        {
            InitializeComponent();
        }

		private void Key_Pressed(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is Button butt)
			{
				if (butt.Content is string chr)
				{
					if (chr.Equals("SPACE"))
						RaiseOnKeyPressed(" ");
					else
						RaiseOnKeyPressed(chr);
				}
				else if (butt.Content is Image)
				{
					if ((butt.Tag is string bTag) && (bTag.Equals("Keyboard")))
					{
						// not function at the moment.
					}
					else if ((butt.Tag is string cTag) && (cTag.Equals("Backspace")))
					{
						RaiseOnKeyPressed(KeyboardEventArgs.KeyCat.Backspace);
					}
					else if ((butt.Tag is string dTag) && (dTag.Equals("Enter")))
					{
						RaiseOnKeyPressed(KeyboardEventArgs.KeyCat.Enter);
					}
				}
			}

		}

		private void RaiseOnKeyPressed(string chrStr)
		{
			try
			{
				if (OnKeyPressed != null)
				{
					OnKeyPressed.Invoke(this, new KeyboardEventArgs(chrStr));
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "", ex, "EX01", "uscKeyboard.RaiseOnKeyPressed");
			}
		}

		private void RaiseOnKeyPressed(KeyboardEventArgs.KeyCat kyCat)
		{
			try
			{
				if (OnKeyPressed != null)
				{
					OnKeyPressed.Invoke(this, new KeyboardEventArgs(kyCat));
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "", ex, "EX01", "uscKeyboard.RaiseOnKeyPressed");
			}
		}
	}
}