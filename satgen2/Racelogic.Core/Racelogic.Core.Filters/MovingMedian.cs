using System;
using System.Collections.Generic;

namespace Racelogic.Core.Filters;

public class MovingMedian : IRealTimeSmoother
{
	private int count;

	private int median;

	private List<double> vals;

	public MovingMedian(int count)
	{
		this.count = count;
		vals = new List<double>(count);
		median = (int)Math.Ceiling((double)count / 2.0);
	}

	public double? GetNextValue(double? value)
	{
		if (vals.Count == count)
		{
			vals.RemoveAt(0);
		}
		if (value.HasValue)
		{
			vals.Add(value.Value);
		}
		if (vals.Count == count)
		{
			List<double> list = new List<double>(vals);
			list.Sort();
			return list[median];
		}
		return null;
	}
}
