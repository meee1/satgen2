namespace Racelogic.DataSource;

public class DistanceOptions : GpsDataTypeOptions
{
	private DistanceUnit units;

	public DistanceUnit Units
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

	internal DistanceOptions()
		: base(2, ToStringOptions.None)
	{
	}
}
