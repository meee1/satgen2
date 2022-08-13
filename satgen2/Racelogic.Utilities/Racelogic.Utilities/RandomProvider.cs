using System;
using System.Threading;

namespace Racelogic.Utilities;

public static class RandomProvider
{
	private static int seed = Environment.TickCount;

	private static readonly ThreadLocal<Random> RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

	private const double TwoPi = Math.PI * 2.0;

	public static Random ThreadRandom => RandomWrapper.Value;

	public static double GaussianNoise()
	{
		double num = ThreadRandom.NextDouble();
		if (num < 1E-100)
		{
			num = 1E-100;
		}
		double num2 = ThreadRandom.NextDouble();
		double num3 = Math.Log(num);
		return Math.Sqrt(0.0 - num3 - num3) * Math.Cos(Math.PI * 2.0 * num2);
	}
}
