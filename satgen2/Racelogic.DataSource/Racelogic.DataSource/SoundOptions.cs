namespace Racelogic.DataSource;

public class SoundOptions : GpsDataTypeOptions
{
	private SoundUnit units;

	public SoundUnit Units
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

	internal SoundOptions()
		: base(1, ToStringOptions.ReturnAbsolute)
	{
	}
}
