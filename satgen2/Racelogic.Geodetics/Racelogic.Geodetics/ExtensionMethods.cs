using System;

namespace Racelogic.Geodetics;

public static class ExtensionMethods
{
	public static double ToDegrees(this double radians)
	{
		return radians * 57.295779513082316;
	}

	public static decimal ToDegrees(this decimal radians)
	{
		return radians * 57.295779513082320876798154814m;
	}

	public static double ToRadians(this double degrees)
	{
		return degrees * (Math.PI / 180.0);
	}

	public static decimal ToRadians(this decimal degrees)
	{
		return degrees * 0.0174532925199432957692369077m;
	}
}
