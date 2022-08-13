using System.Collections.Generic;

namespace Racelogic.Core.Filters;

public static class TwoPoleButterworthFilter
{
	private static Dictionary<CutOffFrequency, TwoPoleButterworthFilterCoEfficients> Coefficients = new Dictionary<CutOffFrequency, TwoPoleButterworthFilterCoEfficients>
	{
		{
			CutOffFrequency.FifteenHz,
			new TwoPoleButterworthFilterCoEfficients(7.627390391, -0.2722149379, 0.7477891783)
		},
		{
			CutOffFrequency.TenHz,
			new TwoPoleButterworthFilterCoEfficients(14.82463775, -0.4128015981, 1.1429805025)
		},
		{
			CutOffFrequency.SevenHz,
			new TwoPoleButterworthFilterCoEfficients(27.34120269, -0.5371946248, 1.3908952814)
		}
	};

	private static TwoPoleButterworthFilterCoEfficients? coefficients = null;

	private static double[] xv = new double[3];

	private static double[] yv = new double[3];

	public static double Filter(double data)
	{
		if (!coefficients.HasValue)
		{
			return data;
		}
		xv[0] = xv[1];
		xv[1] = xv[2];
		xv[2] = data / coefficients.Value.Gain;
		yv[0] = yv[1];
		yv[1] = yv[2];
		yv[2] = xv[0] + xv[2] + 2.0 * xv[1] + coefficients.Value.CoEfficient1 * yv[0] + coefficients.Value.CoEfficient2 * yv[1];
		return yv[2];
	}

	public static void Reset(double sampleRateHz, CutOffFrequency cutOffFrequency)
	{
		if (Maths.Compare(sampleRateHz, 100.0) == ValueIs.Equal)
		{
			if (Coefficients.TryGetValue(cutOffFrequency, out var value))
			{
				coefficients = value;
			}
			else
			{
				coefficients = null;
			}
		}
		else
		{
			coefficients = null;
		}
		for (int i = 0; i < xv.Length; i++)
		{
			xv[i] = 0.0;
			yv[i] = 0.0;
		}
	}

	public static List<double> Filter(List<double> data, double sampleRateHz, CutOffFrequency cutOffFrequency)
	{
		List<double> list = new List<double>(data.Count);
		Reset(sampleRateHz, cutOffFrequency);
		foreach (double datum in data)
		{
			list.Add(Filter(datum));
		}
		return list;
	}
}
