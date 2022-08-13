namespace Racelogic.DataSource;

public class PressureOptions : GpsDataTypeOptions
{
	private PressureUnit units;

	public PressureUnit Units
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

	internal PressureOptions()
		: base(3, ToStringOptions.ReturnAbsolute)
	{
	}
}
