using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Racelogic.DataSource;
using Racelogic.Utilities;

namespace Racelogic.Comms.Serial;

public class CanData : BasePropertyChanged
{
	private ObservableCollection<CanChannel> channels;

	private uint crcError;

	internal Dispatcher Dispatcher { get; private set; }

	public uint ChannelsBeingSentOverSerial { get; set; }

	public ObservableCollection<CanChannel> Channels
	{
		get
		{
			return channels;
		}
		set
		{
			channels = value;
			RaisePropertyChanged("Channels");
			uint num = 0u;
			foreach (CanChannel item in value.Where((CanChannel c) => c.IsBeingSentOverSerial))
			{
				num++;
			}
			ChannelsBeingSentOverSerial = num;
		}
	}

	public uint CrcError
	{
		get
		{
			return crcError;
		}
		internal set
		{
			crcError = value;
		}
	}

	internal void Clear(bool clearCrcCount)
	{
		foreach (CanChannel channel in channels)
		{
			channel.Value = 0.0;
		}
		ChannelsBeingSentOverSerial = 0u;
		if (clearCrcCount)
		{
			CrcError = 0u;
		}
	}

	public CanData Clone()
	{
		CanData canData = new CanData();
		canData.channels = new ObservableCollection<CanChannel>(channels.ToList());
		return canData;
	}

	public CanData()
	{
		Dispatcher = Dispatcher.CurrentDispatcher;
		channels = new ObservableCollection<CanChannel>();
	}
}
