using System.Collections.Generic;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	public class EcefTrajectory : FixedLengthTrajectory
	{
		public EcefTrajectory(in GnssTime startTime, in int sampleRate, IReadOnlyList<Ecef> ecefs, in bool isVelocityPresent = true)
		{
			base.SampleRate = sampleRate;
			base.Samples = new Pvt[ecefs.Count];
			if (isVelocityPresent)
			{
				int num = 0;
				GnssTime time = startTime;
				foreach (Ecef ecef4 in ecefs)
				{
					Ecef ecef = ecef4;
					base.Samples[num] = new Pvt(in time, in ecef);
					time += base.SampleSpan;
					num++;
				}
			}
			else
			{
				int count = ecefs.Count;
				double[] array = new double[count];
				double[] array2 = new double[count];
				double[] array3 = new double[count];
				double[] array4 = new double[count];
				for (int i = 0; i < ecefs.Count; i++)
				{
					array[i] = (double)((decimal)i * base.SamplePeriod);
					Vector3D position = ecefs[i].Position;
					array2[i] = position.X;
					array3[i] = position.Y;
					array4[i] = position.Z;
				}
				AkimaSpline akimaSpline = new AkimaSpline(array, array2, isConstantRate: true);
				AkimaSpline akimaSpline2 = new AkimaSpline(array, array3, isConstantRate: true);
				AkimaSpline akimaSpline3 = new AkimaSpline(array, array4, isConstantRate: true);
				GnssTime time2 = startTime;
				int num2 = count - 1;
				base.Samples = new Pvt[count];
				for (int j = 0; j < ecefs.Count; j++)
				{
					Ecef ecef2 = ecefs[j];
					if (j < num2)
					{
						double position2 = array[j];
						double x = akimaSpline.Differentiate(position2);
						double y = akimaSpline2.Differentiate(position2);
						double z = akimaSpline3.Differentiate(position2);
						Vector3D velocity = new Vector3D(x, y, z);
						ecef2 = new Ecef(in ecef2.Position, in velocity);
					}
					base.Samples[j] = new Pvt(in time2, in ecef2);
					time2 += base.SampleSpan;
				}
				Pvt pvt = base.Samples[^1];
				Vector3D position3 = pvt.Ecef.Position;
				Vector3D position4 = base.Samples[^2].Ecef.Position;
				Vector3D velocity2 = base.SampleRate * (position3 - position4);
				Pvt[]? array5 = base.Samples;
				int num3 = array5!.Length - 1;
				ref readonly GnssTime time3 = ref pvt.Time;
				Ecef ecef3 = new Ecef(in position3, in velocity2);
				array5[num3] = new Pvt(in time3, in ecef3);
			}
			base.Interval = new Range<GnssTime, GnssTimeSpan>(base.Samples[0].Time, base.Samples[^1].Time);
		}
	}
}
