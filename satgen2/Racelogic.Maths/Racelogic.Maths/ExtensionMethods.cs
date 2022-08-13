using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Maths;

public static class ExtensionMethods
{
	public static double PopulationVariance(this IEnumerable<double> values)
	{
		return Variance(values, isSample: false);
	}

	public static double SampleVariance(this IEnumerable<double> values)
	{
		return Variance(values, isSample: true);
	}

	private static double Variance(IEnumerable<double> values, bool isSample)
	{
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		foreach (double value in values)
		{
			num++;
			double num4 = value - num2;
			num2 += num4 / (double)num;
			num3 += num4 * (value - num2);
		}
		return num3 / (double)(isSample ? (num - 1) : num);
	}

	public static double Slope(this IEnumerable<Point> values)
	{
		List<Point> list = values.ToList();
		if (list.Count == 0)
		{
			return double.NaN;
		}
		double yAvg = values.Select((Point c) => c.Y).Average();
		double xAvg = values.Select((Point c) => c.X).Average();
		double num = list.Sum((Point c) => (c.X - xAvg) * (c.Y - yAvg));
		double num2 = list.Sum((Point c) => (c.X - xAvg) * (c.X - xAvg));
		if (num2 == 0.0)
		{
			return double.NaN;
		}
		return num / num2;
	}

	public static double Intercept(this IEnumerable<Point> values)
	{
		if (values.ToList().Count == 0)
		{
			return double.NaN;
		}
		double num = values.Select((Point c) => c.Y).Average();
		double num2 = values.Select((Point c) => c.X).Average();
		double num3 = values.Slope();
		if (!double.IsNaN(num3))
		{
			return num - num3 * num2;
		}
		return double.NaN;
	}
}
