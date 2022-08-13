namespace Racelogic.DataSource;

public struct FirmwareSmoothedData : ISmoothing
{
	private uint smoothBy;

	private double total;

	private double smoothedValue;

	public double SmoothedValue
	{
		get
		{
			return smoothedValue;
		}
		private set
		{
			smoothedValue = value;
		}
	}

	public uint SmoothBy
	{
		get
		{
			return smoothBy;
		}
		set
		{
			if (value != smoothBy)
			{
				smoothBy = value;
				Reset();
			}
		}
	}

	private double Total
	{
		get
		{
			return total;
		}
		set
		{
			total = value;
		}
	}

	public FirmwareSmoothedData(uint smoothBy)
	{
		this.smoothBy = smoothBy;
		total = 0.0;
		smoothedValue = 0.0;
	}

	public double GetSmoothedValue(double rawValue)
	{
		if (SmoothBy != 0)
		{
			Total += rawValue;
			SmoothedValue = Total / (double)SmoothBy;
			Total -= SmoothedValue;
		}
		else
		{
			SmoothedValue = rawValue;
		}
		return SmoothedValue;
	}

	public void Reset()
	{
		Total = 0.0;
		smoothedValue = 0.0;
	}
}
