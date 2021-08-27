using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics
{
	[JsonObject(MemberSerialization.OptIn)]
	[DebuggerDisplay("UTC{UtcTime} GpsW{GpsWeek} GpsS{(double)GpsSecondOfWeekDecimal} BdsW{BeiDouWeek} BdsS{(double)BeiDouSecondOfWeekDecimal} GLO{GlonassTime} GloD{GlonassFourYearPeriodDayNumber} GloS{(double)GlonassSecondOfDayDecimal} GalW{GalileoNavicWeek} GalS{(double)GalileoNavicSecondOfWeekDecimal}")]
	public readonly struct GnssTime : IEquatable<GnssTime>, IComparable<GnssTime>, IComparable, IFormattable
	{
		public const long NanosecondsPerSecond = 1000000000L;

		public const long NanosecondsPerMinute = 60000000000L;

		public const long NanosecondsPerHour = 3600000000000L;

		public const long NanosecondsPerDay = 86400000000000L;

		public const long NanosecondsPerWeek = 604800000000000L;

		public const long TicksPerNanosecond = 100L;

		public static readonly DateTime GpsEpoch = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

		public static readonly DateTime BeiDouEpoch = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static readonly DateTime GalileoNavicEpoch = new DateTime(1999, 8, 21, 23, 59, 47, DateTimeKind.Utc);

		public const long GlonassToUtcOffsetNanoseconds = 10800000000000L;

		public static readonly TimeSpan GlonassToUtcOffset = TimeSpan.FromHours(3.0);

		private static readonly long GalileoNavicToGpsOffsetNanoseconds = (GalileoNavicEpoch - GpsEpoch).Ticks * 100 + (long)LeapSecond.GpsLeapSecondsForDate(GalileoNavicEpoch) * 1000000000L;

		private static readonly long BeiDouToGpsOffsetNanoseconds = (BeiDouEpoch - GpsEpoch).Ticks * 100 + (long)LeapSecond.GpsLeapSecondsForDate(BeiDouEpoch) * 1000000000L;

		private static readonly DateTime MaxDate = new DateTime(2272, 4, 15, 23, 47, 0, 0, DateTimeKind.Utc);

		public const decimal NanosecondLengthDecimal = 0.000000001m;

		public const double NanosecondLength = 1E-09;

		public const long NanosecondsPerTick = 100L;

		public const int SecondsPerDay = 86400;

		public const int SecondsPerWeek = 604800;

		[JsonProperty(PropertyName = "Nanoseconds")]
		private readonly long Nanoseconds;

		public static GnssTime Now
		{
			[DebuggerStepThrough]
			get
			{
				return FromUtc(DateTime.UtcNow);
			}
		}

		public static GnssTime MinValue
		{
			[DebuggerStepThrough]
			get
			{
				return new GnssTime(0L);
			}
		}

		public static GnssTime MaxValue
		{
			[DebuggerStepThrough]
			get
			{
				return new GnssTime(long.MaxValue);
			}
		}

		public bool IsEmpty
		{
			[DebuggerStepThrough]
			get
			{
				return Nanoseconds == 0;
			}
		}

		public DateTime UtcTime
		{
			[DebuggerStepThrough]
			get
			{
				DateTime utcTime = GpsEpoch.AddTicks(Nanoseconds / 100);
				int num = LeapSecond.GpsLeapSecondsForDate(utcTime);
				DateTime dateTime = utcTime.AddSeconds(-num);
				int num2 = LeapSecond.GpsLeapSecondsForDate(dateTime);
				int num3 = num - num2;
				if (num3 != 0)
				{
					dateTime = dateTime.AddSeconds(num3);
				}
				return dateTime;
			}
		}

		public DateTime GpsTime
		{
			[DebuggerStepThrough]
			get
			{
				return GpsEpoch.AddTicks(Nanoseconds / 100);
			}
		}

		public int GpsWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)(Nanoseconds / 604800000000000L);
			}
		}

		public int GpsDayOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)(Nanoseconds % 604800000000000L / 86400000000000L);
			}
		}

		public int GpsSecondOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)(Nanoseconds % 604800000000000L / 1000000000);
			}
		}

		public decimal GpsSecondOfWeekDecimal
		{
			[DebuggerStepThrough]
			get
			{
				return (decimal)(Nanoseconds % 604800000000000L) * 0.000000001m;
			}
		}

		public DateTime GlonassTime
		{
			[DebuggerStepThrough]
			get
			{
				return UtcTime + GlonassToUtcOffset;
			}
		}

		public int GlonassFourYearPeriodNumber
		{
			[DebuggerStepThrough]
			get
			{
				return (GlonassTime.Year - 1996 >> 2) + 1;
			}
		}

		public int GlonassFourYearPeriodDayNumber
		{
			[DebuggerStepThrough]
			get
			{
				DateTime dateTime = new DateTime(1996 + (GlonassFourYearPeriodNumber - 1 << 2), 1, 1);
				return (int)(GlonassTime.Date - dateTime).TotalDays + 1;
			}
		}

		public int GlonassSecondOfDay
		{
			[DebuggerStepThrough]
			get
			{
				GnssTime gnssTime = FromGlonass(GlonassTime.Date);
				int num = (int)((Nanoseconds - gnssTime.Nanoseconds) / 1000000000);
				if (LeapSecond.LeapSecondsForDate(UtcTime).Seconds > LeapSecond.LeapSecondsForDate(gnssTime.UtcTime).Seconds)
				{
					num--;
				}
				return num;
			}
		}

		public decimal GlonassSecondOfDayDecimal
		{
			[DebuggerStepThrough]
			get
			{
				GnssTime gnssTime = FromGlonass(GlonassTime.Date);
				decimal result = (decimal)(Nanoseconds - gnssTime.Nanoseconds) * 0.000000001m;
				if (LeapSecond.LeapSecondsForDate(UtcTime).Seconds > LeapSecond.LeapSecondsForDate(gnssTime.UtcTime).Seconds)
				{
					--result;
				}
				return result;
			}
		}

		public DateTime BeiDouTime
		{
			[DebuggerStepThrough]
			get
			{
				DateTime gpsTime = GpsTime;
				return gpsTime.AddSeconds(-LeapSecond.BeiDouLeapSecondsForDate(gpsTime));
			}
		}

		public int BeiDouWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - BeiDouToGpsOffsetNanoseconds) / 604800000000000L);
			}
		}

		public int BeiDouDayOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - BeiDouToGpsOffsetNanoseconds) % 604800000000000L / 86400000000000L);
			}
		}

		public int BeiDouSecondOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - BeiDouToGpsOffsetNanoseconds) % 604800000000000L / 1000000000);
			}
		}

		public decimal BeiDouSecondOfWeekDecimal
		{
			[DebuggerStepThrough]
			get
			{
				return (decimal)((Nanoseconds - BeiDouToGpsOffsetNanoseconds) % 604800000000000L) * 0.000000001m;
			}
		}

		public DateTime GalileoNavicTime
		{
			[DebuggerStepThrough]
			get
			{
				DateTime gpsTime = GpsTime;
				return gpsTime.AddSeconds(-LeapSecond.GalileoNavicLeapSecondsForDate(gpsTime));
			}
		}

		public int GalileoNavicWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - GalileoNavicToGpsOffsetNanoseconds) / 604800000000000L);
			}
		}

		public int GalileoNavicDayOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - GalileoNavicToGpsOffsetNanoseconds) % 604800000000000L / 86400000000000L);
			}
		}

		public int GalileoNavicSecondOfWeek
		{
			[DebuggerStepThrough]
			get
			{
				return (int)((Nanoseconds - GalileoNavicToGpsOffsetNanoseconds) % 604800000000000L / 1000000000);
			}
		}

		public decimal GalileoNavicSecondOfWeekDecimal
		{
			[DebuggerStepThrough]
			get
			{
				return (decimal)((Nanoseconds - GalileoNavicToGpsOffsetNanoseconds) % 604800000000000L) * 0.000000001m;
			}
		}

		[JsonConstructor]
		public GnssTime(long nanoseconds)
		{
			Nanoseconds = nanoseconds;
		}

		public static GnssTime FromUtc(DateTime utcDateTime)
		{
			if (utcDateTime < GpsEpoch || utcDateTime > MaxDate)
			{
				throw new ArgumentOutOfRangeException("utcDateTime", "Cannot represent instants prior to the GPS epoch or after MaxDate.");
			}
			long num = (long)LeapSecond.GpsLeapSecondsForDate(utcDateTime) * 1000000000L;
			return new GnssTime((utcDateTime - GpsEpoch).Ticks * 100 + num);
		}

		public static GnssTime FromGps(DateTime gpsDateTime)
		{
			if (gpsDateTime < GpsEpoch || gpsDateTime > MaxDate)
			{
				throw new ArgumentOutOfRangeException("gpsDateTime", "Cannot represent instants prior to the GPS epoch or after MaxDate.");
			}
			return new GnssTime((gpsDateTime - GpsEpoch).Ticks * 100);
		}

		public static GnssTime FromGps(uint week, uint second)
		{
			return new GnssTime((long)week * 604800000000000L + (long)second * 1000000000L);
		}

		public static GnssTime FromGps(int week, int second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(week * 604800000000000L + (long)second * 1000000000L);
		}

		public static GnssTime FromGps(int week, double second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0.0)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(week * 604800000000000L + (long)(second * 1000000000.0));
		}

		public static GnssTime FromGps(int week, decimal second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0m)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(week * 604800000000000L + (long)(second * 1000000000m));
		}

		public static GnssTime FromGlonass(DateTime glonassDateTime)
		{
			return FromUtc(glonassDateTime - GlonassToUtcOffset);
		}

		public static GnssTime FromGlonass(int fourYearPeriodNumber, int dayNumber, double daySeconds)
		{
			DateTime glonassDateTime = new DateTime(1996 + (fourYearPeriodNumber - 1 << 2), 1, 1).AddDays(dayNumber - 1).AddSeconds(daySeconds);
			return FromGlonass(glonassDateTime);
		}

		public static GnssTime FromBeiDou(DateTime beiDouDateTime)
		{
			if (beiDouDateTime < BeiDouEpoch || beiDouDateTime > MaxDate)
			{
				throw new ArgumentOutOfRangeException("beiDouDateTime", "Cannot represent instants prior to the BeiDou epoch or after MaxDate.");
			}
			long num = (beiDouDateTime - BeiDouEpoch).Ticks * 100;
			return new GnssTime(BeiDouToGpsOffsetNanoseconds + num);
		}

		public static GnssTime FromBeiDou(uint week, uint second)
		{
			return new GnssTime(BeiDouToGpsOffsetNanoseconds + (long)week * 604800000000000L + (long)second * 1000000000L);
		}

		public static GnssTime FromBeiDou(int week, int second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(BeiDouToGpsOffsetNanoseconds + week * 604800000000000L + (long)second * 1000000000L);
		}

		public static GnssTime FromBeiDou(int week, double second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0.0)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(BeiDouToGpsOffsetNanoseconds + week * 604800000000000L + (long)(second * 1000000000.0));
		}

		public static GnssTime FromGalileoNavic(DateTime galileoNavicDateTime)
		{
			if (galileoNavicDateTime < GalileoNavicEpoch || galileoNavicDateTime > MaxDate)
			{
				throw new ArgumentOutOfRangeException("galileoNavicDateTime", "Cannot represent instants prior to the Galileo/Navic epoch or after MaxDate.");
			}
			long num = (galileoNavicDateTime - GalileoNavicEpoch).Ticks * 100;
			return new GnssTime(GalileoNavicToGpsOffsetNanoseconds + num);
		}

		public static GnssTime FromGalileoNavic(uint week, uint second)
		{
			return new GnssTime(GalileoNavicToGpsOffsetNanoseconds + (long)week * 604800000000000L + (long)second * 1000000000L);
		}

		public static GnssTime FromGalileoNavic(int week, int second)
		{
			if (week < 0)
			{
				throw new ArgumentOutOfRangeException("week", "Week number must be greater than or equal zero");
			}
			if (second < 0)
			{
				throw new ArgumentOutOfRangeException("second", "Number of seconds must be greater than or equal zero");
			}
			return new GnssTime(GalileoNavicToGpsOffsetNanoseconds + week * 604800000000000L + (long)second * 1000000000L);
		}

		public bool Equals(GnssTime other)
		{
			return Nanoseconds == other.Nanoseconds;
		}

		public override bool Equals(object obj)
		{
			if (obj is GnssTime)
			{
				GnssTime gnssTime = (GnssTime)obj;
				return Nanoseconds == gnssTime.Nanoseconds;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Nanoseconds.GetHashCode();
		}

		public int CompareTo(GnssTime other)
		{
			if (Nanoseconds > other.Nanoseconds)
			{
				return 1;
			}
			if (Nanoseconds < other.Nanoseconds)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(object obj)
		{
			if (!(obj is GnssTime))
			{
				return -1;
			}
			GnssTime other = (GnssTime)obj;
			return CompareTo(other);
		}

		public static GnssTime Add(GnssTime left, GnssTimeSpan right)
		{
			if (right.Nanoseconds < 0)
			{
				if (-right.Nanoseconds > left.Nanoseconds)
				{
					throw new InvalidOperationException("Cannot express dates prior to GPS Epoch");
				}
				return new GnssTime(left.Nanoseconds + right.Nanoseconds);
			}
			if (right.Nanoseconds > long.MaxValue - left.Nanoseconds)
			{
				throw new InvalidOperationException("GnssTime overflow");
			}
			return new GnssTime(left.Nanoseconds + right.Nanoseconds);
		}

		public static GnssTimeSpan Subtract(GnssTime left, GnssTime right)
		{
			return new GnssTimeSpan(left.Nanoseconds - right.Nanoseconds);
		}

		public static GnssTime Subtract(GnssTime left, GnssTimeSpan right)
		{
			return Add(left, new GnssTimeSpan(-right.Nanoseconds));
		}

		public static GnssTime operator +(GnssTime left, GnssTimeSpan right)
		{
			return Add(left, right);
		}

		public static GnssTimeSpan operator -(GnssTime left, GnssTime right)
		{
			return Subtract(left, right);
		}

		public static GnssTime operator -(GnssTime left, GnssTimeSpan right)
		{
			return Subtract(left, right);
		}

		public static GnssTime operator ++(GnssTime left)
		{
			return Add(left, GnssTimeSpan.FromSeconds(1));
		}

		public static GnssTime operator --(GnssTime left)
		{
			return Subtract(left, GnssTimeSpan.FromSeconds(1));
		}

		public static bool operator <(GnssTime left, GnssTime right)
		{
			return left.Nanoseconds < right.Nanoseconds;
		}

		public static bool operator <=(GnssTime left, GnssTime right)
		{
			if (!(left < right))
			{
				return left == right;
			}
			return true;
		}

		public static bool operator >(GnssTime left, GnssTime right)
		{
			return left.Nanoseconds > right.Nanoseconds;
		}

		public static bool operator >=(GnssTime left, GnssTime right)
		{
			if (!(left > right))
			{
				return left == right;
			}
			return true;
		}

		public static bool operator ==(GnssTime left, GnssTime right)
		{
			return left.Nanoseconds == right.Nanoseconds;
		}

		public static bool operator !=(GnssTime left, GnssTime right)
		{
			return left.Nanoseconds != right.Nanoseconds;
		}

		public override string ToString()
		{
			return UtcTime.ToString();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return UtcTime.ToString(format, formatProvider);
		}
	}
}
