using System.Diagnostics;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

internal static class NeQuickG
{
	private const double alpha0 = 63.7;

	private const double alpha1 = 0.0;

	private const double alpha2 = 0.0;

	public static double Alpha0
	{
		[DebuggerStepThrough]
		get
		{
			return 63.7;
		}
	}

	public static double Alpha1
	{
		[DebuggerStepThrough]
		get
		{
			return 0.0;
		}
	}

	public static double Alpha2
	{
		[DebuggerStepThrough]
		get
		{
			return 0.0;
		}
	}

	public static double GetIonosphericDelay(in GnssTime time, in Geodetic position, in Topocentric azimuthElevation, in double frequency)
	{
		double referenceFrequency = 1575420000.0;
		return Klobuchar.GetIonosphericDelay(in time, in position, in azimuthElevation, in frequency, in referenceFrequency);
	}
}
