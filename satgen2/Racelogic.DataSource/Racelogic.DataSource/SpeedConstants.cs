namespace Racelogic.DataSource;

public static class SpeedConstants
{
	public static readonly double DefaultKilometresPerHourToMilesPerHour;

	public static readonly double DefaultKilometresPerHourToKnots;

	public static readonly double DefaultKilometresPerHourToMetresPerSecond;

	public static readonly double DefaultKilometresPerHourToFeetPerSecond;

	public static readonly SpeedKilometresPerHour MinimumSpeedKilometresPerHour;

	public static readonly SpeedKilometresPerHour MinimumGpsSpeedKilometresPerHour;

	public static double KilometresPerHourToMilesPerHour { get; set; }

	public static double KilometresPerHourToKnots { get; set; }

	public static double KilometresPerHourToMetresPerSecond { get; set; }

	public static double KilometresPerHourToFeetPerSecond { get; set; }

	static SpeedConstants()
	{
		DefaultKilometresPerHourToMilesPerHour = 0.621371192237334;
		DefaultKilometresPerHourToKnots = 0.5399568034557235;
		DefaultKilometresPerHourToMetresPerSecond = 5.0 / 18.0;
		DefaultKilometresPerHourToFeetPerSecond = 0.91134441528142318;
		MinimumSpeedKilometresPerHour = 0.8;
		MinimumGpsSpeedKilometresPerHour = 0.27 / DefaultKilometresPerHourToKnots;
		Reset();
	}

	public static void Reset()
	{
		KilometresPerHourToMilesPerHour = DefaultKilometresPerHourToMilesPerHour;
		KilometresPerHourToKnots = DefaultKilometresPerHourToKnots;
		KilometresPerHourToMetresPerSecond = DefaultKilometresPerHourToMetresPerSecond;
		KilometresPerHourToFeetPerSecond = DefaultKilometresPerHourToFeetPerSecond;
	}

	public static double ConvertToDefaultSpeedUnit(double currentSpeed, SpeedUnit currentUnit)
	{
		return currentUnit switch
		{
			SpeedUnit.FeetPerSecond => currentSpeed / KilometresPerHourToFeetPerSecond, 
			SpeedUnit.Knots => currentSpeed / KilometresPerHourToKnots, 
			SpeedUnit.MetresPerSecond => currentSpeed / KilometresPerHourToMetresPerSecond, 
			SpeedUnit.MilesPerHour => currentSpeed / KilometresPerHourToMilesPerHour, 
			_ => currentSpeed, 
		};
	}

	public static double ConvertFromDefaultSpeedUnit(double currentSpeed, SpeedUnit targetUnit)
	{
		return targetUnit switch
		{
			SpeedUnit.FeetPerSecond => currentSpeed * KilometresPerHourToFeetPerSecond, 
			SpeedUnit.Knots => currentSpeed * KilometresPerHourToKnots, 
			SpeedUnit.MetresPerSecond => currentSpeed * KilometresPerHourToMetresPerSecond, 
			SpeedUnit.MilesPerHour => currentSpeed * KilometresPerHourToMilesPerHour, 
			_ => currentSpeed, 
		};
	}
}
