using System;
using System.Collections.ObjectModel;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public sealed class Global : BasePropertyChanged, IGlobalStores
{
	private static Global instance;

	private static ObservableCollection<SplitPoint> splits;

	private static DistanceMetres gateWidthMetres = 25.0;

	private Func<DistanceMetres, DistanceMetres, DistanceMetres> CheckValue = delegate(DistanceMetres defaultValue, DistanceMetres value)
	{
		if ((double)value == 0.0)
		{
			value = defaultValue;
		}
		return value;
	};

	public ObservableCollection<SplitPoint> Splits
	{
		get
		{
			return splits ?? (splits = new ObservableCollection<SplitPoint>());
		}
		set
		{
			splits = value;
			OnPropertyChanged("Splits");
		}
	}

	public DistanceMetres GateWidthMetres
	{
		get
		{
			return gateWidthMetres;
		}
		set
		{
			value = CheckValue(gateWidthMetres, value);
			gateWidthMetres = value;
			OnPropertyChanged("GateWidthMetres");
		}
	}

	public static IGlobalStores Instance => instance ?? (instance = new Global());

	private Global()
	{
	}
}
