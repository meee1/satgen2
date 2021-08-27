using System;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	internal static class EarthRotation
	{
		public static (double Pseudorange, double DopplerVelocity, Vector3D LineOfSight) SagnacCorrection(SatelliteBase sat, in Pvt observer, in double trueRange)
		{
			double num = trueRange;
			double sinEarthRotation;
			double cosEarthRotation;
			Ecef ecef;
			Vector3D item;
			double num3;
			do
			{
				double num2 = trueRange * 3.3356409519815204E-09;
				double num4 = sat.Datum.AngularVelocity * num2;
				sinEarthRotation = Math.Sin(num4);
				cosEarthRotation = Math.Cos(num4);
				GnssTime time = observer.Time - GnssTimeSpan.FromSeconds(num2);
				ecef = sat.GetEcef(in time, out var _);
				item = OrbitalMechanics.EcefToEci(in ecef.Position, sinEarthRotation, cosEarthRotation) - observer.Ecef.Position;
				double num5 = item.Magnitude();
				num3 = Math.Abs(num5 - num);
				num = num5;
			}
			while (num3 > 1E-06);
			Vector3D vector3D = OrbitalMechanics.EcefToEci(in ecef.Velocity, sinEarthRotation, cosEarthRotation);
			Vector3D vector3D2 = item.Normalize(num);
			Vector3D right = vector3D - observer.Ecef.Velocity;
			double item2 = 0.0 - vector3D2.DotProduct(in right);
			return (num, item2, item);
		}
	}
}
