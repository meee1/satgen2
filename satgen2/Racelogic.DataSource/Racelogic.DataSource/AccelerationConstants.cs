namespace Racelogic.DataSource;

public static class AccelerationConstants
{
	public static readonly double DefaultGToMetresPerSecondSquared;

	public static readonly double DefaultGToFeetPerSecondSquared;

	public static readonly double DefaultMeteresPerSecondSquaredToFeetPerSecondSquared;

	public static double GToMetresPerSecondSquared { get; set; }

	public static double GToFeetPerSecondSquared { get; set; }

	public static double MeteresPerSecondSquaredToFeetPerSecondSquared { get; set; }

	static AccelerationConstants()
	{
		DefaultGToMetresPerSecondSquared = 9.80665;
		DefaultGToFeetPerSecondSquared = 32.17404856;
		DefaultMeteresPerSecondSquaredToFeetPerSecondSquared = 3.280839895;
		Reset();
	}

	public static void Reset()
	{
		GToMetresPerSecondSquared = DefaultGToMetresPerSecondSquared;
		GToFeetPerSecondSquared = DefaultGToFeetPerSecondSquared;
		MeteresPerSecondSquaredToFeetPerSecondSquared = DefaultMeteresPerSecondSquaredToFeetPerSecondSquared;
	}

	public static double ConvertFromDefaultAccelerationUnit(double currentAccel, AccelerationUnit targetUnit)
	{
		return targetUnit switch
		{
			AccelerationUnit.FeetPerSecondSquared => currentAccel * GToFeetPerSecondSquared, 
			AccelerationUnit.MetresPerSecondSquared => currentAccel * GToMetresPerSecondSquared, 
			_ => currentAccel, 
		};
	}

	public static double ConvertToDefaultAccelerationUnit(double currentAcceleration, AccelerationUnit currentUnit)
	{
		return currentUnit switch
		{
			AccelerationUnit.FeetPerSecondSquared => currentAcceleration / GToFeetPerSecondSquared, 
			AccelerationUnit.MetresPerSecondSquared => currentAcceleration / GToMetresPerSecondSquared, 
			_ => currentAcceleration, 
		};
	}
}
