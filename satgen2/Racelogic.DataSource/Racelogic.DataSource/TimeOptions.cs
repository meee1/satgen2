namespace Racelogic.DataSource;

public class TimeOptions : GpsDataTypeOptions
{
	private TimeUnit units = TimeUnit.HoursMinutesSeconds;

	public TimeUnit Units
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

	internal TimeOptions()
		: base(2, ToStringOptions.None)
	{
	}
}
