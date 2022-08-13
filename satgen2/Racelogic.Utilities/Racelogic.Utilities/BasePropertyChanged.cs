using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Racelogic.Utilities;

[DataContract]
public abstract class BasePropertyChanged : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null, bool beginInvoke = false)
	{
		if (beginInvoke)
		{
			this.PropertyChanged?.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), null, null);
		}
		else
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
	{
		OnPropertyChanged(propertyName);
	}

	protected void ClearPropertyChanged()
	{
		this.PropertyChanged = null;
	}
}
