using System;
using System.Diagnostics;

namespace Racelogic.Geodetics
{
	[DebuggerDisplay("LeapSeconds:{Seconds}  UTC:{Utc}")]
	public readonly struct LeapSecond : IEquatable<LeapSecond>
	{
		private readonly DateTime utc;

		private readonly int seconds;

		private static readonly LeapSecond[] leapSeconds = new LeapSecond[19]
		{
			new LeapSecond(new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc), 0),
			new LeapSecond(new DateTime(1981, 7, 1, 0, 0, 0, DateTimeKind.Utc), 1),
			new LeapSecond(new DateTime(1982, 7, 1, 0, 0, 0, DateTimeKind.Utc), 2),
			new LeapSecond(new DateTime(1983, 7, 1, 0, 0, 0, DateTimeKind.Utc), 3),
			new LeapSecond(new DateTime(1985, 7, 1, 0, 0, 0, DateTimeKind.Utc), 4),
			new LeapSecond(new DateTime(1988, 1, 1, 0, 0, 0, DateTimeKind.Utc), 5),
			new LeapSecond(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), 6),
			new LeapSecond(new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc), 7),
			new LeapSecond(new DateTime(1992, 7, 1, 0, 0, 0, DateTimeKind.Utc), 8),
			new LeapSecond(new DateTime(1993, 7, 1, 0, 0, 0, DateTimeKind.Utc), 9),
			new LeapSecond(new DateTime(1994, 7, 1, 0, 0, 0, DateTimeKind.Utc), 10),
			new LeapSecond(new DateTime(1996, 1, 1, 0, 0, 0, DateTimeKind.Utc), 11),
			new LeapSecond(new DateTime(1997, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12),
			new LeapSecond(new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc), 13),
			new LeapSecond(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), 14),
			new LeapSecond(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc), 15),
			new LeapSecond(new DateTime(2012, 7, 1, 0, 0, 0, DateTimeKind.Utc), 16),
			new LeapSecond(new DateTime(2015, 7, 1, 0, 0, 0, DateTimeKind.Utc), 17),
			new LeapSecond(new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc), 18)
		};

		public int Seconds
		{
			[DebuggerStepThrough]
			get
			{
				return seconds;
			}
		}

		public DateTime Utc
		{
			[DebuggerStepThrough]
			get
			{
				return utc;
			}
		}

		public static int GpsLeapSecondsForBeiDouEpoch
		{
			[DebuggerStepThrough]
			get
			{
				return GpsLeapSecondsForDate(GnssTime.BeiDouEpoch);
			}
		}

		public static int GpsLeapSecondsForGalileoNavicEpoch
		{
			[DebuggerStepThrough]
			get
			{
				return GpsLeapSecondsForDate(GnssTime.GalileoNavicEpoch);
			}
		}

		private LeapSecond(DateTime utc, int leapSecondCount)
		{
			this.utc = utc;
			seconds = leapSecondCount;
		}

		public static int GpsLeapSecondsForDate(DateTime utcTime)
		{
			for (int num = leapSeconds.Length - 1; num >= 0; num--)
			{
				LeapSecond leapSecond = leapSeconds[num];
				if (utcTime >= leapSecond.Utc)
				{
					return leapSecond.Seconds;
				}
			}
			return 0;
		}

		public static int BeiDouLeapSecondsForDate(DateTime utcTime)
		{
			return GpsLeapSecondsForDate(utcTime) - GpsLeapSecondsForBeiDouEpoch;
		}

		public static int GalileoNavicLeapSecondsForDate(DateTime utcTime)
		{
			return GpsLeapSecondsForDate(utcTime) - GpsLeapSecondsForGalileoNavicEpoch;
		}

		public static LeapSecond LeapSecondsForDate(DateTime utcTime)
		{
			for (int num = leapSeconds.Length - 1; num >= 0; num--)
			{
				LeapSecond result = leapSeconds[num];
				if (utcTime >= result.Utc)
				{
					return result;
				}
			}
			return leapSeconds[0];
		}

		public static LeapSecond NextLeapSecondsAfterDate(DateTime utcTime)
		{
			for (int i = 0; i < leapSeconds.Length; i++)
			{
				LeapSecond result = leapSeconds[i];
				if (result.Utc > utcTime)
				{
					return result;
				}
			}
			return leapSeconds[leapSeconds.Length - 1];
		}

		public override bool Equals(object obj)
		{
			if (obj is LeapSecond)
			{
				LeapSecond other = (LeapSecond)obj;
				return Equals(other);
			}
			return false;
		}

		public bool Equals(LeapSecond other)
		{
			return seconds == other.seconds;
		}

		public static bool operator ==(LeapSecond left, LeapSecond right)
		{
			return left.seconds == right.seconds;
		}

		public static bool operator !=(LeapSecond left, LeapSecond right)
		{
			return left.seconds != right.seconds;
		}

		public override int GetHashCode()
		{
			return 5993773 + seconds.GetHashCode();
		}
	}
}
