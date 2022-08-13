namespace Racelogic.DataSource;

public class LatLongOptions : GpsDataTypeOptions
{
	private LatLongUnit units;

	public LatLongUnit Units
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

	internal LatLongOptions()
		: base(6, ToStringOptions.AlwaysSigned)
	{
	}
}
