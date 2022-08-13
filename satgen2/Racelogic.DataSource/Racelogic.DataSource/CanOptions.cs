namespace Racelogic.DataSource;

public class CanOptions : GpsDataTypeOptions
{
	private bool vboOutput;

	public readonly string VboFormat = "0.000000E+00";

	public bool VboOutput
	{
		get
		{
			return VboOutput;
		}
		set
		{
			vboOutput = value;
		}
	}

	internal CanOptions()
		: base(6, ToStringOptions.AlwaysSigned)
	{
	}
}
