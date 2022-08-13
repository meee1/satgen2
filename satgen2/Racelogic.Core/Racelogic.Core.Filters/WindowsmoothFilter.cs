using System.Collections.Generic;

namespace Racelogic.Core.Filters;

public class WindowsmoothFilter : IRealTimeSmoother
{
	private readonly int smoothLevel;

	private readonly List<double> smoothingList = new List<double>();

	public int MinimumInputRequiredForFirstResult => 2 * smoothLevel + 1;

	public WindowsmoothFilter(int smoothlevel)
	{
		smoothLevel = smoothlevel;
	}

	public double? GetNextValue(double? currentValue)
	{
		if (currentValue.HasValue)
		{
			smoothingList.Add(currentValue.Value);
			while (smoothingList.Count < smoothLevel + 1)
			{
				smoothingList.Add(currentValue.Value);
			}
			if (smoothingList.Count == MinimumInputRequiredForFirstResult)
			{
				double value = SmoothData(smoothingList);
				smoothingList.RemoveAt(0);
				return value;
			}
			return null;
		}
		if (smoothingList.Count > 0)
		{
			smoothingList.Add(smoothingList[smoothingList.Count - 1]);
			double value2 = SmoothData(smoothingList);
			smoothingList.RemoveAt(0);
			return value2;
		}
		return null;
	}

	private double SmoothData(List<double> samples)
	{
		double num = 0.0;
		for (int i = -smoothLevel; i <= smoothLevel; i++)
		{
			int num2 = smoothLevel + i;
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 >= samples.Count)
			{
				num2 = samples.Count - 1;
			}
			num += samples[num2];
		}
		return num / (double)(2 * smoothLevel + 1);
	}
}
