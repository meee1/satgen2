using System;

namespace Racelogic.Geodetics;

public sealed class Datum
{
	public static readonly Datum WGS84 = new Datum(6378137.0, 298.257223563, 398600500000000.0, 7.2921151467E-05, 3.1415926535898);

	public static readonly Datum PZ90 = new Datum(6378136.0, 298.257839303, 398600441800000.0, 7.2921151467064E-05, Math.PI);

	public static readonly Datum CGCS2000 = new Datum(6378137.0, 298.257222101, 398600441800000.0, 7.292115E-05, 3.1415926535898);

	public static readonly Datum GTRF = new Datum(6378137.0, 298.257222101, 398600441800000.0, 7.2921151467E-05, 3.1415926535898);

	public const double SpeedOfLight = 299792458.0;

	public const double SpeedOfLightInv = 3.3356409519815204E-09;

	public const decimal SpeedOfLightInvDecimal = 0.0000000033356409519815204958m;

	public readonly double GM;

	public readonly double SqrtGM;

	public readonly double AngularVelocity;

	public readonly double PI;

	public readonly double FirstEccentricitySquared;

	public readonly double Flattening;

	public readonly double SemiMajorAxis;

	public readonly double SemiMinorAxis;

	public Datum(double semiMajorAxis, double flatteningReciprocal, double gm, double angularVelocity, double pi)
	{
		SemiMajorAxis = semiMajorAxis;
		GM = gm;
		AngularVelocity = angularVelocity;
		PI = pi;
		SqrtGM = Math.Sqrt(gm);
		Flattening = 1.0 / flatteningReciprocal;
		SemiMinorAxis = semiMajorAxis * (flatteningReciprocal - 1.0) / flatteningReciprocal;
		FirstEccentricitySquared = (2.0 - 1.0 / flatteningReciprocal) / flatteningReciprocal;
	}

	public static bool operator ==(Datum left, Datum right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(Datum left, Datum right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(Datum))
		{
			return false;
		}
		return Equals((Datum)obj);
	}

	public bool Equals(Datum other)
	{
		if ((object)other != null)
		{
			double semiMajorAxis = other.SemiMajorAxis;
			if (semiMajorAxis.Equals(SemiMajorAxis))
			{
				semiMajorAxis = other.Flattening;
				if (semiMajorAxis.Equals(Flattening))
				{
					semiMajorAxis = other.GM;
					if (semiMajorAxis.Equals(GM))
					{
						semiMajorAxis = other.AngularVelocity;
						if (semiMajorAxis.Equals(AngularVelocity))
						{
							semiMajorAxis = other.PI;
							return semiMajorAxis.Equals(PI);
						}
					}
				}
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		double semiMajorAxis = SemiMajorAxis;
		int num = (5993773 + semiMajorAxis.GetHashCode()) * 9973;
		semiMajorAxis = Flattening;
		int num2 = (num + semiMajorAxis.GetHashCode()) * 9973;
		semiMajorAxis = GM;
		int num3 = (num2 + semiMajorAxis.GetHashCode()) * 9973;
		semiMajorAxis = AngularVelocity;
		int num4 = (num3 + semiMajorAxis.GetHashCode()) * 9973;
		semiMajorAxis = PI;
		return num4 + semiMajorAxis.GetHashCode();
	}
}
