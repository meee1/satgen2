using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Racelogic.Utilities.Win;

public static class Extensions
{
	public static System.Windows.Media.Color ToWpfColour(this System.Drawing.Color originalColour)
	{
		return System.Windows.Media.Color.FromArgb(originalColour.A, originalColour.R, originalColour.G, originalColour.B);
	}

	public static System.Drawing.Color ToWinFormsColour(this System.Windows.Media.Color originalColour)
	{
		return System.Drawing.Color.FromArgb(originalColour.A, originalColour.R, originalColour.G, originalColour.B);
	}

	public static void RaiseOnUI(this PropertyChangedEventHandler handler, object sender, [CallerMemberName] string propertyName = null, InvokeMode invokeMode = InvokeMode.Automatic)
	{
		if (handler == null)
		{
			return;
		}
		switch (invokeMode)
		{
		case InvokeMode.Invoke:
			Application.Current.Dispatcher.Invoke(delegate
			{
				handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
			});
			return;
		case InvokeMode.BeginInvoke:
			Application.Current.Dispatcher.Invoke(() => handler?.BeginInvoke(sender, new PropertyChangedEventArgs(propertyName), null, null));
			return;
		}
		if (Application.Current.Dispatcher.CheckAccess())
		{
			handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
			return;
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
		});
	}
}
