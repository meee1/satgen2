using System;

namespace Racelogic.DataSource;

[Obsolete("This class is about to be removed.  Please use Datum defined in Racelogic.Geodetic.")]
public class Datum
{
	public static Datum Wgs84 => new Datum(6378137.0, 6356752.314, 299792458.0, 0.0033528106598350155, 398600500000000.0, 7.2921151467E-05);

	public double SquareRootA { get; private set; }

	public double SemiMajorAxis { get; private set; }

	public double SquareRootB { get; private set; }

	public double SemiMinorAxis { get; private set; }

	public double SpeedOfLight { get; private set; }

	public double Eccentricity { get; private set; }

	public double EccentricitySquared { get; private set; }

	public double EccentricityPrime { get; private set; }

	public double EccentricityPrimeSquared { get; private set; }

	public double Flattening { get; private set; }

	public double FlatteningReciprocal { get; private set; }

	public double GravitationalMass { get; private set; }

	public double AngularVelocity { get; private set; }

	public Datum(double semiMajorAxisSquareRoot, double semiMinorAxisSquareRoot, double speedOfLight, double flattening, double gravitationalMass, double angularVelocity)
	{
		SquareRootA = semiMajorAxisSquareRoot;
		SemiMajorAxis = SquareRootA * SquareRootA;
		SquareRootB = semiMinorAxisSquareRoot;
		SemiMinorAxis = SquareRootB * SquareRootB;
		SpeedOfLight = speedOfLight;
		Flattening = flattening;
		FlatteningReciprocal = 1.0 / Flattening;
		EccentricitySquared = (SemiMajorAxis - SemiMinorAxis) / SemiMajorAxis;
		Eccentricity = Math.Sqrt(EccentricitySquared);
		EccentricityPrimeSquared = (SemiMajorAxis - SemiMinorAxis) / SemiMajorAxis;
		EccentricityPrime = Math.Sqrt(EccentricityPrimeSquared);
		GravitationalMass = gravitationalMass;
		AngularVelocity = angularVelocity;
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
		if (other.SquareRootA.Equals(SquareRootA) && other.SemiMajorAxis.Equals(SemiMajorAxis) && other.SquareRootB.Equals(SquareRootB) && other.SemiMinorAxis.Equals(SemiMinorAxis) && other.SpeedOfLight.Equals(SpeedOfLight) && other.Eccentricity.Equals(Eccentricity) && other.EccentricitySquared.Equals(EccentricitySquared) && other.EccentricityPrime.Equals(EccentricityPrime) && other.EccentricityPrimeSquared.Equals(EccentricityPrimeSquared) && other.Flattening.Equals(Flattening) && other.FlatteningReciprocal.Equals(FlatteningReciprocal) && other.GravitationalMass.Equals(GravitationalMass))
		{
			return other.AngularVelocity.Equals(AngularVelocity);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((((((((((((((((((SquareRootA.GetHashCode() * 397) ^ SemiMajorAxis.GetHashCode()) * 397) ^ SquareRootB.GetHashCode()) * 397) ^ SemiMinorAxis.GetHashCode()) * 397) ^ SpeedOfLight.GetHashCode()) * 397) ^ Eccentricity.GetHashCode()) * 397) ^ EccentricitySquared.GetHashCode()) * 397) ^ EccentricityPrime.GetHashCode()) * 397) ^ EccentricityPrimeSquared.GetHashCode()) * 397) ^ Flattening.GetHashCode()) * 397) ^ FlatteningReciprocal.GetHashCode()) * 397) ^ GravitationalMass.GetHashCode()) * 397) ^ AngularVelocity.GetHashCode();
	}
}
