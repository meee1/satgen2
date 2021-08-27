using System;
using MathNet.Numerics.LinearAlgebra;

namespace Racelogic.Geodetics
{
	public static class OrbitalMechanics
	{
		public static double Kepler(double eccentricAnomaly, double eccentricity)
		{
			return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
		}

		public static double InverseKepler(double meanAnomaly, double eccentricity)
		{
			double num = meanAnomaly;
			double num2 = double.MaxValue;
			int num3 = 5;
			while (num2 != 0.0 && num3 > 0)
			{
				double num4 = num + (meanAnomaly - num + eccentricity * Math.Sin(num)) / (1.0 - eccentricity * Math.Cos(num));
				num2 = num4 - num;
				num = num4;
				num3--;
			}
			return num;
		}

		public static Vector3D EcefToEci(in Vector3D ecefPosition, double earthRotation)
		{
			return EcefToEci(in ecefPosition, Math.Sin(earthRotation), Math.Cos(earthRotation));
		}

		public static Vector3D EcefToEci(in Vector3D ecefPosition, double sinEarthRotation, double cosEarthRotation)
		{
			return new Vector3D(ecefPosition.X * cosEarthRotation + ecefPosition.Y * sinEarthRotation, (0.0 - ecefPosition.X) * sinEarthRotation + ecefPosition.Y * cosEarthRotation, ecefPosition.Z);
		}

		public static Matrix<double> GetRotationMatrix(double angle, RotationAxis axis)
		{
			double num = Math.Cos(angle);
			double num2 = Math.Sin(angle);
			return axis switch
			{
				RotationAxis.X => Matrix<double>.Build.DenseOfArray(new double[3, 3]
				{
					{ 1.0, 0.0, 0.0 },
					{ 0.0, num, num2 },
					{
						0.0,
						0.0 - num2,
						num
					}
				}), 
				RotationAxis.Y => Matrix<double>.Build.DenseOfArray(new double[3, 3]
				{
					{
						num,
						0.0,
						0.0 - num2
					},
					{ 0.0, 1.0, 0.0 },
					{ num2, 0.0, num }
				}), 
				_ => Matrix<double>.Build.DenseOfArray(new double[3, 3]
				{
					{ num, num2, 0.0 },
					{
						0.0 - num2,
						num,
						0.0
					},
					{ 0.0, 0.0, 1.0 }
				}), 
			};
		}
	}
}
