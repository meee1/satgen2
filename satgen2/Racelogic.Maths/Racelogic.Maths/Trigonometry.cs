using System;

namespace Racelogic.Maths;

public static class Trigonometry
{
	public static double DegreesToRadians(double degrees)
	{
		return degrees * Math.PI / 180.0;
	}

	public static double RadiansToDegrees(double radians)
	{
		return radians * 180.0 / Math.PI;
	}

	public static double ToPrincipalRadians(double radians)
	{
		while (radians < 0.0)
		{
			radians += Math.PI * 2.0;
		}
		while (radians >= Math.PI * 2.0)
		{
			radians -= Math.PI * 2.0;
		}
		return radians;
	}
}
