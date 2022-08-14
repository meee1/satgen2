using System.Collections.Generic;
using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

public abstract class FixedLengthTrajectory : Trajectory
{
	private Pvt[]? samples;

	protected Pvt[]? Samples
	{
		[DebuggerStepThrough]
		get
		{
			return samples;
		}
		[DebuggerStepThrough]
		set
		{
			samples = value;
		}
	}

	public override IReadOnlyList<Pvt>? GetSamples(in Range<GnssTime, GnssTimeSpan> interval)
	{
		if (Samples == null)
		{
			return null;
		}
		int num = (int)((interval.Start - Interval.Start).SecondsDecimal * (decimal)base.SampleRate).SafeCeiling();
		int num2 = (int)((interval.End - Interval.Start).SecondsDecimal * (decimal)base.SampleRate).SafeFloor();
		int num3 = ((num2 >= Samples!.Length) ? (Samples!.Length - 1) : num2);
		Pvt[] array = new Pvt[num2 - num + 1];
		int num4;
		int num5;
		if (num >= 0)
		{
			num4 = num;
			num5 = 0;
		}
		else
		{
			num4 = 0;
			num5 = -num;
		}
		for (int i = num4; i <= num3; i++)
		{
			array[num5++] = Samples[i];
		}
		if (num < 0)
		{
			Pvt firstSample = Samples[0];
			Pvt secondSample = Samples[1];
			int num6;
			int num7;
			if (num2 >= 0)
			{
				num6 = -num - 1;
				num7 = -1;
			}
			else
			{
				num6 = -num + num2;
				num7 = num2;
			}
			for (int num8 = num6; num8 >= 0; num8--)
			{
				int num9 = num8;
				int sampleIndex = num7--;
				array[num9] = ExtrapolateLinear(in firstSample, in secondSample, in sampleIndex);
			}
		}
		if (num2 > num3 && num3 > 0)
		{
			Pvt firstSample2 = Samples[^2];
			Pvt secondSample2 = Samples[^1];
			int num10 = num3 - num + 1;
			int num11 = num3 - num + num2 - num3;
			int num12 = 1;
			for (int j = num10; j <= num11; j++)
			{
				int num13 = j;
				int sampleIndex = num12++;
				array[num13] = ExtrapolateLinear(in firstSample2, in secondSample2, in sampleIndex);
			}
		}
		return array;
	}
}
