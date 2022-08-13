namespace Racelogic.DataSource;

public struct SmoothedData : ISmoothing
{
	private uint smoothBy;

	private double total;

	private double smoothedValue;

	public double SmoothedValue => smoothedValue;

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

	public SmoothedData(ushort smoothBy)
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
			rawValue = Total / (double)SmoothBy;
			Total -= rawValue;
		}
		smoothedValue = rawValue;
		return rawValue;
	}

	public void Reset()
	{
		Total = 0.0;
		smoothedValue = 0.0;
	}
}
