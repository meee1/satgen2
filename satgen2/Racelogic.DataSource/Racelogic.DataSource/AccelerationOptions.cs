namespace Racelogic.DataSource;

public class AccelerationOptions : GpsDataTypeOptions
{
	private AccelerationUnit units;

	public AccelerationUnit Units
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

	internal AccelerationOptions()
		: base(3, ToStringOptions.AlwaysSigned)
	{
	}
}
