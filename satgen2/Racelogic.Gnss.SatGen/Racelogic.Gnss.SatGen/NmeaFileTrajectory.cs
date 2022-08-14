using System;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Libraries.Nmea;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

public class NmeaFileTrajectory : FixedLengthTrajectory
{
	public NmeaFileTrajectory(in GnssTime startTime, string nmeaFileName, GravitationalModel gravitationalModel)
	{
		using (NmeaFile nmeaFile = new NmeaFile(nmeaFileName))
		{
			if (double.IsNaN(nmeaFile.SamplePeriod) || nmeaFile.SamplePeriod == 0.0 || nmeaFile.SamplePeriod > 1.0)
			{
				base.ErrorMessage = "Sampling rate of the NMEA file cannot be determined.";
				return;
			}
			int num = (int)Math.Round(1.0 / nmeaFile.SamplePeriod);
			if (num == 0 || !nmeaFile.SamplePeriod.AlmostEquals(1.0 / (double)num))
			{
				base.ErrorMessage = "Sampling rate of the NMEA file is not a whole number.";
				return;
			}
			if (nmeaFile.Length + 1 < 5)
			{
				base.ErrorMessage = $"Trajectory must contain at least {5} points.\nEither increase the sample rate or make the simulation longer.";
				return;
			}
			base.SampleRate = num;
			int num2 = 0;
			int length = nmeaFile.Length;
			Ecef[] array = new Ecef[length];
			foreach (GpsSample sample in nmeaFile.Samples)
			{
				array[num2++] = Ecef.FromCoordinates(sample.Latitude.DecimalDegrees, sample.Longitude.DecimalDegrees, sample.Height, sample.GeoidHeight, gravitationalModel);
			}
			double[] array2 = new double[length];
			double[] array3 = new double[length];
			double[] array4 = new double[length];
			double[] array5 = new double[length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = (double)((decimal)i * base.SamplePeriod);
				Vector3D position = array[i].Position;
				array3[i] = position.X;
				array4[i] = position.Y;
				array5[i] = position.Z;
			}
			AkimaSpline akimaSpline = new AkimaSpline(array2, array3, isConstantRate: true);
			AkimaSpline akimaSpline2 = new AkimaSpline(array2, array4, isConstantRate: true);
			AkimaSpline akimaSpline3 = new AkimaSpline(array2, array5, isConstantRate: true);
			GnssTime time = startTime;
			int num3 = length - 1;
			base.Samples = new Pvt[length];
			for (int j = 0; j < array.Length; j++)
			{
				Ecef ecef = array[j];
				if (j < num3)
				{
					double position2 = array2[j];
					double x = akimaSpline.Differentiate(position2);
					double y = akimaSpline2.Differentiate(position2);
					double z = akimaSpline3.Differentiate(position2);
					Vector3D velocity = new Vector3D(x, y, z);
					ecef = new Ecef(in ecef.Position, in velocity);
				}
				base.Samples[j] = new Pvt(in time, in ecef);
				time += base.SampleSpan;
			}
		}
		Pvt pvt = base.Samples[^1];
		Vector3D position3 = pvt.Ecef.Position;
		Vector3D position4 = base.Samples[^2].Ecef.Position;
		Vector3D velocity2 = base.SampleRate * (position3 - position4);
		Pvt[]? array6 = base.Samples;
		int num4 = array6!.Length - 1;
		ref readonly GnssTime time2 = ref pvt.Time;
		Ecef ecef2 = new Ecef(in position3, in velocity2);
		array6[num4] = new Pvt(in time2, in ecef2);
		base.Interval = new Range<GnssTime, GnssTimeSpan>(base.Samples[0].Time, base.Samples[^1].Time);
	}
}
