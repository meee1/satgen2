namespace Racelogic.DataSource;

public static class GpsUnitsConverter
{
	public static double SpeedKmhToUserUnits(double value, SpeedUnit units)
	{
		switch (units)
		{
		case SpeedUnit.FeetPerSecond:
			value *= SpeedConstants.KilometresPerHourToFeetPerSecond;
			break;
		case SpeedUnit.Knots:
			value *= SpeedConstants.KilometresPerHourToKnots;
			break;
		case SpeedUnit.MetresPerSecond:
			value *= SpeedConstants.KilometresPerHourToMetresPerSecond;
			break;
		case SpeedUnit.MilesPerHour:
			value *= SpeedConstants.KilometresPerHourToMilesPerHour;
			break;
		}
		return value;
	}

	public static double SpeedUserUnitsToKmh(double value, SpeedUnit units)
	{
		switch (units)
		{
		case SpeedUnit.FeetPerSecond:
			value /= SpeedConstants.KilometresPerHourToFeetPerSecond;
			break;
		case SpeedUnit.Knots:
			value /= SpeedConstants.KilometresPerHourToKnots;
			break;
		case SpeedUnit.MetresPerSecond:
			value /= SpeedConstants.KilometresPerHourToMetresPerSecond;
			break;
		case SpeedUnit.MilesPerHour:
			value /= SpeedConstants.KilometresPerHourToMilesPerHour;
			break;
		}
		return value;
	}

	public static double SpeedKmhToUserUnitsScale(SpeedUnit units)
	{
		double result = 1.0;
		switch (units)
		{
		case SpeedUnit.FeetPerSecond:
			result = SpeedConstants.KilometresPerHourToFeetPerSecond;
			break;
		case SpeedUnit.Knots:
			result = SpeedConstants.KilometresPerHourToKnots;
			break;
		case SpeedUnit.MetresPerSecond:
			result = SpeedConstants.KilometresPerHourToMetresPerSecond;
			break;
		case SpeedUnit.MilesPerHour:
			result = SpeedConstants.KilometresPerHourToMilesPerHour;
			break;
		}
		return result;
	}

	public static double DistanceMetresToUserUnits(double value, DistanceUnit units)
	{
		switch (units)
		{
		case DistanceUnit.Feet:
			value *= DistanceConstants.MetresToFeet;
			break;
		case DistanceUnit.Kilometres:
			value /= 1000.0;
			break;
		case DistanceUnit.Miles:
			value *= DistanceConstants.MetresToMiles;
			break;
		case DistanceUnit.NauticalMiles:
			value *= DistanceConstants.MetresToNauticalMiles;
			break;
		}
		return value;
	}

	public static double DistanceUserUnitsToMetres(double value, DistanceUnit units)
	{
		switch (units)
		{
		case DistanceUnit.Feet:
			value /= DistanceConstants.MetresToFeet;
			break;
		case DistanceUnit.Kilometres:
			value *= 1000.0;
			break;
		case DistanceUnit.Miles:
			value /= DistanceConstants.MetresToMiles;
			break;
		case DistanceUnit.NauticalMiles:
			value /= DistanceConstants.MetresToNauticalMiles;
			break;
		}
		return value;
	}

	public static double PressurePsiToUserUnits(double value, PressureUnit units)
	{
		switch (units)
		{
		case PressureUnit.Atmosphere:
			value *= PressureConstants.PsiToAtmosphere;
			break;
		case PressureUnit.Bar:
			value *= PressureConstants.PsiToBar;
			break;
		case PressureUnit.KiloPascal:
			value *= PressureConstants.PsiToKiloPascal;
			break;
		}
		return value;
	}

	public static double PressureUserUnitsToPsi(double value, PressureUnit units)
	{
		switch (units)
		{
		case PressureUnit.Atmosphere:
			value /= PressureConstants.PsiToAtmosphere;
			break;
		case PressureUnit.Bar:
			value /= PressureConstants.PsiToBar;
			break;
		case PressureUnit.KiloPascal:
			value /= PressureConstants.PsiToKiloPascal;
			break;
		}
		return value;
	}

	public static double PressurePsiToUserUnitsScale(PressureUnit units)
	{
		double result = 1.0;
		switch (units)
		{
		case PressureUnit.Atmosphere:
			result = PressureConstants.PsiToAtmosphere;
			break;
		case PressureUnit.Bar:
			result = PressureConstants.PsiToBar;
			break;
		case PressureUnit.KiloPascal:
			result = PressureConstants.PsiToKiloPascal;
			break;
		}
		return result;
	}

	public static double AccelerationGToUserUnits(double value, AccelerationUnit units)
	{
		switch (units)
		{
		case AccelerationUnit.FeetPerSecondSquared:
			value *= AccelerationConstants.GToFeetPerSecondSquared;
			break;
		case AccelerationUnit.MetresPerSecondSquared:
			value *= AccelerationConstants.GToMetresPerSecondSquared;
			break;
		}
		return value;
	}

	public static double AccelerationUserUnitsToG(double value, AccelerationUnit units)
	{
		switch (units)
		{
		case AccelerationUnit.FeetPerSecondSquared:
			value /= AccelerationConstants.GToFeetPerSecondSquared;
			break;
		case AccelerationUnit.MetresPerSecondSquared:
			value /= AccelerationConstants.GToMetresPerSecondSquared;
			break;
		}
		return value;
	}

	public static double AccelerationGToUserUnitsScale(AccelerationUnit units)
	{
		double result = 1.0;
		switch (units)
		{
		case AccelerationUnit.FeetPerSecondSquared:
			result = AccelerationConstants.GToFeetPerSecondSquared;
			break;
		case AccelerationUnit.MetresPerSecondSquared:
			result = AccelerationConstants.GToMetresPerSecondSquared;
			break;
		}
		return result;
	}

	public static double MassKilogramToUserUnits(double value, MassUnit units)
	{
		return MassConstants.ConvertFromDefaultMassUnit(value, units);
	}

	public static double MassUserUnitsToKilogram(double value, MassUnit units)
	{
		return MassConstants.ConvertToDefaultMassUnit(value, units);
	}

	public static double MassKilogramToUserUnitsScale(MassUnit units)
	{
		return MassConstants.ConvertFromDefaultMassUnit(1.0, units);
	}
}
