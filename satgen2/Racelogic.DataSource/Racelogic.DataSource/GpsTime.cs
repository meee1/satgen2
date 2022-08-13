using System;
using System.Globalization;

namespace Racelogic.DataSource;

[Obsolete("This struct is about to be removed.  Please use GnssTime defined in Racelogic.Geodetic.")]
public struct GpsTime : IEquatable<GpsTime>
{
	public static readonly DateTime Epoch = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

	public static readonly GpsTime MinValue = new GpsTime(0, 0);

	private const int SecondsInAWeek = 604800;

	private static readonly decimal Epsilon = new decimal(1E-10);

	private readonly decimal secondsOfWeek;

	private readonly int week;

	public int Week => week;

	public decimal SecondsOfWeek => secondsOfWeek;

	public int GpsWeek => Week % 1024;

	public GpsTime(double seconds)
		: this(new decimal(seconds))
	{
	}

	public GpsTime(decimal seconds)
	{
		this = default(GpsTime);
		week = (int)(seconds / 604800m);
		secondsOfWeek = ((week == 0) ? seconds : (seconds % (decimal)(week * 604800)));
	}

	public GpsTime(double seconds, GpsTime reference)
		: this(new decimal(seconds), reference)
	{
	}

	public GpsTime(decimal seconds, GpsTime reference)
		: this(seconds)
	{
		week += reference.week;
		secondsOfWeek += reference.secondsOfWeek;
	}

	public GpsTime(int week, int secondsOfWeek)
		: this(week, new decimal(secondsOfWeek))
	{
	}

	public GpsTime(int week, double secondsOfWeek)
		: this(week, new decimal(secondsOfWeek))
	{
	}

	public GpsTime(int week, decimal secondsOfWeek)
	{
		this = default(GpsTime);
		this.week = week;
		this.secondsOfWeek = secondsOfWeek;
		while (this.secondsOfWeek >= 604800m)
		{
			this.secondsOfWeek -= 604800m;
			this.week++;
		}
		while (this.secondsOfWeek < 0m)
		{
			this.secondsOfWeek += 604800m;
			this.week--;
		}
	}

	public static GpsTime FromUtc(DateTime dateTime)
	{
		if (dateTime < Epoch)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "GPS Time cannot be before GPS Epoch ({0})", Epoch));
		}
		TimeSpan timeSpan = dateTime - Epoch;
		int num = (int)Math.Floor(timeSpan.TotalDays / 7.0);
		double totalSeconds = timeSpan.Subtract(TimeSpan.FromDays(num * 7)).Add(TimeSpan.FromSeconds(LeapSeconds(dateTime))).TotalSeconds;
		return new GpsTime(num, totalSeconds);
	}

	public static GpsTime operator +(GpsTime left, GpsTime right)
	{
		return new GpsTime(left.Week + right.Week + (int)((left.SecondsOfWeek + right.SecondsOfWeek) / 604800m), (left.SecondsOfWeek + right.SecondsOfWeek) % 604800m);
	}

	public static bool operator ==(GpsTime left, GpsTime right)
	{
		return left.Equals(right);
	}

	public static bool operator >(GpsTime left, GpsTime right)
	{
		if (left.Week <= right.Week)
		{
			if (left.Week == right.Week)
			{
				return left.SecondsOfWeek > right.SecondsOfWeek;
			}
			return false;
		}
		return true;
	}

	public static bool operator >=(GpsTime left, GpsTime right)
	{
		if (!(left > right))
		{
			return left == right;
		}
		return true;
	}

	public static bool operator !=(GpsTime left, GpsTime right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(GpsTime left, GpsTime right)
	{
		if (left.Week >= right.Week)
		{
			if (left.Week == right.Week)
			{
				return left.SecondsOfWeek < right.SecondsOfWeek;
			}
			return false;
		}
		return true;
	}

	public static bool operator <=(GpsTime left, GpsTime right)
	{
		if (!(left < right))
		{
			return left == right;
		}
		return true;
	}

	public static int Compare(GpsTime left, GpsTime right)
	{
		if (left < right)
		{
			return -1;
		}
		if (left > right)
		{
			return 1;
		}
		return 0;
	}

	public static GpsTime operator -(GpsTime left, GpsTime right)
	{
		decimal num = left.SecondsOfWeek - right.SecondsOfWeek;
		int num2 = left.Week - right.Week;
		while (num < 0m)
		{
			num += 604800m;
			num2--;
		}
		return new GpsTime(num2, num);
	}

	public GpsTime Add(GpsTime other)
	{
		return this + other;
	}

	public GpsTime Add(double dt)
	{
		return Add(new decimal(dt));
	}

	public GpsTime Add(decimal dt)
	{
		return new GpsTime(Week, SecondsOfWeek + dt);
	}

	public GpsTime Subtract(GpsTime other)
	{
		return this - other;
	}

	public bool Equals(GpsTime other)
	{
		if (other.Week == Week)
		{
			return Math.Abs(SecondsOfWeek - other.SecondsOfWeek) < Epsilon;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(GpsTime))
		{
			return false;
		}
		return Equals((GpsTime)obj);
	}

	public override int GetHashCode()
	{
		return (Week * 397) ^ SecondsOfWeek.GetHashCode();
	}

	public double SiderealTime()
	{
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (280.46061837 + 360.98564736629 * (Utc() - dateTime).TotalDays) % 360.0 * Math.PI / 180.0;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{{{0}:{1}}}", Week, SecondsOfWeek);
	}

	public decimal TotalSeconds()
	{
		return TotalSeconds(MinValue);
	}

	public decimal TotalSeconds(GpsTime fromGpsTime)
	{
		int num = Week - fromGpsTime.Week;
		decimal num2 = SecondsOfWeek - fromGpsTime.SecondsOfWeek;
		return (decimal)(num * 604800) + num2;
	}

	public DateTime Utc()
	{
		DateTime epoch = Epoch;
		DateTime dateTime = new DateTime(epoch.Ticks);
		dateTime = dateTime.AddDays(Week * 7).AddSeconds((double)SecondsOfWeek);
		return dateTime.AddSeconds(-1 * LeapSeconds(dateTime));
	}

	private static int LeapSeconds(DateTime utc)
	{
		return GpsLeapSeconds.LeapSecondsForDate(utc);
	}
}
