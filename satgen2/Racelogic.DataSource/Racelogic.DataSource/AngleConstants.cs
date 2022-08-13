using System;

namespace Racelogic.DataSource;

public static class AngleConstants
{
	public static readonly double DefaultDegreeToRadians;

	public static double DegreeToRadians { get; set; }

	static AngleConstants()
	{
		DefaultDegreeToRadians = Math.PI / 180.0;
		Reset();
	}

	public static void Reset()
	{
		DegreeToRadians = DefaultDegreeToRadians;
	}

	public static double ConvertToDefaultAngleUnit(double currentAngle, AngleUnit currentUnit)
	{
		if (currentUnit == AngleUnit.Radians)
		{
			return currentAngle / DegreeToRadians;
		}
		return currentAngle;
	}

	public static double ConvertFromDefaultAngleUnit(double currentAngle, AngleUnit targetUnit)
	{
		if (targetUnit == AngleUnit.Radians)
		{
			return currentAngle * DegreeToRadians;
		}
		return currentAngle;
	}
}
