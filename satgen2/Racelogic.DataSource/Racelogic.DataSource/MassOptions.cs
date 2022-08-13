namespace Racelogic.DataSource;

public class MassOptions : GpsDataTypeOptions
{
	private MassUnit units;

	public MassUnit Units
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

	internal MassOptions()
		: base(3, ToStringOptions.ReturnAbsolute)
	{
	}
}
