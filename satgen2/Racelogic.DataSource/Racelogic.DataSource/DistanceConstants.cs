namespace Racelogic.DataSource;

public static class DistanceConstants
{
	public static readonly double DefaultMetresToFeet;

	public static readonly double DefaultMetresToMiles;

	public static readonly double DefaultMetresToNauticalMiles;

	public static double MetresToFeet { get; set; }

	public static double MetresToMiles { get; set; }

	public static double MetresToNauticalMiles { get; set; }

	static DistanceConstants()
	{
		DefaultMetresToFeet = 3.280839895;
		DefaultMetresToMiles = 0.000621371192237334;
		DefaultMetresToNauticalMiles = 0.00053995680345572347;
		Reset();
	}

	public static void Reset()
	{
		MetresToFeet = DefaultMetresToFeet;
		MetresToMiles = DefaultMetresToMiles;
		MetresToNauticalMiles = DefaultMetresToNauticalMiles;
	}

	public static double ConvertFromDefaultDistanceUnit(double currentDistance, DistanceUnit targetUnit)
	{
		return targetUnit switch
		{
			DistanceUnit.Feet => currentDistance * MetresToFeet, 
			DistanceUnit.Miles => currentDistance * MetresToMiles, 
			DistanceUnit.NauticalMiles => currentDistance * MetresToNauticalMiles, 
			DistanceUnit.Kilometres => currentDistance / 1000.0, 
			_ => currentDistance, 
		};
	}

	public static double ConvertToDefaultDistanceUnit(double currentDistance, DistanceUnit currentUnit)
	{
		return currentUnit switch
		{
			DistanceUnit.Feet => currentDistance / MetresToFeet, 
			DistanceUnit.Miles => currentDistance / MetresToMiles, 
			DistanceUnit.NauticalMiles => currentDistance / MetresToNauticalMiles, 
			DistanceUnit.Kilometres => currentDistance * 1000.0, 
			_ => currentDistance, 
		};
	}
}
