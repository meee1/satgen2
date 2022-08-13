namespace Racelogic.Core.Filters;

public class SmoothedMovingAverage : IRealTimeSmoother
{
	private int count;

	private int addedVals;

	private double total;

	public SmoothedMovingAverage(int TotalSampleSize)
	{
		count = TotalSampleSize;
	}

	public double? GetNextValue(double? value)
	{
		if (value.HasValue)
		{
			total += value.Value;
			addedVals++;
			if (addedVals < count)
			{
				return null;
			}
			double num = total / (double)count;
			total -= num;
			return num;
		}
		return null;
	}
}
