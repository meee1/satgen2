namespace Racelogic.DataSource;

public class TemperatureOptions : GpsDataTypeOptions
{
	private TemperatureUnit units;

	public TemperatureUnit Units
	{
		get
		{
			return units;
		}
		set
		{
			if (units != value)
			{
				units = value;
				OnPropertyChanged("Units");
			}
		}
	}

	internal TemperatureOptions()
		: base(3, ToStringOptions.None)
	{
	}
}
