using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Threading;

namespace Racelogic.Utilities.Win;

[DataContract]
public abstract class BasePropertyChanged : Racelogic.Utilities.BasePropertyChanged
{
	private static readonly Dispatcher dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

	protected void OnPropertyChangedUI([CallerMemberName] string propertyName = null, InvokeMode invokeMode = InvokeMode.Automatic)
	{
		bool flag;
		switch (invokeMode)
		{
		case InvokeMode.Invoke:
		case InvokeMode.BeginInvoke:
			flag = invokeMode == InvokeMode.Invoke;
			break;
		default:
			flag = dispatcher.CheckAccess();
			break;
		}
		if (flag)
		{
			OnPropertyChanged(propertyName);
			return;
		}
		dispatcher.BeginInvoke((Action)delegate
		{
			OnPropertyChanged(propertyName);
		});
	}

	protected void RaisePropertyChangedOnUI([CallerMemberName] string propertyName = null)
	{
		if (dispatcher.CheckAccess())
		{
			OnPropertyChanged(propertyName);
			return;
		}
		dispatcher.BeginInvoke((Action)delegate
		{
			OnPropertyChanged(propertyName);
		});
	}
}
