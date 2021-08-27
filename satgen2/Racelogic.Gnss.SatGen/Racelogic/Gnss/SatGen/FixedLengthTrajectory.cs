using System.Collections.Generic;
using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
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
			int num4 = (int)((interval.End - Interval.Start).SecondsDecimal * (decimal)base.SampleRate).SafeFloor();
			int num5 = ((num4 >= Samples!.Length) ? (Samples!.Length - 1) : num4);
			Pvt[] array = new Pvt[num4 - num + 1];
			int num6;
			int num7;
			if (num >= 0)
			{
				num6 = num;
				num7 = 0;
			}
			else
			{
				num6 = 0;
				num7 = -num;
			}
			for (int i = num6; i <= num5; i++)
			{
				array[num7++] = Samples[i];
			}
			if (num < 0)
			{
				Pvt firstSample = Samples[0];
				Pvt secondSample = Samples[1];
				int num8;
				int num9;
				if (num4 >= 0)
				{
					num8 = -num - 1;
					num9 = -1;
				}
				else
				{
					num8 = -num + num4;
					num9 = num4;
				}
				for (int num10 = num8; num10 >= 0; num10--)
				{
					int num11 = num10;
					int sampleIndex = num9--;
					array[num11] = ExtrapolateLinear(in firstSample, in secondSample, in sampleIndex);
				}
			}
			if (num4 > num5 && num5 > 0)
			{
				Pvt firstSample2 = Samples[^2];
				Pvt secondSample2 = Samples[^1];
				int num12 = num5 - num + 1;
				int num2 = num5 - num + num4 - num5;
				int num3 = 1;
				for (int j = num12; j <= num2; j++)
				{
					int num13 = j;
					int sampleIndex = num3++;
					array[num13] = ExtrapolateLinear(in firstSample2, in secondSample2, in sampleIndex);
				}
			}
			return array;
		}
	}
}
