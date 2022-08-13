namespace Racelogic.DataSource;

public class AngleOptions : GpsDataTypeOptions
{
	private AngleUnit units;

	public AngleUnit Units
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

	internal AngleOptions()
		: base(3, ToStringOptions.None)
	{
	}
}
