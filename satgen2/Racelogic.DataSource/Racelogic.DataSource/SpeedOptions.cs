namespace Racelogic.DataSource;

public class SpeedOptions : GpsDataTypeOptions
{
	private SpeedUnit units;

	public SpeedUnit Units
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

	internal SpeedOptions()
		: base(3, ToStringOptions.None)
	{
	}
}
