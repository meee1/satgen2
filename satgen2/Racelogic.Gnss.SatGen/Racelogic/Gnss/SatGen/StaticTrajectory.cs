using System.Collections.Generic;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	public sealed class StaticTrajectory : Trajectory
	{
		private readonly Ecef ecef;

		public StaticTrajectory(in GnssTime startTime, in GnssTimeSpan duration, in Geodetic location, GravitationalModel gravitationalModel = GravitationalModel.Wgs84)
		{
			base.SampleRate = 1;
			ecef = location.ToEcef(Datum.WGS84, Geoid.FromGravitationalModel(gravitationalModel));
			Interval = new Range<GnssTime, GnssTimeSpan>(startTime, startTime + duration);
		}

		public sealed override IReadOnlyList<Pvt>? GetSamples(in Range<GnssTime, GnssTimeSpan> interval)
		{
			int num = (int)((interval.Start - Interval.Start).SecondsDecimal * (decimal)base.SampleRate).SafeCeiling();
			int num2 = (int)((interval.End - Interval.Start).SecondsDecimal * (decimal)base.SampleRate).SafeFloor();
			int num3 = num2 - num + 1;
			GnssTime gnssTime3 = Interval.Start + num * base.SampleSpan;
			GnssTime gnssTime2 = Interval.Start + num2 * base.SampleSpan;
			Pvt[] array = new Pvt[num3];
			int num4 = 0;
			for (GnssTime time = gnssTime3; time <= gnssTime2; time += base.SampleSpan)
			{
				array[num4++] = new Pvt(in time, in ecef);
			}
			return array;
		}
	}
}
