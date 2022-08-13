using System;
using System.Collections.Generic;
using System.Linq;

namespace Racelogic.DataSource;

[Obsolete("This class is about to be removed.  Please use LeapSecond defined in Racelogic.Geodetic.")]
public class GpsLeapSeconds
{
	private static List<GpsLeapSeconds> leapSeconds;

	public static List<GpsLeapSeconds> LeapSeconds => leapSeconds;

	public int Seconds { get; set; }

	public DateTime Utc { get; set; }

	static GpsLeapSeconds()
	{
		leapSeconds = new List<GpsLeapSeconds>();
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1981, 7, 1, 0, 0, 0, DateTimeKind.Utc), 1));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1982, 7, 1, 0, 0, 0, DateTimeKind.Utc), 2));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1983, 7, 1, 0, 0, 0, DateTimeKind.Utc), 3));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1985, 7, 1, 0, 0, 0, DateTimeKind.Utc), 4));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1988, 1, 1, 0, 0, 0, DateTimeKind.Utc), 5));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), 6));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc), 7));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1992, 7, 1, 0, 0, 0, DateTimeKind.Utc), 8));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1993, 7, 1, 0, 0, 0, DateTimeKind.Utc), 9));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1994, 7, 1, 0, 0, 0, DateTimeKind.Utc), 10));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1996, 1, 1, 0, 0, 0, DateTimeKind.Utc), 11));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1997, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc), 13));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), 14));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc), 15));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(2012, 7, 1, 0, 0, 0, DateTimeKind.Utc), 16));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(2015, 7, 1, 0, 0, 0, DateTimeKind.Utc), 17));
		leapSeconds.Add(new GpsLeapSeconds(new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc), 18));
	}

	private GpsLeapSeconds(DateTime utc, int leapSeconds)
	{
		Utc = utc;
		Seconds = leapSeconds;
	}

	public static int LeapSecondsForDate(DateTime utc)
	{
		return LeapSeconds.OrderByDescending((GpsLeapSeconds ls) => ls.Utc).FirstOrDefault((GpsLeapSeconds ls) => utc >= ls.Utc)?.Seconds ?? 0;
	}

	public static GpsLeapSeconds NextLeapSecondsAfterDate(DateTime utc)
	{
		return LeapSeconds.OrderBy((GpsLeapSeconds ls) => ls.Utc).FirstOrDefault((GpsLeapSeconds ls) => ls.Utc > utc) ?? LeapSeconds.Last();
	}
}
